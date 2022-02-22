using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BinaryBundle; 

public class BufferReader : IDisposable {

    private readonly BinaryReader reader;
    private readonly MemoryStream stream;

    public int Position => (int)reader.BaseStream.Position;

    public byte[] Buffer;

    public BufferReader(byte[] buffer) {
        this.Buffer = buffer;
        stream = new MemoryStream(buffer);
        reader = new BinaryReader(stream);
    }

    private readonly List<byte> byteList = new List<byte>();
    public string ReadString() {
        byteList.Clear();
        while (true) {
            byte readByte = reader.ReadByte();
            if (readByte == 0) {
                break;
            }
            byteList.Add(readByte);
        }

        return Encoding.UTF8.GetString(byteList.ToArray());
    }

    public T ReadEnum<T>() where T : Enum {
        if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte)) {
            byte @byte = reader.ReadByte();
#if NET5_0_OR_GREATER
            return Unsafe.As<byte, T>(ref @byte);
#else
            return (T)(object)(@byte);
#endif

        } else {
            int @int = reader.ReadInt32();
#if NET5_0_OR_GREATER
            return Unsafe.As<int, T>(ref @int);
#else
            return (T)(object)(@int);
#endif
        }
    }

    public char ReadChar() => reader.ReadChar();

    public long ReadInt64() => reader.ReadInt64();

    public ulong ReadUInt64() => reader.ReadUInt64();

    public int ReadInt32() => reader.ReadInt32();

    public uint ReadUInt32() => reader.ReadUInt32();

    public short ReadInt16() => reader.ReadInt16();

    public ushort ReadUInt16() => reader.ReadUInt16();

    public byte ReadByte() => reader.ReadByte();

    public sbyte ReadSByte() => reader.ReadSByte();

    public bool ReadBool() => reader.ReadBoolean();

    public float ReadFloat() => reader.ReadSingle();

    public decimal ReadDecimal() => reader.ReadDecimal();

    public double ReadDouble() => reader.ReadDouble();

    public byte[] ReadBytes(int count) => reader.ReadBytes(count);

    public void ResetPosition() {
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
    }

    public void Dispose() {
        stream.Dispose();
        reader.Dispose();
    }
}