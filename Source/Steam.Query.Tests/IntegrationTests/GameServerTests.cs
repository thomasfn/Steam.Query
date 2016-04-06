using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Steam.Query.GameServers;
using Steam.Query.MasterServers;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query.Tests.IntegrationTests
{
    [TestFixture]
    public class GameServerTests
    {
        private List<IGameServer> _servers;

        private const int GetRulesAttempts = 4;
        private const int GetInfoAttempts = 4;
        private const int GetPlayersAttempts = 4;

        [TestFixtureSetUp]
        public void Setup() //Should be async, but isn't currently supported by NUnit
        {
            var client = new MasterServer();
            _servers = client.GetServersAsync(MasterServerRegion.Europe, Filter.GamedirIs("tf"), Filter.NameMatches("Valve*")).Result.ToList().Shuffle();
        }

        [Test]
        public async Task GetServerRulesAsync()
        {
            if (!_servers.Any())
                Assert.Inconclusive();

            for (var i = 0; i < GetRulesAttempts; i++)
            {
                try
                {
                    var serverRules = await _servers[i].GetServerRulesAsync().TimeoutAfter(TimeSpan.FromSeconds(3));
                    var rules = serverRules.Rules.ToList();

                    Assert.That(rules.Count, Is.AtLeast(100));
                    Assert.That(rules.All(x => IsValidRuleString(x.Key)));
                    
                    return;
                }
                catch (TimeoutException)
                {
                    
                }
            }

            Assert.Fail($"Query timed out on all {GetRulesAttempts} attempt(s)");
        }

        [Test]
        public void TimesOutWhenNotReceivingAnswer()
        {
            var server = new GameServer(new IPEndPoint(IPAddress.Parse("1.1.1.1"), 65534));

            Assert.Throws<TimeoutException>(async () => await server.GetServerInfoAsync());
        }

        private static readonly Regex RuleStringRegex = new Regex(@"^[A-Za-z0-9;,\.\-_]+$");
        private bool IsValidRuleString(string s)
        {
            return RuleStringRegex.IsMatch(s);
        }

        [Test]
        public async Task GetServerInfoAsync()
        {
            if (!_servers.Any())
                Assert.Inconclusive();

            for (var i = 0; i < GetInfoAttempts; i++)
            {
                try
                {
                    var serverInfo = await _servers[i].GetServerInfoAsync().TimeoutAfter(TimeSpan.FromSeconds(3));

                    Assert.That(serverInfo.Gamedir, Is.EqualTo("tf"));
                    Assert.That(serverInfo.Type, Is.EqualTo(GameServerType.Dedicated));
                    Assert.That(serverInfo.Name, Is.StringStarting("Valve"));
                    Assert.That(serverInfo.Ping, Is.InRange(1, 800));

                    return;
                }
                catch (TimeoutException)
                {

                }
            }

            Assert.Fail($"Query timed out on all {GetInfoAttempts} attempt(s)");
        }

        [Test]
        public async Task GetServerPlayersAsync()
        {
            if (!_servers.Any())
                Assert.Inconclusive();

            for (var i = 0; i < GetPlayersAttempts; i++)
            {
                try
                {
                    var rng = new Random();
                    IGameServer server;
                    IGameServerInfo serverInfo;

                    while (true)
                    {
                        server = _servers[rng.Next(_servers.Count)];
                        serverInfo = await server.TryGetServerInfoAsync(timeout: TimeSpan.FromSeconds(1));

                        if ((serverInfo?.Players ?? 0) >= 2)
                            break;
                    }

                    var playerInfos = (await server.GetServerPlayersAsync().TimeoutAfter(TimeSpan.FromSeconds(3)))
                        .ToList();

                    Assert.That(playerInfos.Count, Is.EqualTo(serverInfo.Players));

                    return;
                }
                catch (TimeoutException)
                {

                }
            }

            Assert.Fail($"Query timed out on all {GetPlayersAttempts} attempt(s)");
        }

    }
}