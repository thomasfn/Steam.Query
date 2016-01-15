using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.Query
{
    using System.IO;
    using System.Linq;

    public class BufferReader
    {
        private readonly byte[] _bytes;

        public BufferReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public int CurrentPosition { get; private set; }

        public bool HasUnreadBytes => _bytes.Length - 1 >= CurrentPosition;

        public int Remaining => _bytes.Length - CurrentPosition;

        public byte ReadByte()
        {
            return _bytes[CurrentPosition++];
        }
        
        public void Skip(int i)
        {
            CurrentPosition += i;
        }

        public string ReadString()
        {
            var terminus = Array.IndexOf<byte>(_bytes, 0, CurrentPosition);

            if (terminus == -1)
                return null;
            
            var str = Encoding.ASCII.GetString(_bytes, CurrentPosition, terminus - CurrentPosition);
            CurrentPosition = terminus + 1;

            return str;
        }

        public bool IsStringTerminated()
        {
            return Array.IndexOf<byte>(_bytes, 0, CurrentPosition) != -1;
        }

        public string ReadPartialString()
        {
            var terminus = Array.IndexOf<byte>(_bytes, 0, CurrentPosition);

            if (terminus == -1)
                terminus = _bytes.Length;

            var str = Encoding.ASCII.GetString(_bytes, CurrentPosition, terminus - CurrentPosition);
            CurrentPosition = terminus + 1;

            return str;
        }

        public short ReadShort()
        {
            var n = BitConverter.ToInt16(_bytes, CurrentPosition);
            CurrentPosition += 2;

            return n;
        }

        public int ReadLong()
        {
            var n = BitConverter.ToInt32(_bytes, CurrentPosition);
            CurrentPosition += 4;

            return n;
        }

        public char ReadChar()
        {
            return Encoding.ASCII.GetChars(new[] {ReadByte()}, 0, 1).Single();
        }

        public ArraySegment<byte> ReadSegment(int length)
        {
            var segment =  new ArraySegment<byte>(_bytes, CurrentPosition, length);
            CurrentPosition += length;

            return segment;
        }

        public IEnumerable<byte> ReadUntil(Func<byte, bool> predicate)
        {
            var bytes = _bytes.Skip(CurrentPosition).TakeWhile(predicate).ToList();
            CurrentPosition += bytes.Count;

            return bytes;
        }

        public void WriteAllToFile(string file)
        {
            File.WriteAllBytes(file, _bytes);
        }

    }
}