using System;

namespace Steam.Query
{
    public interface IGameServerPlayer
    {
        string Name { get; }

        long Score { get; }

        DateTime ConnectedAt { get; }

        TimeSpan ConnectedFor { get; }
    }
}