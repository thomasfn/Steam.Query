using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Steam.Query.MasterServers;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query.Tests.IntegrationTests
{
    [TestFixture]
    public class MasterServerTests
    {
        [Test]
        public async Task BasicQuery()
        {
            var client = new MasterServer();

            var request = new MasterServerRequest(
                MasterServerRegion.Europe,
                Filter.GamedirIs("tf")
                )
            {
                MaximumPackets = 2,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            var t = await client.GetServersAsync(request);
            var servers = t.ToList();

            Assert.IsTrue(servers.Any(), "No servers were returned");
        }

        [Test]
        public async Task MultiFilterQuery()
        {
            var client = new MasterServer();

            var request = new MasterServerRequest(
                MasterServerRegion.Europe,
                Filter.GamedirIs("tf"),
                Filter.AppIdIsNot("500")
                )
            {
                MaximumPackets = 2,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            var t = await client.GetServersAsync(request);
            var servers = t.ToList();

            Assert.IsTrue(servers.Any(), "No servers were returned");
        }

        [Test]
        public async Task LogicalQuery()
        {
            var client = new MasterServer();

            var request = new MasterServerRequest(
                MasterServerRegion.Europe,
                Filter.GamedirIs("tf"),
                Filter.IsNotEmpty,
                Filter.IsNotFull,
                Filter.Nor(
                    Filter.NameMatches("*Valve*"),
                    Filter.NameMatches("*trade*")
                    )
                )
            {
                MaximumPackets = 1
            };

            var servers = (await client.GetServersAsync(request)).Take(100).ToList();
            Assert.IsTrue(servers.Any(), "No servers were returned");

            var infos = await Task.WhenAll(servers.Select(async x => new {Server = x, Info = await x.TryGetServerInfoAsync()}));



            foreach (var i in infos.Where(x => x.Info != null))
            {
                Assert.AreEqual("tf", i.Info.Gamedir, $"Unexpected gamedir {i.Server.EndPoint}; {i.Info.Gamedir}");

                Assert.That(i.Info.Players, Is.AtLeast(1), $"Unexpected empty server {i.Server.EndPoint}");
                Assert.That(i.Info.Players, Is.LessThan(i.Info.MaxPlayers), $"Unexpected full server {i.Server.EndPoint}");

                Assert.That(!i.Info.Name.Contains("Valve") && !i.Info.Name.Contains("trade"), $"Expected server name to contain neither 'Valve' or 'trade' - was [{i.Info.Name}].");

            }
        }

    }
}
