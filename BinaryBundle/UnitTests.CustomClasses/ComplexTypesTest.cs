using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.CustomClasses; 

internal partial class ComplexTypesTest {
    [BinaryBundle]
    public partial class ArrayClass {
        public int[] IntArray;
    }


    [Test]
    public void TestArray() {
        ArrayClass @class = new() {
            IntArray = new[] { 0x1, 0x2, 0x4 }
        };


        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.IntArray, deserializedClass.IntArray);

    }
}