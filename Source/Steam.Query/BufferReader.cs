using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.Query
{
    public class BufferReader
    {
        private readonly byte[] _bytes;

        public BufferReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte ReadByte()
        {
            return _bytes[CurrentPosition++];
        }

        public bool HasUnreadBytes => _bytes.Length - 1 >= CurrentPosition;

        public void Skip(int i)
        {
            CurrentPosition += i;
        }

        public string ReadString()
        {
            var buffer = new List<byte>();
            for (;CurrentPosition < _bytes.Length; CurrentPosition++)
            {
                var b = _bytes[CurrentPosition];
                if (b != 0x00)
                {
                    buffer.Add(b);
                }
                else
                {
                    CurrentPosition++;
                    break;
                }
            }
            return Encoding.ASCII.GetString(buffer.ToArray());
        }

        public ushort ReadShort()
        {
            return BitConverter.ToUInt16(new[]
            {
                _bytes[CurrentPosition++],
                _bytes[CurrentPosition++]
            }, 0);
        }

        public char ReadChar()
        {
            return Encoding.ASCII.GetChars(_bytes, CurrentPosition++, 1)[0];
        }

        public int CurrentPosition { get; set; }

    }
}