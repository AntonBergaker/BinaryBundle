using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.ComplexTypes;
public partial class SerializableFieldsTests {
    [BinaryBundle]
    private partial class BundleSerializableFieldClass {
        [BinaryBundle]
        public partial class InnerClass {
            public int IntField;
        }

        public InnerClass SerializableField = new();
    }

    [Test]
    public void SerializableField() {
        BundleSerializableFieldClass @class = new() {
            SerializableField = {
                IntField = 5
            }
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.SerializableField.IntField, deserializedClass.SerializableField.IntField);
    }
}
