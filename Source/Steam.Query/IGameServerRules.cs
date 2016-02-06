using System.Collections.Generic;

namespace Steam.Query
{
    public interface IGameServerRules
    {
        IEnumerable<IGameServerRule> Rules { get; }
    }

    public interface IGameServerRule
    {
        string Key { get; }
        string Value { get; }
    }
}