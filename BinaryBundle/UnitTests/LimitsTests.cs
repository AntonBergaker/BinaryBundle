using BinaryBundle;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests;
internal partial class LimitsTests {

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

            Assert.AreEqual(5, deserializedClass.array.Length);
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
        }
    }

    [BinaryBundle]
    private partial class LimitedDictionaryClass {
        [BundleLimit(3, BundleLimitBehavior.Clamp)]
        public Dictionary<int, string> Dictionary = new();

        public int NextThing;
    }

    [Test]
    public void ClampedDictionary() {
        var @class = new LimitedDictionaryClass() {
            NextThing = 123
        };
        @class.Dictionary.Add(0, "littlemint");
        @class.Dictionary.Add(2, "mint");
        @class.Dictionary.Add(4, "gigamint");
        @class.Dictionary.Add(6, "omegamint");

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(3, deserializedClass.Dictionary.Count);
        Assert.AreEqual(@class.NextThing, deserializedClass.NextThing);
    }
}
