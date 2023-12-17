using BinaryBundle;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests.ComplexTypes;
public partial class DictionaryTests {
    [BinaryBundle]
    private partial class DictionaryClass {
        public readonly Dictionary<string, int> Dictionary = new();
    }

    [Test]
    public void TestDictionary() {
        DictionaryClass @class = new() {
            Dictionary = {
                {"Nice", 69},
                {"Leet", 1337},
                {"Moe's discovery", 5318008},
                {"Blaze", 420}
            }
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.Dictionary, deserializedClass.Dictionary);

    }
}
