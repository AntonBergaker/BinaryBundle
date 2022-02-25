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

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

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

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.SerializableField.IntField, deserializedClass.SerializableField.IntField);
    }

    [BinaryBundle]
    private partial class ArrayClass {
        public byte[] ByteArray = Array.Empty<byte>();
    }

    [Test]
    public void TestArray() {
        ArrayClass @class = new() {
            ByteArray = new byte[] { 0x1, 0x2, 0x4 }
        };


        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.ByteArray, deserializedClass.ByteArray);
    }

    [BinaryBundle]
    private partial class ComplexArrayClass {
        public int[][] JaggedArray = Array.Empty<int[]>();
        public int[,] MultiDimensionalArray = new int[0, 0];
        public int[][,][] ScrewEverythingYouLoveArray = Array.Empty<int[,][]>();
    }

    [Test]
    public void TestComplexArray() {
        ComplexArrayClass @class = new() {
            JaggedArray = new [] {
                new [] {3, 9, 183},
                new [] {1, 2, 3, 4, 5},
                new [] {2},
            },

            MultiDimensionalArray = new [,] {
                {2, 4},
                {8, 16}
            },

            ScrewEverythingYouLoveArray = new int[][,][] {
                new int[,][] {
                    {
                        new[] {52, 34}, new[] {73}
                    },
                    {
                        new[] {54}, new int[] {}
                    },
                },
                new int[,][] {
                    {
                        new [] {43},
                        new [] {52},
                        new [] {83, 363, 634},
                    }
                }
            }
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.JaggedArray, deserializedClass.JaggedArray);
        Assert.AreEqual(@class.MultiDimensionalArray, deserializedClass.MultiDimensionalArray);
        Assert.AreEqual(@class.ScrewEverythingYouLoveArray, deserializedClass.ScrewEverythingYouLoveArray);
    }

    [BinaryBundle]
    private partial class ListClass {
        public List<int> List = new();

        public List<int[]> ListOfArrays = new();

        public List<List<string>> ListOfLists = new();
    }

    [Test]
    public void TestList() {
        ListClass @class = new() {
            List = { 13, 41, 64, 63, 85 },
            ListOfArrays = new() {
                new[] {2, 3, 4},
                new[] { 5, 6, 7},
            },
            ListOfLists = new() {
                new() {"Kokosboll", "Vanilla", "Dorito"},
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
    public void TestValueTypeInList() {
        ValueTypeList @class = new() {
            ValueTypes = new() {
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
}