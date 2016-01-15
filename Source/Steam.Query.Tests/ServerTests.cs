using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Steam.Query.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private List<Server> _servers;

        [TestFixtureSetUp]
        public void Setup()
        {
            var client = new MasterServer();
            _servers = client.GetServersAsync(MasterServerRegion.Europe, MasterServerFilter.Gamedir("tf"), MasterServerFilter.NameMatch("Valve*")).Result.ToList();
            ;
        }

        [Test]
        public void GetServerRulesAsync()
        {
            if (!_servers.Any())
                Assert.Inconclusive();

            for (var i = 0; i < 1; i++)
            {
                var t = _servers[i].GetServerRulesAsync();

                if (t.Wait(TimeSpan.FromSeconds(5)))
                {
                    var rules = t.Result.Rules.ToList();

                    Assert.That(rules.Count > 20);
                    Assert.That(rules.All(x => IsValidRuleString(x.Key)));
                    return;
                }
            }

            Assert.Fail("Tried 10 servers and nothing came back....");
        }

        private static readonly Regex RuleStringRegex = new Regex(@"^[A-Za-z0-9;,\.\-_]+$");
        private bool IsValidRuleString(string s)
        {
            return RuleStringRegex.IsMatch(s);
        }

        [Test]
        public void GetServerInfoAsync()
        {
            if (!_servers.Any())
                Assert.Inconclusive();

            for (var i = 0; i < 10; i++)
            {
                var t = _servers[i].GetServerInfoAsync();

                if (t.Wait(TimeSpan.FromSeconds(3)))
                {

                    var server = t.Result;

                    Assert.That(server.Folder, Is.EqualTo("tf"));
                    Assert.That(new[] {'w', 'l', 'm', 'o'}.Contains(server.Environment));
                    Assert.That(new[] {'d', 'l', 'p'}.Contains(server.Type));
                    Assert.That(server.Name, Is.StringStarting("Valve"));

                    return;
                }
            }
            Assert.Fail("Tried 10 servers and nothing came back....");
        }

    }
}