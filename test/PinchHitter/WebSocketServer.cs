// <copyright file="WebSocketServer.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Buffers.Binary;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A simple in-memory web server that can serve have its protocol upgraded from
/// HTTP to using a WebSocket. This server uses a TcpListener instead of an
/// HttpListener so as to avoid the restriction of having to register non-localhost
/// prefixes as an admin on Windows.
/// </summary>
public class WebSocketServer : Server
{
    private static readonly string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private static readonly byte ParityBit = 0x80;
    private readonly HttpRequestProcessor httpProcessor = new();
    private WebSocketState state = WebSocketState.None;
    private bool ignoreCloseRequest = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a random port.
    /// </summary>
    public WebSocketServer()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    public WebSocketServer(int port)
        : base(port)
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the server should ignore requests from the client to close the WebSocket.
    /// </summary>
    public bool IgnoreCloseRequest { get => this.ignoreCloseRequest; set => this.ignoreCloseRequest = value; }

    /// <summary>
    /// Gets a value indicating whether the server should continue listening for incoming connections.
    /// </summary>
    protected override bool ContinueRunning
    {
        get { return base.ContinueRunning && this.state != WebSocketState.Closed; }
    }

    /// <summary>
    /// Asynchronously sends data to the connected WebSocket client.
    /// </summary>
    /// <param name="data">A string representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task SendData(string data)
    {
        WebSocketFrameData frame = this.EncodeData(data);
        await this.SendData(frame.Data);
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task Disconnect()
    {
        if (this.HasClientSocket && this.state == WebSocketState.Open)
        {
            await this.SendCloseFrame("Initiating close");
            this.state = WebSocketState.CloseSent;
        }
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ProcessIncomingData(byte[] buffer, int receivedLength)
    {
        this.LogMessage($"RECV {receivedLength} bytes");
        if (this.state == WebSocketState.None)
        {
            // A WebSocket connection has not yet been established. Treat the
            // incoming data as a standard HTTP request. If the HTTP request
            // is a request to upgrade the connection, we will handle sending
            // the expected response in ProcessHttpRequest.
            await this.ProcessHttpRequest(buffer, receivedLength);
        }
        else
        {
            // Note: We do not handle continuation frames (WebSocketOpcodeType.Fragment)
            // in this implementation. Consider it a feature for a future iteration.
            // Likewise, we do not handle non-text frames (WebSocketOpcodeType.Binary)
            // in this implementation.
            // Finally, we do not handle ping and pong frames.
            WebSocketFrameData frame = this.DecodeData(buffer);
            if (frame.Opcode == WebSocketOpcodeType.Text)
            {
                string text = Encoding.UTF8.GetString(frame.Data);
                this.OnDataReceived(new ServerDataReceivedEventArgs(text));
            }

            if (frame.Opcode == WebSocketOpcodeType.ClosedConnection)
            {
                if (this.state == WebSocketState.Open && !this.ignoreCloseRequest)
                {
                    this.state = WebSocketState.CloseReceived;
                    await this.SendCloseFrame("Acknowledge close");
                }

                this.state = WebSocketState.Closed;
            }
        }
    }

    private static bool TryCreateWebSocketHandshakeResponse(HttpRequest request, out HttpResponse response)
    {
        if (request.Headers.ContainsKey("Connection") && request.Headers["Connection"].Contains("Upgrade") &&
            request.Headers.ContainsKey("Upgrade") && request.Headers["Upgrade"].Contains("websocket") &&
            request.Headers.ContainsKey("Sec-WebSocket-Key"))
        {
            // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
            // 3. Compute SHA-1 and Base64 hash of the new value
            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
            string websocketSecureKey = request.Headers["Sec-WebSocket-Key"][0];
            string websocketSecureResponse = websocketSecureKey + WebSocketGuid;
            byte[] websocketResponseHash = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(websocketSecureResponse));
            string websocketHashBase64 = Convert.ToBase64String(websocketResponseHash);

            WebResource resource = new(string.Empty);
            response = resource.CreateHttpResponse(HttpStatusCode.SwitchingProtocols);
            response.Headers["Connection"] = new List<string>() { "Upgrade" };
            response.Headers["Upgrade"] = new List<string>() { "websocket" };
            response.Headers["Sec-WebSocket-Accept"] = new List<string>() { websocketHashBase64 };
            return true;
        }

        response = HttpResponse.InvalidResponse;
        return false;
    }

    private async Task ProcessHttpRequest(byte[] buffer, int receivedLength)
    {
        string rawRequest = Encoding.UTF8.GetString(buffer, 0, receivedLength);
        HttpRequest request = HttpRequest.Parse(rawRequest);
        if (!TryCreateWebSocketHandshakeResponse(request, out HttpResponse response))
        {
            // The request was not a request to upgrade to a WebSocket
            // connection. Treat it like a normal HTTP request.
            response = this.httpProcessor.ProcessRequest(request);
            await this.SendData(response.ToByteArray());
            return;
        }

        this.state = WebSocketState.Connecting;
        await this.SendData(response.ToByteArray());
        this.state = WebSocketState.Open;
    }

    private async Task SendCloseFrame(string message)
    {
        WebSocketFrameData closeFrame = this.EncodeData(message, WebSocketOpcodeType.ClosedConnection);
        await this.SendData(closeFrame.Data);
    }

    private WebSocketFrameData DecodeData(byte[] buffer)
    {
        int keyOffset = 0;
        const byte opcodeMask = 0x0F;
        const byte messageLengthMask = 0x7F;
        WebSocketOpcodeType opcode = (WebSocketOpcodeType)(buffer[0] & opcodeMask);
        this.LogMessage($"Decoding data with opcode {opcode}");

        long messageLength = Convert.ToInt64(buffer[1] & messageLengthMask);

        if (messageLength <= 125)
        {
            keyOffset = 2;
        }

        if (messageLength == 126)
        {
            ReadOnlySpan<byte> messageLengthSpan = new(buffer, 2, sizeof(short));
            messageLength = BinaryPrimitives.ReadInt16BigEndian(messageLengthSpan);
            keyOffset = 4;
        }

        if (messageLength == 127)
        {
            ReadOnlySpan<byte> messageLengthSpan = new(buffer, 2, sizeof(long));
            messageLength = BinaryPrimitives.ReadInt64BigEndian(messageLengthSpan);
            keyOffset = 10;
        }

        byte[] decoded = new byte[messageLength];
        ArraySegment<byte> key = new(buffer, keyOffset, 4);
        long offset = Convert.ToInt64(keyOffset + key.Count);
        for (long index = 0; index < messageLength; index++)
        {
            decoded[index] = Convert.ToByte(buffer[offset + index] ^ key[Convert.ToInt32(index % 4)]);
        }

        return new WebSocketFrameData(opcode, decoded);
    }

    private WebSocketFrameData EncodeData(string data, WebSocketOpcodeType opcode = WebSocketOpcodeType.Text)
    {
        this.LogMessage($"Encoding data with opcode {opcode}");
        byte opcodeByte = Convert.ToByte(Convert.ToByte(opcode) | ParityBit);
        if (opcode == WebSocketOpcodeType.ClosedConnection)
        {
            // NOTE: Hard code the close frame data.
            return new WebSocketFrameData(opcode, new byte[] { opcodeByte, 0x00 });
        }

        long dataOffset = -1;
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        long length = dataBytes.LongLength;

        byte[] frameHeader = new byte[10];
        frameHeader[0] = opcodeByte;

        if (length <= 125)
        {
            frameHeader[1] = Convert.ToByte(length);
            dataOffset = 2;
        }

        if (length >= 126 && length <= 65535)
        {
            frameHeader[1] = 126;
            Span<byte> messageLengthSpan = new(frameHeader, 2, sizeof(short));
            BinaryPrimitives.WriteInt16BigEndian(messageLengthSpan, Convert.ToInt16(length));
            dataOffset = 4;
        }

        if (length >= 65536)
        {
            frameHeader[1] = 127;
            Span<byte> messageLengthSpan = new(frameHeader, 2, sizeof(long));
            BinaryPrimitives.WriteInt64BigEndian(messageLengthSpan, length);
            dataOffset = 10;
        }

        byte[] buffer = new byte[dataOffset + length];
        frameHeader.CopyTo(buffer, 0);
        dataBytes.CopyTo(buffer, dataOffset);
        return new WebSocketFrameData(opcode, buffer);
    }
}