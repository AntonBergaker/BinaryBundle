using System;
using System.IO;

namespace BinaryBundle;

/// <summary>
/// Interface containing every method used by the reader inside BinaryBundle's generated Serialize method.
/// </summary>
public interface IBundleWriter {
    void WriteString(string value);
    void WriteInt64(long value);
    void WriteUInt64(ulong value);
    void WriteInt32(int value);
    void WriteUInt32(uint value);
    void WriteInt16(short value);
    void WriteUInt16(ushort value);
    void WriteByte(byte value);
    void WriteSByte(sbyte value);
    void WriteFloat(float value);
    void WriteDecimal(decimal value);
    void WriteDouble(double value);
    void WriteBool(bool value);
    void WriteChar(char value);
    void WriteBytes(byte[] bytes);
    void WriteBytes(byte[] bytes, int offset, int count);
}