using System;
using BinaryBundle;
using NUnit.Framework;

namespace UnitTests; 

internal class HelpersTest {

    [Test]
    public void TestCollectionSizeWritingRandomNumbers() {

        Random random = new();
        byte[] buffer = new byte[0x8];
        BufferWriter writer = new(buffer);
        BufferReader reader = new(buffer);

        for (int i = 0; i < 1000; i++) {
            writer.ResetPosition();
            reader.ResetPosition();

            int value = random.Next((1 << 28) - 1);
            BinaryBundleHelpers.WriteCollectionSize(writer, value);
            int readValue = BinaryBundleHelpers.ReadCollectionSize(reader);

            Assert.AreEqual(value, readValue);
        }
    }

    private readonly (byte[] rawBytes, int value)[] knownNumbers = {
        (new byte[] {0b0_0000001}, 0b0000001),
        (new byte[] {0b1_0010001, 0b0_0000001}, 0b0010001_0000001),
        (new byte[] {0b1_0010001, 0b1_0000001, 0b0_1000011}, 0b0010001_0000001_1000011),
        (new byte[] {0b1_0010001, 0b1_0000000, 0b0_0000000}, 0b0010001_0000000_0000000)
    };

    [Test]
    public void TestWriteKnownNumbers() {
        byte[] buffer = new byte[0x8];
        BufferWriter writer = new(buffer);
        BufferReader reader = new(buffer);

        foreach (var pair in knownNumbers) {
            writer.ResetPosition();
            reader.ResetPosition();

            BinaryBundleHelpers.WriteCollectionSize(writer, pair.value);

            foreach (byte b in pair.rawBytes) {
                Assert.AreEqual(b, reader.ReadByte());
            }
        }
    }

    [Test]
    public void TestReadKnownNumbers() {
        byte[] buffer = new byte[0x8];
        BufferWriter writer = new(buffer);
        BufferReader reader = new(buffer);

        foreach (var pair in knownNumbers) {
            writer.ResetPosition();
            reader.ResetPosition();
            
            foreach (byte b in pair.rawBytes) {
                writer.WriteByte(b);
            }

            int size = BinaryBundleHelpers.ReadCollectionSize(reader);
            Assert.AreEqual(pair.value, size);
        }
    }
}