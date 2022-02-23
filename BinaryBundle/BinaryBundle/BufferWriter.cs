using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace BinaryBundle; 

public class BufferWriter : IDisposable, IBundleWriter {

    private readonly BinaryWriter writer;

    private readonly bool managesStream;
    private readonly Stream stream;


    public BufferWriter(byte[] buffer) {
        managesStream = true;
        stream = new MemoryStream(buffer);
        writer = new BinaryWriter(stream);
    }

    public BufferWriter(Stream stream) {
        managesStream = false;
        this.stream = stream;
        this.writer = new BinaryWriter(stream);
    }

    /// <summary>
    /// Writes an enum to the buffer using the enums underlying type and advances the stream position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    public void WriteEnum<T>(T enumValue) where T : Enum {
        if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte)) {
#if NET5_0_OR_GREATER
            byte b = Unsafe.As<T, byte>(ref enumValue);
#else
            byte b = (byte)(object)enumValue;
#endif
            writer.Write(b);
        }
        else {
#if NET5_0_OR_GREATER
            int i = Unsafe.As<T, int>(ref enumValue);
#else
            int i = (int)(object)enumValue;
#endif
            writer.Write(i);
        }
    }

    /// <summary>
    /// Writes a UTF8 encoded null terminated string to the buffer and advances the current position in the stream according to the size of the string.
    /// </summary>
    /// <param name="value"></param>
    public void WriteString(string value) {
        writer.Write(Encoding.UTF8.GetBytes(value));
        byte b = 0;
        writer.Write(b);
    }

    /// <inheritdoc cref="BinaryWriter.Write(long)"/>
    public void WriteInt64(long value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(ulong)"/>
    public void WriteUInt64(ulong value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(int)"/>
    public void WriteInt32(int value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(uint)"/>
    public void WriteUInt32(uint value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(short)"/>
    public void WriteInt16(short value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(ushort)"/>
    public void WriteUInt16(ushort value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(byte)"/>
    public void WriteByte(byte value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(sbyte)"/>
    public void WriteSByte(sbyte value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(float)"/>
    public void WriteFloat(float value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(decimal)"/>
    public void WriteDecimal(decimal value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(double)"/>
    public void WriteDouble(double value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(bool)"/>
    public void WriteBool(bool value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(char)"/>
    public void WriteChar(char value) => writer.Write(value);

    /// <inheritdoc cref="BinaryWriter.Write(byte[])"/>
    public void WriteBytes(byte[] bytes) => writer.Write(bytes);

    /// <inheritdoc cref="BinaryWriter.Write(byte[], int, int)"/>
    public void WriteBytes(byte[] bytes, int offset, int count) => writer.Write(bytes, offset, count);

    public void ResetPosition() {
        writer.Seek(0, SeekOrigin.Begin);
    }

    public int Position => (int)writer.BaseStream.Position;

    public void Dispose() {
        writer.Dispose();
        if (managesStream) {
            stream.Dispose();
        }
    }
}