using BinaryBundle;
using NUnit.Framework;

namespace UnitTests;

[BinaryBundle]
partial class SimpleClass {
    public int IntField;
}

public partial class BinarySerializationTest {

    [Test]
    public void SimpleSerialization() {
        SimpleClass @class = new SimpleClass {
            IntField = 3,
        };
        SimpleClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.IntField, deserializedClass.IntField);
    }

    [BinaryBundle]
    partial class MyNestedClass {
        public string StringField = "";
    }

    [Test]
    public void NestedClass() {
        MyNestedClass @class = new() {
            StringField = "hello"
        };

        MyNestedClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.StringField, deserializedClass.StringField);
    }

    [BinaryBundle]
    partial struct SimpleStruct {
        public byte ByteField;
    }

    [Test]
    public void Struct() {
        SimpleStruct @struct = new() {
            ByteField = 0xB0
        };

        SimpleStruct deserializedStruct = TestUtils.MakeSerializedCopy(@struct);

        Assert.AreEqual(@struct.ByteField, deserializedStruct.ByteField);
    }

    [BinaryBundle]
    partial class ClassWithManyTypes {
        public bool BoolField;
        public byte ByteField;
        public sbyte SByteField;
        public char CharField;
        public decimal DecimalField;
        public double DoubleField;
        public float FloatField;
        public int IntField;
        public uint UIntField;
        public long LongField;
        public ulong ULongField;
        public short ShortField;
        public ushort UShortField;
        public string? StringField;
    }

    [Test]
    public void AllPrimitiveTypes() {
        ClassWithManyTypes @class = new() {
            BoolField = true,
            ByteField = 0x13,
            SByteField = -0x13,
            CharField = 'V',
            DecimalField = 123.123M,
            DoubleField = 12.1234,
            FloatField = 156.4f,
            IntField = 123,
            UIntField = 125,
            LongField = 941_126_526_183_184,
            ULongField = 283_237_371_7817_637,
            ShortField = 123,
            UShortField = 821,
            StringField = "123",
        };

        ClassWithManyTypes deserializedClass = TestUtils.MakeSerializedCopy(@class);

        // Maybe I need a library for generating comparisons as well...
        Assert.AreEqual(@class.BoolField, @deserializedClass.BoolField);
        Assert.AreEqual(@class.ByteField, @deserializedClass.ByteField);
        Assert.AreEqual(@class.SByteField, @deserializedClass.SByteField);
        Assert.AreEqual(@class.CharField, @deserializedClass.CharField);
        Assert.AreEqual(@class.DecimalField, @deserializedClass.DecimalField);
        Assert.AreEqual(@class.DoubleField, @deserializedClass.DoubleField);
        Assert.AreEqual(@class.FloatField, @deserializedClass.FloatField);
        Assert.AreEqual(@class.IntField, @deserializedClass.IntField);
        Assert.AreEqual(@class.UIntField, @deserializedClass.UIntField);
        Assert.AreEqual(@class.LongField, @deserializedClass.LongField);
        Assert.AreEqual(@class.ULongField, @deserializedClass.ULongField);
        Assert.AreEqual(@class.ShortField, @deserializedClass.ShortField);
        Assert.AreEqual(@class.UShortField, @deserializedClass.UShortField);
        Assert.AreEqual(@class.StringField, @deserializedClass.StringField);
    }

    [BinaryBundle]
    partial class ClassWithIgnoredFields {
        [BundleIgnore]
        public string IgnoredString = "";

        public int IntField = 2;
    }

    [Test]
    public void IgnoreAttribute() {
        ClassWithIgnoredFields @class = new() {
            IgnoredString = "lol I sure hope I'm not set",
            IntField = 6
        };

        ClassWithIgnoredFields deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual("", deserializedClass.IgnoredString);
        Assert.AreNotEqual(@class.IgnoredString, deserializedClass.IgnoredString);
        Assert.AreEqual(@class.IntField, deserializedClass.IntField);
    }
}