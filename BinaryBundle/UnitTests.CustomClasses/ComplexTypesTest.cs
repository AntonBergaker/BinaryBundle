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
        

    }
}