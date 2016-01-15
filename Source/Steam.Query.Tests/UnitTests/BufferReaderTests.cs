using NUnit.Framework;

namespace Steam.Query.Tests.UnitTests
{
    [TestFixture]
    public class BufferReaderTests
    {
        [Test]
        public void ReadsByte()
        {
            Assert.AreEqual(32, new BufferReader(new byte[] { 32 }).ReadByte());
        }

        [Test]
        public void ReadsChar()
        {
            Assert.AreEqual('A', new BufferReader(new byte[] { 65 }).ReadChar());
        }

        [Test]
        public void ReadsShort()
        {
            Assert.AreEqual(513, new BufferReader(new byte[] { 1, 2 }).ReadShort());
        }

        [Test]
        public void ReadsString()
        {
            Assert.AreEqual("ABC", new BufferReader(new byte[] { 65, 66, 67, 00 }).ReadString());
        }

        [Test]
        public void ReadsSequentially()
        {
            var reader = new BufferReader(new byte[] {10, 0, 10, 67, 65, 75, 69, 00, 00, 2, 2, 0});
            
            Assert.AreEqual(10, reader.ReadByte());
            Assert.AreEqual(2560, reader.ReadShort());
            Assert.AreEqual("CAKE", reader.ReadString());
            Assert.AreEqual("", reader.ReadString());
            Assert.AreEqual(514, reader.ReadShort());
        }

        [Test]
        public void ReadsPartialTerminatedString()
        {
            var reader = new BufferReader(new byte[] { 65, 66, 67, 00 });

            Assert.IsTrue(reader.IsStringTerminated());
            Assert.AreEqual("ABC", reader.ReadPartialString());
        }

        [Test]
        public void ReadsPartialUnterminatedString()
        {
            var reader = new BufferReader(new byte[] { 65, 66, 67 });

            Assert.IsFalse(reader.IsStringTerminated());
            Assert.AreEqual("ABC", reader.ReadPartialString());
        }

    }
}
