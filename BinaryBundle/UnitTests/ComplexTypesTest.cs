using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryBundle;
using NUnit.Framework;

namespace UnitTests; 

internal partial class ComplexTypesTest {
    private enum MyEnum {
        Entry0,
        Entry1,
        Entry2,
    }
    
    [BinaryBundle]
    private partial class EnumClass {
        public MyEnum EnumField;
    }

    [Test]
    public void TestEnum() {
        EnumClass @class = new() {
            EnumField = MyEnum.Entry1
        };

        EnumClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.EnumField, deserializedClass.EnumField);
    }

    [BinaryBundle]
    private partial class BundleSerializableFieldClass {
        [BinaryBundle]
        public partial class InnerClass {
            public int IntField;
        }

        public InnerClass SerializableField = new();
    }

    [Test]
    public void TestSerializableField() {
        BundleSerializableFieldClass @class = new() {
            SerializableField = {
                IntField = 5
            }
        };

        BundleSerializableFieldClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.SerializableField.IntField, deserializedClass.SerializableField.IntField);
    }

    [BinaryBundle]
    private partial class ArrayClass {
        public byte[] ByteArray;
    }

    [Test]
    public void TestArray() {
        ArrayClass @class = new() {
            ByteArray = new byte[] { 0x1, 0x2, 0x4 }
        };


        ArrayClass deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.ByteArray, deserializedClass.ByteArray);
    }
}