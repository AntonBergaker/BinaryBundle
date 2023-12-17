using BinaryBundle;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests.ComplexTypes;
public partial class ListTests {
    [BinaryBundle]
    private partial class ListClass {
        public List<int> List = new();

        public List<int[]> ListOfArrays = new();

        public List<List<string>> ListOfLists = new();
    }

    [Test]
    public void ComplexList() {
        ListClass @class = new() {
            List = { 13, 41, 64, 63, 85 },
            ListOfArrays = new() {
                new[] { 2, 3, 4 },
                new[] { 5, 6, 7 },
            },
            ListOfLists = new() {
                new() { "Kokosboll", "Vanilla", "Dorito" },
                new() { "Dossie", "Shroomy" },
            },
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.List, deserializedClass.List);
        Assert.AreEqual(@class.ListOfArrays, deserializedClass.ListOfArrays);
        Assert.AreEqual(@class.ListOfLists, deserializedClass.ListOfLists);
    }

    [BinaryBundle]
    private partial class ValueTypeList {
        [BinaryBundle]
        public partial struct MyValueType {
            public float X;
            public float Y;
        }

        public List<MyValueType> ValueTypes = new();
    }

    [Test]
    public void ValueTypeInList() {
        ValueTypeList @class = new() {
            ValueTypes = {
                new() {
                    X = 5,
                    Y = 4,
                },
                new() {
                    X = 128,
                    Y = 256,
                }
            }
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.ValueTypes.Count, deserializedClass.ValueTypes.Count);
        for (int i = 0; i < @class.ValueTypes.Count; i++) {
            Assert.AreEqual(@class.ValueTypes[i].X, deserializedClass.ValueTypes[i].X);
            Assert.AreEqual(@class.ValueTypes[i].Y, deserializedClass.ValueTypes[i].Y);
        }
    }

    [BinaryBundle]
    public partial class PropertyListClass {
        public List<int> Ints { get; private set; } = new();
    }

    [Test]
    public void PropertyList() {
        PropertyListClass @class = new() {
            Ints = { 13, 41, 64, 63, 85 },
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.Ints, deserializedClass.Ints);
    }
}
