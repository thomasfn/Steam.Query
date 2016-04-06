using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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
        public void ReadsBytes()
        {
            Assert.That(new BufferReader(new byte[] {254, 255, 200, 1}).ReadBytes(3), new EqualConstraint(new [] {254, 255, 200}));
        }

        [Test]
        public void ReadsChar()
        {
            Assert.AreEqual('A', new BufferReader(new byte[] { 65 }).ReadChar());
        }

        [Test]
        public void ReadsUtf8Chars()
        {
            var reader = new BufferReader(new byte[] { 65, 231, 140, 171, 66 });

            Assert.AreEqual('A', reader.ReadChar());
            Assert.AreEqual(1, reader.CurrentPosition);

            Assert.AreEqual('猫', reader.ReadChar());
            Assert.AreEqual(4, reader.CurrentPosition);

            Assert.AreEqual('B', reader.ReadChar());
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
            var reader = new BufferReader(new byte[] { 10, 0, 10, 67, 65, 75, 69, 00, 00, 2, 2, 0 });

            Assert.AreEqual(10, reader.ReadByte());
            Assert.AreEqual(2560, reader.ReadShort());
            Assert.AreEqual("CAKE", reader.ReadString());
            Assert.AreEqual("", reader.ReadString());
            Assert.AreEqual(514, reader.ReadShort());
        }

        [Test]
        public void ReadsSequentiallyAndGenerically()
        {
            var reader = new BufferReader(new byte[] { 10, 0, 10, 67, 65, 75, 69, 00, 00, 2, 2, 0 });

            Assert.AreEqual(10, reader.Read<byte>());
            Assert.AreEqual(2560, reader.Read<ushort>());
            Assert.AreEqual("CAKE", reader.Read<string>());
            Assert.AreEqual("", reader.Read<string>());
            Assert.AreEqual(514, reader.Read<ushort>());
        }

        [Test]
        public void ReadsEnums()
        {
            var reader = new BufferReader(new byte[] { 1, 2, 0, 4, 0, 0, 0 });

            Assert.AreEqual(TestEnum.One, reader.ReadEnum<TestEnum>());
            Assert.AreEqual(TestEnum.Two, reader.ReadEnum<TestEnum, ushort>());
            Assert.AreEqual(TestEnum.Four, reader.ReadEnum<TestEnum, int>());
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

        [Test]
        public void ReadsFloat()
        {
            var reader = new BufferReader(new byte[] { 133, 235, 85, 65 });
            
            Assert.AreEqual(13.37f, reader.ReadFloat());
        }

    }
}
