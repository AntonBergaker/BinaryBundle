using BinaryBundle;

namespace UnitTests.CustomClasses; 

internal static class TestUtils {

    public static T MakeSerializedCopy<T>(T instance) where T : ICustomInterface, new() {
        StringWriter writer = new StringWriter();

        instance.Serialize(writer);

        T deserializedClass = new T();
        StringReader reader = new StringReader(writer.ToString());
        deserializedClass.Deserialize(reader);

        return deserializedClass;
    }
}