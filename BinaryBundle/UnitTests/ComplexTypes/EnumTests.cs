using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.ComplexTypes;
public partial class EnumTests {
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
    public void SimpleEnum() {
        EnumClass @class = new() {
            EnumField = MyEnum.Entry1
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.EnumField, deserializedClass.EnumField);
    }
}
