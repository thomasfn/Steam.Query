using System.Collections.Generic;

namespace Steam.Query.GameServers
{
    public class GameServerRules
    {

        public IEnumerable<GameServerRule> Rules { get; }

        internal GameServerRules(IEnumerable<GameServerRule> rules)
        {
            Rules = rules;
        }
    }

    public class GameServerRule
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