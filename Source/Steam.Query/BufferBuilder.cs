using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Steam.Query
{
    internal class BufferBuilder
    {

        private readonly List<byte> _bytes;

        public BufferBuilder(int initialCapacity = 128)
        {
            _bytes = new List<byte>(initialCapacity);
        }

        public int Length => _bytes.Count;


        public void WriteByte(byte b)
        {
            _bytes.Add(b);
        }

        public void WriteBytes(IEnumerable<byte> bytes)
        {
            _bytes.AddRange(bytes);
        }

        public void WriteChar(char c)
        {
            WriteByte((byte)c);
        }
  
        public void WriteShort(short n)
        {
            WriteBytes(BitConverter.GetBytes(n));
        }

        public void WriteLong(int n)
        {
            WriteBytes(BitConverter.GetBytes(n));
        }
        
        public void WriteString(string str)
        {
            WriteBytes(Encoding.ASCII.GetBytes(str));
            WriteByte(0x00);
        }

        public byte[] ToArray() => _bytes.ToArray();
    }
}