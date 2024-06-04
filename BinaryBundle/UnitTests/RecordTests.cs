using BinaryBundle;
using NUnit.Framework;

namespace UnitTests;
internal partial class RecordTest {

    [BinaryBundle]
    public partial record struct RecordStruct(int Funny, bool Guy);


    [Test]
    public void SimpleRecordStruct() {

        var record = new RecordStruct(123, true);
        var deserializedRecord = TestUtils.MakeSerializedCopy(record);
        // How convenient
        Assert.AreEqual(record, deserializedRecord);
    }

    // This mostly tests that the generator didn't try to capture the init properties
    [BinaryBundle]
    public partial record Record(string Crock, uint Pot) {
        public Record(): this("", 0) { }
    }

    [Test]
    public void SimpleRecord() {

        var record = new Record("Giga", 123);
        var deserializedRecord = TestUtils.MakeSerializedCopy(record);

        Assert.AreNotEqual(record, deserializedRecord);
    }

    [BinaryBundle]
    public partial record ANotVeryRecordLikeRecord() {
        public string JacobsTestVariable { get; set; } = "";
        public long JacobsLongVariable { get; set; }
    }

    [Test]
    public void ClasslikeRecord() {

        var record = new ANotVeryRecordLikeRecord() {
            JacobsLongVariable = 123456789_10_11_12_13_14,
            JacobsTestVariable = "Glorp"
        };
        var deserializedRecord = TestUtils.MakeSerializedCopy(record);
        // How convenient
        Assert.AreEqual(record, deserializedRecord);
    }
}
