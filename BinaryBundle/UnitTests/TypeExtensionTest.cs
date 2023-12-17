using System.Numerics;
using BinaryBundle;
using NUnit.Framework;

namespace UnitTests; 

internal partial class TypeExtensionTest {
    [BinaryBundle]
    public partial class VectorClass {
        public Vector2 VectorField;
    }

    [Test]
    public void TestConvertedType() {
        VectorClass @class = new() {
            VectorField = new Vector2(5, 15),
        };

        VectorClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.VectorField, deserializedClass.VectorField);
    }
}

static class VectorSerializeExtension {
    [BundleSerializeTypeExtension]
    public static void WriteVector2(this BundleDefaultWriter writer, Vector2 vector) {
        writer.WriteFloat(vector.X);
        writer.WriteFloat(vector.Y);
    }

    [BundleDeserializeTypeExtension]
    public static Vector2 ReadVector2(this BundleDefaultReader reader) {
        float x = reader.ReadFloat();
        float y = reader.ReadFloat();
        return new Vector2(x, y);
    }
}