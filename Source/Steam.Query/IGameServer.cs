using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Query
{
    public interface IGameServer
    {
        Task<IGameServerInfo> GetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        Task<IGameServerRules> GetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        Task<IEnumerable<IGameServerPlayer>> GetServerPlayersAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        Task<IGameServerInfo> TryGetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        Task<IGameServerRules> TryGetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        Task<IEnumerable<IGameServerPlayer>> TryGetServerPlayersAsync(bool forceRefresh = false, TimeSpan? timeout = default(TimeSpan?));
        IPEndPoint EndPoint { get; }
    }
}