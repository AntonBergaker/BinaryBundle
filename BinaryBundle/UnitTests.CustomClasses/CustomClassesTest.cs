using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.CustomClasses; 

internal partial class CustomClassesTest {

    [BinaryBundle]
    private partial class SimpleClass {

        public string StringField = "";
    }
    
    [Test]
    public void TestCustomWriter() {
        SimpleClass @class = new() {
            StringField = "Hello there"
        };

        StringWriter writer = new StringWriter();
        @class.Serialize(writer);

        Assert.AreEqual("Hello there,", writer.ToString());
    }

    [Test]
    public void TestCustomReader() {
        SimpleClass deserializedClass = new();
        StringReader reader = new StringReader("Nice weather today,");
        deserializedClass.Deserialize(reader);

        Assert.AreEqual("Nice weather today", deserializedClass.StringField);
    }
}