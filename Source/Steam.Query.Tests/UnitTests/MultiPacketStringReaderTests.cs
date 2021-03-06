﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Steam.Query.Tests.UnitTests
{
    [TestFixture]
    public class MultiPacketStringReaderTests
    {

        [Test]
        public async Task ReadsSinglePacketString()
        {
            var bytes = new byte[] { 65, 65, 65, 00 };
            var reader = new MultiPacketStringReader(new BufferReader(bytes), () => { throw new Exception("Should not call this method"); });

            Assert.AreEqual("AAA", await reader.ReadStringAsync());
        }

        [Test]
        public async Task ReadsTwoPacketString()
        {
            var bytes = new byte[] { 65, 65, 65 };
            var bytes2 = new byte[] { 66, 66, 66, 00 };

            var reader = new MultiPacketStringReader(new BufferReader(bytes), () => Task.FromResult(new BufferReader(bytes2)));

            Assert.AreEqual("AAABBB", await reader.ReadStringAsync());
        }

        [Test]
        public async Task ReadsDelayedTwoPacketString()
        {
            var bytes = new byte[] { 66, 65, 65 };
            var bytes2 = new byte[] { 67, 66, 66, 00 };

            var reader = new MultiPacketStringReader(new BufferReader(bytes), async () =>
            {
                await Task.Delay(2000);
                return new BufferReader(bytes2);
            });

            Assert.AreEqual("BAACBB", await reader.ReadStringAsync());
        }



        [Test]
        public async Task ReadsDelayedMultiPacketString()
        {
            var bytes = new byte[] { 65, 65, 65 };
            var extraPackets = new[]
            {
                new byte[] { 66, 66, 66 },
                new byte[] { 67, 67 },
                new byte[] { 67 },
                new byte[] { 68, 68, 68, 00 },
            };

            var index = 0;

            var reader = new MultiPacketStringReader(new BufferReader(bytes), async () =>
            {
                await Task.Delay(1000);
                return new BufferReader(extraPackets[index++]);
            });

            Assert.AreEqual("AAABBBCCCDDD", await reader.ReadStringAsync());
        }

        [Test]
        public async Task ReadsMultiPacketStringExcludingPacketHeaders()
        {
            var bytes = new byte[] { 65, 65, 65 };
            var extraPackets = new[]
            {
                new byte[] { 00, 66, 66, 66 },
                new byte[] { 00, 67, 67 },
                new byte[] { 00, 67 },
                new byte[] { 00, 68, 68, 68, 00 },
            };

            var index = 0;

            var reader = new MultiPacketStringReader(new BufferReader(bytes), async () =>
            {
                await Task.Delay(1000);

                var sequelReader = new BufferReader(extraPackets[index++]);
                sequelReader.Skip(1);

                return sequelReader;
            });

            Assert.AreEqual("AAABBBCCCDDD", await reader.ReadStringAsync());
        }

    }
}
