using BinaryBundle;
using NUnit.Framework;
using System;

namespace UnitTests;
public partial class ConstructorTests {

    [BinaryBundle]
    public partial class IntWrapper {
        public int Int { get; private set; }

        public IntWrapper(int @int) {
            Int = @int;
        }
    }

    [BinaryBundle]
    public partial class IntWrapperWrapper {
        public IntWrapper? IntWrapper { get; set; }
    }

    [Test]
    public void ConstructSimpleClass() {
        var intWrapper = new IntWrapperWrapper() {
            IntWrapper = new(5)
        };
        var deserializedClass = TestUtils.MakeSerializedCopy(intWrapper);
        Assert.AreEqual(intWrapper.IntWrapper.Int, deserializedClass.IntWrapper?.Int);
    }

    [BinaryBundle]
    public partial class ClassArrayClass {
        public IntWrapper[] Wrappers { get; set; } = [];

    }

    [Test]
    public void ArrayOfClasses() {
        var intArray = new ClassArrayClass() {
            Wrappers = [new(5), new(123), new(523)]
        };
        var deserializedClass = TestUtils.MakeSerializedCopy(intArray);
        Assert.AreEqual(intArray.Wrappers.Length, deserializedClass.Wrappers.Length);
        Assert.AreEqual(intArray.Wrappers[0].Int, deserializedClass.Wrappers[0].Int);
    }

    [BinaryBundle]
    public partial class PrimaryConstructorClass(int myInt, string myString)
    {
        public int MyInt { get; private set; } = myInt;
        public string MyString { get; private set; } = myString;
    }

    [Test]
    public void PrimaryConstructor() {
        var simpleConstructor = new PrimaryConstructorClass(123, "hello");

        var buffer = new byte[0x20];
        BundleDefaultWriter writer = new BundleDefaultWriter(buffer);

        simpleConstructor.Serialize(writer);

        BundleDefaultReader reader = new BundleDefaultReader(buffer);
        var constructed = PrimaryConstructorClass.ConstructFromBuffer(reader);
        Assert.AreEqual(simpleConstructor.MyInt, constructed.MyInt);
        Assert.AreEqual(simpleConstructor.MyString, constructed.MyString);
    }
}
