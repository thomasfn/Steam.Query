using System.Collections.Generic;

namespace Steam.Query
{
    public class ServerRules
    {

        public IEnumerable<ServerRule> Rules => _rules;

        private readonly List<ServerRule> _rules = new List<ServerRule>();
        
        private void Add(string key, string value)
        {
            _rules.Add(new ServerRule(key, value));
        }
        
        public static ServerRules Parse(byte[] bytes)
        {
            var rules = new ServerRules();
            var parser = new BufferReader(bytes);

            parser.Skip(19); //header, which for some reason seems longer than spec...

            while (parser.HasUnreadBytes)
            {
                var key = parser.ReadString();
                var value = parser.ReadString();
                rules.Add(key, value);
            }

            return rules;
        }
    }

    public class ServerRule
    {
        public ServerRule(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}