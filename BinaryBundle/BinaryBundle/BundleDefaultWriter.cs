using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace BinaryBundle; 

/// <summary>
/// Binary stream writer for primitive .NET types.
/// </summary>
public class BundleDefaultWriter : IDisposable, IBundleWriter {

    private readonly BinaryWriter writer;

    private readonly bool managesStream;
    private readonly Stream stream;

    public BundleDefaultWriter(byte[] buffer) {
        managesStream = true;
        stream = new MemoryStream(buffer);
        writer = new BinaryWriter(stream);
    }

    public BundleDefaultWriter(Stream stream) {
        managesStream = false;
        this.stream = stream;
        this.writer = new BinaryWriter(stream);
    }

    /// <summary>
    /// Writes a UTF8 encoded null terminated string to the buffer and advances the current position in the stream according to the size of the string.
    /// </summary>
    /// <param name="value"></param>
    public void WriteString(string? value) {
        byte b = 0;
        if (value == null) {
            writer.Write(b);
            return;
        }
        writer.Write(Encoding.UTF8.GetBytes(value));
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

    /// <summary>
    /// Resets the write position to the start of the stream
    /// </summary>
    public void ResetPosition() {
        writer.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Returns the current write index of the stream
    /// </summary>
    public int Position => (int)writer.BaseStream.Position;

    /// <summary>
    /// Releases the BundleDefaultWriter
    /// </summary>
    public void Dispose() {
        writer.Dispose();
        if (managesStream) {
            stream.Dispose();
        }
    }
}