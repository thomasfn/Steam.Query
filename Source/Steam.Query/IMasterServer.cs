using System.Collections.Generic;
using System.Threading.Tasks;
using Steam.Query.MasterServers;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query
{
    public interface IMasterServer
    {
        Task<IEnumerable<IGameServer>> GetServersAsync(MasterServerRequest masterServerRequest);
        Task<IEnumerable<IGameServer>> GetServersAsync(MasterServerRegion region, params IFilter[] filters);
        IEnumerable<IGameServer> GetServersLazy(MasterServerRequest masterServerRequest);
        IEnumerable<IGameServer> GetServersLazy(MasterServerRegion region, params IFilter[] filters);
    }
}