﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Steam.Query
{
    internal class BufferReader
    {
        private readonly byte[] _bytes;

        public BufferReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public int CurrentPosition { get; private set; }

        public int Remaining => _bytes.Length - CurrentPosition;

        public byte ReadByte()
        {
            return _bytes[CurrentPosition++];
        }

        public IEnumerable<byte> ReadBytes(int length)
        {
            var segment = new ArraySegment<byte>(_bytes, CurrentPosition, length);
            CurrentPosition += length;

            return segment;
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
            
            var str = Encoding.UTF8.GetString(_bytes, CurrentPosition, terminus - CurrentPosition);
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

            var str = Encoding.UTF8.GetString(_bytes, CurrentPosition, terminus - CurrentPosition);
            CurrentPosition = terminus + 1;

            return str;
        }

        public ushort ReadShort()
        {
            var n = BitConverter.ToUInt16(_bytes, CurrentPosition);
            CurrentPosition += 2;

            return n;
        }

        public int ReadLong()
        {
            var n = BitConverter.ToInt32(_bytes, CurrentPosition);
            CurrentPosition += 4;

            return n;
        }

        public float ReadFloat()
        {
            var x = BitConverter.ToSingle(_bytes, CurrentPosition);
            CurrentPosition += 4;

            return x;
        }

        public char ReadChar()
        {
            var charBuffer = new char[1];
            int bytesUsed;
            int charsUsed;
            bool completed;
            Encoding.UTF8.GetDecoder().Convert(_bytes, CurrentPosition, Math.Min(8, Remaining), charBuffer, 0, 1, true, out bytesUsed, out charsUsed, out completed);

            CurrentPosition += bytesUsed;
            return charBuffer.Single();
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

        internal TDotNetType Read<TDotNetType>()
        {
            Func<BufferReader, object> func;
            if (!DotNetTypeReadMethods.TryGetValue(typeof(TDotNetType), out func))
                throw new InvalidOperationException($"{typeof(TDotNetType).Name} is not a valid type for Read<TDotNetType>. Valid choices: { string.Join(", ", DotNetTypeReadMethods.Keys.Select(x => x.Name)) }.");

            return (TDotNetType) func(this);
        }

        public TEnum ReadEnum<TEnum>() where TEnum : struct
        {
            return ReadEnum<TEnum, byte>();
        }

        public TEnum ReadEnum<TEnum, TDotNetType>() where TEnum : struct
        {
            var enumType = typeof (TEnum);
            if (!enumType.IsEnum)
                throw new ArgumentException("TEnum must be an enum.");

            if (!IsSteamCompatibleNumericalType(typeof (TDotNetType)))
                throw new InvalidOperationException("TDotNetType must correspond to a valid Steam numerical type.");

            var val = (object)Read<TDotNetType>();
            var enumCompatibleValue = Convert.ChangeType(val, Enum.GetUnderlyingType(enumType));

            if (!Enum.IsDefined(enumType, enumCompatibleValue))
                throw new InvalidCastException($"The enum type {enumType.Name} does not define the read value {val}.");

            return (TEnum) enumCompatibleValue;
        }

        public bool ReadBool()
        {
            var b = ReadByte();
            switch (b)
            {
                case 1:
                    return true;
                case 0:
                    return false;
                default:
                    throw new ProtocolViolationException($"Attempted to read boolean value, but {b} is not a legal value. (Should be 0 or 1)");
            }
        }

        private static bool IsSteamCompatibleNumericalType(Type t)
        {
            return DotNetTypeReadMethods.ContainsKey(t) && t != typeof (string);
        }
        
        private static readonly Dictionary<Type, Func<BufferReader, object>> DotNetTypeReadMethods = new Dictionary<Type, Func<BufferReader, object>>
        {
            [typeof(byte)] = reader => reader.ReadByte(),
            [typeof(ushort)] = reader => reader.ReadShort(),
            [typeof(int)] = reader => reader.ReadLong(),
            [typeof(char)] = reader => reader.ReadChar(),
            [typeof(string)] = reader => reader.ReadString()
        };

        

    }
}