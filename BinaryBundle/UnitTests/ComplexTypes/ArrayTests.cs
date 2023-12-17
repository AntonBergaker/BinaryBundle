using BinaryBundle;
using NUnit.Framework;
using System;
using System.Linq;

namespace UnitTests.ComplexTypes;
public partial class ArrayTests {

    [BinaryBundle]
    private partial class ArrayClass {
        public byte[] ByteArray = Array.Empty<byte>();
    }

    [Test]
    public void SimpleArray() {
        ArrayClass @class = new() {
            ByteArray = new byte[] { 0x1, 0x2, 0x4 }
        };


        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.ByteArray, deserializedClass.ByteArray);
    }

    [Test]
    public void HugeArray() {
        ArrayClass @class = new() {
            ByteArray = Enumerable.Range(0, 0xFFFF).Select(x => (byte)x).ToArray()
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class, new byte[0xFFFFF]);
        Assert.AreEqual(@class.ByteArray, deserializedClass.ByteArray);
    }

    [BinaryBundle]
    private partial class ComplexArrayClass {
        public int[][] JaggedArray = Array.Empty<int[]>();
        public int[,] MultiDimensionalArray = new int[0, 0];
        public int[][,][] ScrewEverythingYouLoveArray = Array.Empty<int[,][]>();
    }

    [Test]
    public void ComplexArray() {
        ComplexArrayClass @class = new() {
            JaggedArray = new[] {
                new [] {3, 9, 183},
                new [] {1, 2, 3, 4, 5},
                new [] {2},
            },

            MultiDimensionalArray = new[,] {
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
}
