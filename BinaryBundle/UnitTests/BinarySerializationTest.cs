using BinaryBundle;
using NUnit.Framework;

namespace UnitTests;

[BinaryBundle]
partial class SimpleClass {
    public int IntField;
}

public partial class BinarySerializationTest {

    [Test]
    public void TestSimpleSerialization() {
        SimpleClass @class = new SimpleClass {
            IntField = 3,
        };
        SimpleClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.IntField, deserializedClass.IntField);
    }

    [BinaryBundle]
    partial class NestedClass {
        public string StringField;
    }

    [Test]
    public void TestNestedClass() {
        NestedClass @class = new() {
            StringField = "hello"
        };

        NestedClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.StringField, deserializedClass.StringField);
    }

    [BinaryBundle]
    partial struct SimpleStruct {
        public byte ByteField;
    }

    [Test]
    public void TestStruct() {
        SimpleStruct @struct = new() {
            ByteField = 0xB0
        };

        SimpleStruct deserializedStruct = TestUtils.MakeSerializedCopy(@struct);

        Assert.AreEqual(@struct.ByteField, deserializedStruct.ByteField);
    }
}