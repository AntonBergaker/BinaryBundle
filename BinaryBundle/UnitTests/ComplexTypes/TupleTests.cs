using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.ComplexTypes;
public partial class TupleTests {

    [BinaryBundle]
    private partial class TupleClass {
        public (string hello, string there) Tuple;
    }

    [Test]
    public void SimpleTuple() {
        var @class = new TupleClass() {
            Tuple = ("obi", "wan")
        };
        var serializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.Tuple, serializedClass.Tuple);
    }

    [BinaryBundle]
    private partial class NestedTupleClass {
        public ((int inside, (float me, decimal there), string are), (string two, byte wolves)) Tuple;
    }

    [Test]
    public void NestedTuple() {
        var @class = new NestedTupleClass() {
            Tuple = ((5, (123f, 0xF), "woof"), ("aoo", 2))
        };
        var serializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.Tuple, serializedClass.Tuple);
    }
}
