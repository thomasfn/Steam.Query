using NUnit.Framework;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query.Tests.UnitTests
{
    [TestFixture]
    public class FilterTests
    {
        [Test]
        public void NorFilterOutputsExpectedValue()
        {
            Assert.AreEqual(@"nor\2\full\1\proxy\1", new NorFilter(Filter.IsNotFull, Filter.IsSpectatorProxy).GetFilterString());
        }

        [Test]
        public void NotFilterOutputsExpectedValue()
        {
            Assert.AreEqual(@"nor\1\full\1", new NorFilter(Filter.IsNotFull).GetFilterString());
        }

        [Test]
        public void NandFilterOutputsExpectedValue()
        {
            Assert.AreEqual(@"nand\2\full\1\proxy\1", new NandFilter(Filter.IsNotFull, Filter.IsSpectatorProxy).GetFilterString());
        }

    }
}
