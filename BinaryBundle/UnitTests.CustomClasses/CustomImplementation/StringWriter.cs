using System;
using System.Text;
using BinaryBundle;
using NUnit.Framework;

namespace UnitTests.CustomClasses; 

public class StringWriter : IBundleWriter {
    private StringBuilder stringBuilder;

    public StringWriter() {
        this.stringBuilder = new();
    }

    public void WriteString(string value) {
        stringBuilder.Append(value);
        stringBuilder.Append(',');
    }

    public void WriteInt64(long value) {
        throw new NotImplementedException();
    }

    public void WriteUInt64(ulong value) {
        throw new NotImplementedException();
    }

    public void WriteInt32(int value) {
        throw new NotImplementedException();
    }

    public void WriteUInt32(uint value) {
        throw new NotImplementedException();
    }

    public void WriteInt16(short value) {
        throw new NotImplementedException();
    }

    public void WriteUInt16(ushort value) {
        throw new NotImplementedException();
    }

    public void WriteByte(byte value) {
        WriteString(value.ToString());
    }

    public void WriteSByte(sbyte value) {
        throw new NotImplementedException();
    }

    public void WriteFloat(float value) {
        throw new NotImplementedException();
    }

    public void WriteDecimal(decimal value) {
        throw new NotImplementedException();
    }

    public void WriteDouble(double value) {
        throw new NotImplementedException();
    }

    public void WriteBool(bool value) {
        throw new NotImplementedException();
    }

    public void WriteChar(char value) {
        throw new NotImplementedException();
    }

    public void WriteBytes(byte[] bytes) {
        throw new NotImplementedException();
    }

    public void WriteBytes(byte[] bytes, int offset, int count) {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return stringBuilder.ToString();
    }
}