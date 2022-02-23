using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryBundle;

namespace UnitTests.CustomClasses {
    internal class StringReader : IBundleReader {

        private readonly string input;
        private int readIndex;

        public StringReader(string input) {
            this.input = input;
            readIndex = 0;
        }

        public string ReadString() {
            int startIndex = readIndex;
            while (readIndex < input.Length) {
                char c = input[readIndex++];
                if (c == ',') {
                    // -1 to not include the comma
                    return input[startIndex..(readIndex-1)];
                }
            }

            throw new Exception("Tried to read outside the string input");
        }

        public char ReadChar() {
            throw new NotImplementedException();
        }

        public long ReadInt64() {
            throw new NotImplementedException();
        }

        public ulong ReadUInt64() {
            throw new NotImplementedException();
        }

        public int ReadInt32() {
            throw new NotImplementedException();
        }

        public uint ReadUInt32() {
            throw new NotImplementedException();
        }

        public short ReadInt16() {
            throw new NotImplementedException();
        }

        public ushort ReadUInt16() {
            throw new NotImplementedException();
        }

        public byte ReadByte() {
            throw new NotImplementedException();
        }

        public sbyte ReadSByte() {
            throw new NotImplementedException();
        }

        public bool ReadBool() {
            throw new NotImplementedException();
        }

        public float ReadFloat() {
            throw new NotImplementedException();
        }

        public decimal ReadDecimal() {
            throw new NotImplementedException();
        }

        public double ReadDouble() {
            throw new NotImplementedException();
        }

        public byte[] ReadBytes(int count) {
            throw new NotImplementedException();
        }
    }
}
