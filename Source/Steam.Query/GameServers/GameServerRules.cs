using System.Collections.Generic;

namespace Steam.Query.GameServers
{
    internal class GameServerRules : IGameServerRules
    {

        public IEnumerable<IGameServerRule> Rules { get; }

        internal GameServerRules(IEnumerable<GameServerRule> rules)
        {
            Rules = rules;
        }
    }

    internal class GameServerRule : IGameServerRule
    {
        internal GameServerRule(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }

        public override string ToString()
        {
            return $"{Key} {Value}";
        }
    }
}