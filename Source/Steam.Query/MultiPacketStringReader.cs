using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Query
{
    public class MultiPacketStringReader
    {
        private BufferReader _reader;
        private readonly Func<Task<BufferReader>> _sequelRequestAsyncFunc;

        public MultiPacketStringReader(BufferReader initialReader, Func<Task<BufferReader>> sequelRequestAsyncFunc)
        {
            _reader = initialReader;
            _sequelRequestAsyncFunc = sequelRequestAsyncFunc;
        }
        
        public async Task<string> ReadStringAsync()
        {
            var byteEnumerables = new List<IEnumerable<byte>>();

            while (!_reader.IsStringTerminated())
            {
                byteEnumerables.Add(_reader.ReadSegment(_reader.Remaining));
                _reader = await _sequelRequestAsyncFunc();
            }

            byteEnumerables.Add(_reader.ReadUntil(x => x != 0x00));
            _reader.Skip(1);

            return Encoding.ASCII.GetString(byteEnumerables.SelectMany(x => x).ToArray());
        }
    }
}