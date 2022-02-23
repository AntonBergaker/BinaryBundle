using System;
using System.IO;

namespace BinaryBundle;

public interface IBundleWriter {
    /// <summary>
    /// Writes an enum to the buffer using the enums underlying type and advances the stream position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    void WriteEnum<T>(T enumValue) where T : Enum;

    /// <summary>
    /// Writes a UTF8 encoded null terminated string to the buffer and advances the current position in the stream according to the size of the string.
    /// </summary>
    /// <param name="value"></param>
    void WriteString(string value);

    /// <inheritdoc cref="BinaryWriter.Write(long)"/>
    void WriteInt64(long value);

    /// <inheritdoc cref="BinaryWriter.Write(ulong)"/>
    void WriteUInt64(ulong value);

    /// <inheritdoc cref="BinaryWriter.Write(int)"/>
    void WriteInt32(int value);

    /// <inheritdoc cref="BinaryWriter.Write(uint)"/>
    void WriteUInt32(uint value);

    /// <inheritdoc cref="BinaryWriter.Write(short)"/>
    void WriteInt16(short value);

    /// <inheritdoc cref="BinaryWriter.Write(ushort)"/>
    void WriteUInt16(ushort value);

    /// <inheritdoc cref="BinaryWriter.Write(byte)"/>
    void WriteByte(byte value);

    /// <inheritdoc cref="BinaryWriter.Write(sbyte)"/>
    void WriteSByte(sbyte value);

    /// <inheritdoc cref="BinaryWriter.Write(float)"/>
    void WriteFloat(float value);

    /// <inheritdoc cref="BinaryWriter.Write(decimal)"/>
    void WriteDecimal(decimal value);

    /// <inheritdoc cref="BinaryWriter.Write(double)"/>
    void WriteDouble(double value);

    /// <inheritdoc cref="BinaryWriter.Write(bool)"/>
    void WriteBool(bool value);

    /// <inheritdoc cref="BinaryWriter.Write(char)"/>
    void WriteChar(char value);

    /// <inheritdoc cref="BinaryWriter.Write(byte[])"/>
    void WriteBytes(byte[] bytes);

    /// <inheritdoc cref="BinaryWriter.Write(byte[], int, int)"/>
    void WriteBytes(byte[] bytes, int offset, int count);
}