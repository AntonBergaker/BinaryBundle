﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BinaryBundle;

/// <summary>
/// Binary stream reader for primitive .NET types.
/// </summary>
public class BundleDefaultReader : IDisposable, IBundleReader {

    private readonly BinaryReader reader;

    private readonly bool managesStream;
    private readonly Stream stream;

    /// <summary>
    /// Current read position of the reader
    /// </summary>
    public int Position => (int)reader.BaseStream.Position;

    /// <summary>
    /// Instantiates a new BundleDefaultReader reading from the provided byte array.
    /// </summary>
    /// <param name="buffer"></param>
    public BundleDefaultReader(byte[] buffer) {
        managesStream = true;
        stream = new MemoryStream(buffer);
        reader = new BinaryReader(stream);
    }

    /// <summary>
    /// Instantiates a new BundleDefaultReader reading from the provided stream.
    /// </summary>
    /// <param name="stream"></param>
    public BundleDefaultReader(Stream stream) {
        managesStream = false;
        this.stream = stream;
        reader = new BinaryReader(stream);
    }

    private readonly List<byte> byteList = [];
    /// <summary>
    /// Reads a null terminated UTF8 string and advances the stream to the end of the string.
    /// </summary>
    /// <returns></returns>
    public string ReadString() {
        byteList.Clear();
        while (true) {
            byte readByte = reader.ReadByte();
            if (readByte == 0) {
                break;
            }
            byteList.Add(readByte);
        }

#if NET7_0_OR_GREATER
        return Encoding.UTF8.GetString(CollectionsMarshal.AsSpan(byteList));
#else
        return Encoding.UTF8.GetString(byteList.ToArray());
#endif
    }

    /// <inheritdoc cref="BinaryReader.ReadChar()"/>
    public char ReadChar() => reader.ReadChar();

    /// <inheritdoc cref="BinaryReader.ReadInt64()"/>
    public long ReadInt64() => reader.ReadInt64();

    /// <inheritdoc cref="BinaryReader.ReadUInt64()"/>
    public ulong ReadUInt64() => reader.ReadUInt64();

    /// <inheritdoc cref="BinaryReader.ReadInt32()"/>
    public int ReadInt32() => reader.ReadInt32();

    /// <inheritdoc cref="BinaryReader.ReadUInt32()"/>
    public uint ReadUInt32() => reader.ReadUInt32();

    /// <inheritdoc cref="BinaryReader.ReadInt16()"/>
    public short ReadInt16() => reader.ReadInt16();

    /// <inheritdoc cref="BinaryReader.ReadUInt16()"/>
    public ushort ReadUInt16() => reader.ReadUInt16();

    /// <inheritdoc cref="BinaryReader.ReadByte()"/>
    public byte ReadByte() => reader.ReadByte();

    /// <inheritdoc cref="BinaryReader.ReadSByte()"/>
    public sbyte ReadSByte() => reader.ReadSByte();

    /// <inheritdoc cref="BinaryReader.ReadBoolean()"/>
    public bool ReadBool() => reader.ReadBoolean();

    /// <inheritdoc cref="BinaryReader.ReadSingle()"/>
    public float ReadFloat() => reader.ReadSingle();

    /// <inheritdoc cref="BinaryReader.ReadDecimal()"/>
    public decimal ReadDecimal() => reader.ReadDecimal();

    /// <inheritdoc cref="BinaryReader.ReadDouble()"/>
    public double ReadDouble() => reader.ReadDouble();

    /// <inheritdoc cref="BinaryReader.ReadBytes()"/>
    public byte[] ReadBytes(int count) => reader.ReadBytes(count);

    /// <summary>
    /// Resets the read position to the start of the stream
    /// </summary>
    public void ResetPosition() {
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Releases the BundleDefaultReader
    /// </summary>
    public void Dispose() {
        if (managesStream) {
            stream.Dispose();
        }
        reader.Dispose();
    }
}