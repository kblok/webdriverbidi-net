// <copyright file="BluetoothManufacturerData.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a Bluetooth device manufacturer.
/// </summary>
public class BluetoothManufacturerData
{
    private uint key = 0;
    private string data = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BluetoothManufacturerData"/> class.
    /// </summary>
    /// <param name="key">A unique integer defining the Company Identifier Code for the manufacturer.</param>
    /// <param name="data">A byte data sequence representing the manufacturer data as a base64-encoded string.</param>
    public BluetoothManufacturerData(uint key, string data)
    {
        this.key = key;
        this.data = data;
    }

    /// <summary>
    /// Gets or sets the Company Identifier Code for the manufacturer.
    /// </summary>
    [JsonPropertyName("key")]
    [JsonRequired]
    [JsonInclude]
    public uint Key { get => this.key; set => this.key = value; }

    /// <summary>
    /// Gets or sets the manufacturer data byte sequence as a base64-encoded string.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonRequired]
    [JsonInclude]
    public string Data { get => this.data; set => this.data = value; }
}
