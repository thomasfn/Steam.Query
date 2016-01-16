using System;
using System.Linq;
using NUnit.Framework;
using Steam.Query.MasterServers;

namespace Steam.Query.Tests.IntegrationTests
{
    [TestFixture]
    public class MasterServerTests
    {
        [Test]
        public void BasicQuery()
        {
            var client = new MasterServer();
            var t = client.GetServersAsync(MasterServerRegion.Europe, MasterServerFilter.Gamedir("tf"));
            t.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(t.Result.Any(), "No servers were returned");
        }

        [Test]
        public void MultiFilterQuery()
        {
            var client = new MasterServer();
            var t = client.GetServersAsync(MasterServerRegion.Europe, MasterServerFilter.Gamedir("tf"), MasterServerFilter.NotApp("500"));
            t.Wait(TimeSpan.FromSeconds(10));
            Assert.IsTrue(t.Result.Any(), "No servers were returned");
        }
    }
}
