using System;

namespace BinaryBundle;

public interface IBundleReader {
    string ReadString();
    T ReadEnum<T>() where T : Enum;
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