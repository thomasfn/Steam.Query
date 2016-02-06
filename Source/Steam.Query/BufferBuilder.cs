using System;
using System.Collections.Generic;
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
  
        public void WriteShort(ushort n)
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

        public void WriteEnum<TEnum>(TEnum val) where TEnum : struct
        {
            WriteEnum<TEnum, byte>(val);
        }

        public void WriteEnum<TEnum, TDotNetType>(TEnum val) where TEnum : struct
        {
            var enumType = typeof (TEnum);
            if (!enumType.IsEnum)
                throw new ArgumentException("TEnum must be an enum.");

            if (!IsSteamCompatibleNumericalType(typeof(TDotNetType)))
                throw new InvalidOperationException("TDotNetType must correspond to a valid Steam numerical type.");

            Action<BufferBuilder, object> writer;
            if (!DotNetTypeWriteMethods.TryGetValue(typeof(TDotNetType), out writer))
                throw new ArgumentException($"Unable to write {typeof(TDotNetType).Name}");

            var enumCompatibleValue = Convert.ChangeType(val, typeof(TDotNetType));

            writer(this, enumCompatibleValue);
        }

        public byte[] ToArray() => _bytes.ToArray();

        private static bool IsSteamCompatibleNumericalType(Type t)
        {
            return DotNetTypeWriteMethods.ContainsKey(t) && t != typeof(string);
        }

        private static readonly Dictionary<Type, Action<BufferBuilder, object>> DotNetTypeWriteMethods = new Dictionary<Type, Action<BufferBuilder, object>>
        {
            [typeof(byte)] = (builder, x) => builder.WriteByte((byte)x),
            [typeof(ushort)] = (builder, x) => builder.WriteShort((ushort)x),
            [typeof(int)] = (builder, x) => builder.WriteLong((int)x),
            [typeof(char)] = (builder, x) => builder.WriteChar((char)x),
            [typeof(string)] = (builder, x) => builder.WriteString((string)x),
        };
    }
}