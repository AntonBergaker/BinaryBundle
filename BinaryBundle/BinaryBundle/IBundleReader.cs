using System;

namespace BinaryBundle;

/// <summary>
/// Interface containing every method used by the reader inside BinaryBundle's generated Deserialize method.
/// </summary>
public interface IBundleReader {
    string ReadString();
    char ReadChar();
    long ReadInt64();
    ulong ReadUInt64();
    int ReadInt32();
    uint ReadUInt32();
    short ReadInt16();
    ushort ReadUInt16();
    byte ReadByte();
    sbyte ReadSByte();
    bool ReadBool();
    float ReadFloat();
    decimal ReadDecimal();
    double ReadDouble();
    byte[] ReadBytes(int count);
}