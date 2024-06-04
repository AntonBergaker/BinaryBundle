using BinaryBundle;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;

namespace UnitTests.ComplexTypes;
public partial class ArrayTests {

    [BinaryBundle]
    private partial class ArrayClass {
        public byte[] ByteArray = [];
    }

    [Test]
    public void SimpleArray() {
        ArrayClass @class = new() {
            ByteArray = [0x1, 0x2, 0x4]
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
        public int[][] JaggedArray = [];
        public int[,] MultiDimensionalArray = new int[0, 0];
        public int[][,][] ScrewEverythingYouLoveArray = [];
    }

    [Test]
    public void ComplexArray() {
        ComplexArrayClass @class = new() {
            JaggedArray = [
                [3, 9, 183],
                [1, 2, 3, 4, 5],
                [2],
            ],

            MultiDimensionalArray = new[,] {
                {2, 4},
                {8, 16}
            },

            ScrewEverythingYouLoveArray = [
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
            ]
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);

        Assert.AreEqual(@class.JaggedArray, deserializedClass.JaggedArray);
        Assert.AreEqual(@class.MultiDimensionalArray, deserializedClass.MultiDimensionalArray);
        Assert.AreEqual(@class.ScrewEverythingYouLoveArray, deserializedClass.ScrewEverythingYouLoveArray);
    }

    [BinaryBundle]
    private partial class LimitedArrayClass {
        [BundleLimit(5)]
        public int[] array = []; 
    }

    [BinaryBundle]
    private partial class LimitedClampArrayClass {
        [BundleLimit(5, BundleLimitBehavior.Clamp)]
        public int[] array = [];
    }

    [BinaryBundle]
    private partial class UnlimitedArrayClass {
        public int[] array = [];
    }

    [Test]
    public void ArrayLimits() {
        /*
        {
            var @class = new LimitedArrayClass() {
                array = [0, 1, 2, 3, 4]
            };
            var deserializedClass = TestUtils.MakeSerializedCopy(@class);

            // Just don't throw lol
        }


        {
            byte[] buffer = new byte[0xFF];

            BundleDefaultWriter writer = new BundleDefaultWriter(buffer);
            var @class = new LimitedArrayClass() {
                array = [0, 1, 2, 3, 4, 5]
            };

            // Throws on serialization
            Assert.Throws<BundleCollectionLimitExceededException>(() => {
                @class.Serialize(writer);
            });
        }


        {
            var @class = new LimitedClampArrayClass() {
                array = [0, 1, 2, 3, 4, 5, 6, 7]
            };
            var deserializedClass = TestUtils.MakeSerializedCopy(@class);

            Assert.AreEqual(5, @class.array.Length);
        }

        {
            byte[] buffer = new byte[0xFF];

            BundleDefaultWriter writer = new BundleDefaultWriter(buffer);
            var @class = new UnlimitedArrayClass() {
                array = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
            };

            @class.Serialize(writer);

            // Throws on deserialization because the array is too big. Even when safe.
            Assert.Throws<BundleCollectionLimitExceededException>(() => {
                var reader = new BundleDefaultReader(buffer);
                var limited = new LimitedArrayClass();
                limited.Deserialize(reader);
            });
            Assert.Throws<BundleCollectionLimitExceededException>(() => {
                var reader = new BundleDefaultReader(buffer);
                var limitedClamp = new LimitedClampArrayClass();
                limitedClamp.Deserialize(reader);
            });
        }*/
    }
}
