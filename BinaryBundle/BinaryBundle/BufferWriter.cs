using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace BinaryBundle; 

public class BufferWriter : IDisposable {

    private readonly BinaryWriter writer;
    public byte[] Buffer { get; }

    private readonly MemoryStream stream;

    public BufferWriter(byte[] buffer) {
        Buffer = buffer;
        stream = new MemoryStream(Buffer);
        writer = new BinaryWriter(stream);
    }

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

    public void WriteString(string value) {
        writer.Write(Encoding.UTF8.GetBytes(value));
        byte b = 0;
        writer.Write(b);
    }

    public void WriteInt64(long value) => writer.Write(value);

    public void WriteUInt64(ulong value) => writer.Write(value);

    public void WriteInt32(int value) => writer.Write(value);

    public void WriteInt16(short value) => writer.Write(value);

    public void WriteUInt16(ushort value) => writer.Write(value);

    public void WriteByte(byte value) => writer.Write(value);

    public void WriteFloat(float value) => writer.Write(value);

    public void WriteDouble(double value) => writer.Write(value);

    public void WriteBool(bool value) => writer.Write(value);

    public void WriteBytes(byte[] bytes) => writer.Write(bytes);

    public void WriteBytes(byte[] bytes, int offset, int count) => writer.Write(bytes, offset, count);

    public void ResetPosition() {
        writer.Seek(0, SeekOrigin.Begin);
    }

    public int Position => (int)writer.BaseStream.Position;

    public void WriteToStream(Stream stream, int count) {
        this.stream.Position = 0;
        stream.Write(Buffer, 0, count);
    }

    public void Dispose() {
        writer.Dispose();
        stream.Dispose();
    }
}