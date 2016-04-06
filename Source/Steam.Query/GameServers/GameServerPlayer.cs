using System;

namespace Steam.Query.GameServers
{
    internal class GameServerPlayer : IGameServerPlayer
    {

        internal GameServerPlayer(string name, long score, DateTime connectedAt)
        {
            Name = name;
            Score = score;
            ConnectedAt = connectedAt;
        }

        public string Name { get; }

        public long Score { get; }

        public DateTime ConnectedAt { get; }

        public TimeSpan ConnectedFor => DateTime.Now.Subtract(ConnectedAt);

    }
}