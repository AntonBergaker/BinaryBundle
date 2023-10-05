using BinaryBundle;
using NUnit.Framework;
using System.Reflection;

namespace UnitTests; 

internal partial class PropertiesTest {

    [BinaryBundle]
    public partial class SimplePropertyClass {
        public string? StringProperty { get; set; }
    }

    [Test]
    public void TestSimpleProperty() {
        SimplePropertyClass @class = new() {
            StringProperty = "Hello there",
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.StringProperty, deserializedClass.StringProperty);
    }

    [BinaryBundle]
    public partial class PrivateSetterClass {
        public int IntProperty { get; private set; }

        public void SetData(int value) {
            IntProperty = value;
        }
    }

    [Test]
    public void TestPrivateSetter() {
        PrivateSetterClass @class = new();
        @class.SetData(0x420);

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.IntProperty, deserializedClass.IntProperty);
    }


    [BinaryBundle]
    public partial class BackedFieldClass {
        private int intProperty;

        public bool FailOnPropertyAccess = false;

        public int IntProperty {
            get {
                if (FailOnPropertyAccess) {
                    Assert.Fail();
                }

                return intProperty;
            }
            set {
                if (FailOnPropertyAccess) {
                    Assert.Fail();
                }

                intProperty = value;
            }
        }
    }

    [Test]
    public void TestBackedField() {
        BackedFieldClass @class = new() {
            IntProperty = 69
        };

        byte[] buffer = new byte[0xFF];

        BufferWriter writer = new BufferWriter(buffer);
        @class.FailOnPropertyAccess = true;
        @class.Serialize(writer);
        @class.FailOnPropertyAccess = false;

        BackedFieldClass deserializedClass = new();

        BufferReader reader = new BufferReader(buffer);

        deserializedClass.FailOnPropertyAccess = true;
        deserializedClass.Deserialize(reader);
        deserializedClass.FailOnPropertyAccess = false;

        Assert.AreEqual(@class.IntProperty, deserializedClass.IntProperty);
    }

    [BinaryBundle]
    partial class StructInProperty {
        [BinaryBundle]
        public partial struct ValueType {
            public int IntField;
        }

        public ValueType Value { get; set; }
    }

    [Test]
    public void TestStructInProperty() {
        StructInProperty @class = new() {
            Value = new() {
                IntField = 6
            }
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.Value.IntField, deserializedClass.Value.IntField);
    }


    [BinaryBundle]
    partial class ReadOnlyPropertyClass {
        public int MyInt => 1;
    }

    [Test]
    public void TestGetOnlyProperty() {
        ReadOnlyPropertyClass @class = new();

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(1, deserializedClass.MyInt);
    }
}