namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class NewCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""sessionId"": ""mySession"", ""capabilities"": { ""unhandledPromptBehavior"": { ""alert"" : ""dismiss""}, ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""manual"", ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" } }";
        NewCommandResult? result = JsonSerializer.Deserialize<NewCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.SessionId, Is.EqualTo("mySession"));
            Assert.That(result.Capabilities.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.Capabilities.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.Capabilities.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Capabilities.UnhandledPromptBehavior!.Alert, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(result.Capabilities.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result.Capabilities.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result.Capabilities.Proxy, Is.Not.Null);
            ProxyConfigurationResult proxyResult = result.Capabilities.Proxy!;
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.Manual));
            Assert.That(proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>().HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(result.Capabilities.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.Capabilities.AdditionalCapabilities, Has.Count.EqualTo(1));
            Assert.That(result.Capabilities.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.Capabilities.AdditionalCapabilities["capName"], Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }
    
    [Test]
    public void TestSupportsUnknownUnhandledPromptBehavior()
    {
        string json = @"{ ""sessionId"": ""mySession"", ""capabilities"": { ""unhandledPromptBehavior"": { ""alert"" : ""something invalid""}, ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""manual"", ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" } }";
        NewCommandResult? result = JsonSerializer.Deserialize<NewCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Capabilities.UnhandledPromptBehavior!.Alert, Is.EqualTo(UserPromptHandlerType.Unknown));
        });
    }

    [Test]
    public void TestDeserializingWithMissingSessionIdThrows()
    {
        string json = @"{ ""capabilities"": { ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" } }";
        Assert.That(() => JsonSerializer.Deserialize<NewCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingCapabilitiesThrows()
    {
        string json = @"{ ""sessionId"": ""mySession"" }";
        Assert.That(() => JsonSerializer.Deserialize<NewCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
