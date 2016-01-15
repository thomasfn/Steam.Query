using System.Collections.Generic;

namespace Steam.Query
{
    using System;

    public class ServerRules
    {

        public IEnumerable<ServerRule> Rules { get; }

        internal ServerRules(IEnumerable<ServerRule> rules)
        {
            Rules = rules;
        }
    }

    public class ServerRule
    {
        internal ServerRule(string key, string value)
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