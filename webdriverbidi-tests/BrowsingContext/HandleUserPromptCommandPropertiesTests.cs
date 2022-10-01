namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class HandleUserPromptCommandPropertiesTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new HandleUserPromptCommandProperties("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestCanSerializePropertiesWithAcceptTrue()
    {
        var properties = new HandleUserPromptCommandProperties("myContextId");
        properties.Accept = true;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("accept"));
        Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializePropertiesWithAcceptFalse()
    {
        var properties = new HandleUserPromptCommandProperties("myContextId");
        properties.Accept = false;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("accept"));
        Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(false));
    }

    [Test]
    public void TestCanSerializePropertiesWithUserText()
    {
        var properties = new HandleUserPromptCommandProperties("myContextId");
        properties.UserText = "myUserText";
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("userText"));
        Assert.That(serialized["userText"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["userText"]!.Value<string>(), Is.EqualTo("myUserText"));
    }
}