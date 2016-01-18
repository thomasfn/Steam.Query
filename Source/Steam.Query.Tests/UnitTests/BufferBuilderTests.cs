using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Steam.Query.Tests.UnitTests
{
    [TestFixture]
    public class BufferBuilderTests
    {
        [Test]
        public void WritesByte()
        {
            var builder = new BufferBuilder();
            builder.WriteByte(32);
            Assert.That(builder.ToArray(), new EqualConstraint(new[] { 32 }));
        }

        [Test]
        public void WritesChar()
        {
            var builder = new BufferBuilder();
            builder.WriteChar('A');
            Assert.That(builder.ToArray(), new EqualConstraint(new[] { 65 }));
        }

        [Test]
        public void WritesShort()
        {
            var builder = new BufferBuilder();
            builder.WriteShort(513);
            Assert.That(builder.ToArray(), new EqualConstraint(new[] { 1, 2 }));
        }

        [Test]
        public void WritesLong()
        {
            var builder = new BufferBuilder();
            builder.WriteLong(0x0FEDBEEF);
            Assert.That(builder.ToArray(), new EqualConstraint(new[] {0xEF, 0xBE, 0xED, 0x0F}));
        }

        [Test]
        public void WritesBytes()
        {
            var builder = new BufferBuilder();
            builder.WriteBytes(new byte[] { 3, 255, 32 });
            Assert.That(builder.ToArray(), new EqualConstraint(new[] {3, 255, 32}));
        }

        [Test]
        public void WritesString()
        {
            var builder = new BufferBuilder();
            builder.WriteString("ABC");
            Assert.That(builder.ToArray(), new EqualConstraint(new[] {65, 66, 67, 0}));
        }

        [Test]
        public void WritesSequentially()
        {
            var builder = new BufferBuilder();
            builder.WriteByte(3);
            builder.WriteString("ABC");
            builder.WriteShort(2);
            builder.WriteLong(8);
            Assert.That(builder.ToArray(), new EqualConstraint(new[] {3, 65, 66, 67, 0, 2, 0, 8, 0, 0, 0}));
        }

        [Test]
        public void WritesEnums()
        {
            var builder = new BufferBuilder();
            builder.WriteEnum(TestEnum.One);
            builder.WriteEnum<TestEnum, ushort>(TestEnum.Two);
            builder.WriteEnum<TestEnum, int>(TestEnum.Four);
            Assert.That(builder.ToArray(), new EqualConstraint(new[] {1, 2, 0, 4, 0, 0, 0}));
        }



    }
}