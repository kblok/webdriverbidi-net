namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CapabilitiesRequestInfoTests
{
    [Test]
    public void TestCanSerialize()
    {
        CapabilitiesRequestInfo capabilities = new();
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestCanSerializeWithBrowserName()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            BrowserName = "greatBrowser"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("browserName"));
            Assert.That(result["browserName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(result["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
        });
    }

    [Test]
    public void TestCanSerializeWithBrowserVersion()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            BrowserVersion = "101.5b"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("browserVersion"));
            Assert.That(result["browserVersion"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(result["browserVersion"]!.Value<string>(), Is.EqualTo("101.5b"));
        });
    }

    [Test]
    public void TestCanSerializeWithPlatformName()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            PlatformName = "oddOS"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("platformName"));
            Assert.That(result["platformName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(result["platformName"]!.Value<string>(), Is.EqualTo("oddOS"));
        });
    }

    [Test]
    public void TestCanSerializeWithAcceptInsecureCertificatesTrue()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            AcceptInsecureCertificates = true
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("acceptInsecureCerts"));
            Assert.That(result["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(result["acceptInsecureCerts"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeWithAcceptInsecureCertificatesFalse()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            AcceptInsecureCertificates = false
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("acceptInsecureCerts"));
            Assert.That(result["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(result["acceptInsecureCerts"]!.Value<bool>(), Is.False);
        });
    }

    [Test]
    public void TestCanSerializeWithUnhandledPromptBehavior()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            UnhandledPromptBehavior = new UserPromptHandler()
            {
                Alert = UserPromptHandlerType.Accept
            }
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("unhandledPromptBehavior"));
            Assert.That(result["unhandledPromptBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? proxyObject = result["unhandledPromptBehavior"] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(proxyObject, Has.Count.EqualTo(1));
            Assert.That(proxyObject, Contains.Key("alert"));
            Assert.That(proxyObject!["alert"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(proxyObject["alert"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithProxy()
    {
        CapabilitiesRequestInfo capabilities = new()
        {
            Proxy = new ManualProxyConfiguration() { HttpProxy = "http.proxy" }
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("proxy"));
            Assert.That(result["proxy"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? proxyObject = result["proxy"] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(proxyObject, Has.Count.EqualTo(2));
            Assert.That(proxyObject, Contains.Key("proxyType"));
            Assert.That(proxyObject!["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(proxyObject["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(proxyObject!, Contains.Key("httpProxy"));
            Assert.That(proxyObject!["httpProxy"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(proxyObject["httpProxy"]!.Value<string>(), Is.EqualTo("http.proxy"));
        });
    }

    [Test]
    public void TestCanSerializeWithAdditionalCapabilities()
    {
        CapabilitiesRequestInfo capabilities = new();
        capabilities.AdditionalCapabilities["capName"] = "capValue";
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("capName"));
            Assert.That(result["capName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(result["capName"]!.Value<string>(), Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanSerializeWithAdditionalCapabilitiesObject()
    {
        CapabilitiesRequestInfo capabilities = new();
        capabilities.AdditionalCapabilities["additional"] = new Dictionary<string, object?>() { { "capName", "capValue" } };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result, Contains.Key("additional"));
            Assert.That(result["additional"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? additionalObject = result["additional"] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(additionalObject, Has.Count.EqualTo(1));
            Assert.That(additionalObject!, Contains.Key("capName"));
            Assert.That(additionalObject!["capName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(additionalObject!["capName"]!.Value<string>(), Is.EqualTo("capValue"));
        });
    }
}
