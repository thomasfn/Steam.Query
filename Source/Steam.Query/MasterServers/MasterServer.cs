using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Steam.Query.GameServers;
using Steam.Query.MasterServers.Filtering;

namespace Steam.Query.MasterServers
{
    public sealed class MasterServer : IMasterServer
    {
        private readonly IPEndPoint _endpoint;

        private const int IpEndPointLength = 6;

        public MasterServer()
            : this("hl2master.steampowered.com", 27011)
        {
        }

        public MasterServer(string hostname, int port) 
            : this(Dns.GetHostEntry(hostname).AddressList[0], port)
        {
        }

        public MasterServer(IPAddress ip, int port)
            : this(new IPEndPoint(ip, port))
        {
        }

        public MasterServer(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<IEnumerable<IGameServer>> GetServersAsync(MasterServerRegion region, params IFilter[] filters)
        {
            return await GetServersAsync(new MasterServerRequest(region, filters));
        }

        public async Task<IEnumerable<IGameServer>> GetServersAsync(MasterServerRequest masterServerRequest)
        {
            var endPoints = new List<IPEndPoint>();

            var packet = 0;
            using (var client = SteamAgent.GetUdpClient(_endpoint))
            {
                while (!masterServerRequest.MaximumPackets.HasValue || packet < masterServerRequest.MaximumPackets.Value)
                {
                    var request = GetRequest(endPoints.LastOrDefault(), masterServerRequest.Region, masterServerRequest.Filters);
                    var response = await SteamAgent.RequestResponseAsync(client, request, IpEndPointLength);

                    packet++;

                    var packetEndPoints = ReadEndPointsFromPacket(response);
                    endPoints.AddRange(packetEndPoints);

                    if (endPoints.Last().IsEmpty())
                    {
                        break;
                    }
                }
            }

            return endPoints.Take(endPoints.Count - 1).Select(e => new GameServer(e));
        }

        public IEnumerable<IGameServer> GetServersLazy(MasterServerRegion region, params IFilter[] filters)
        {
            return GetServersLazy(new MasterServerRequest(region, filters));
        }

        public IEnumerable<IGameServer> GetServersLazy(MasterServerRequest masterServerRequest)
        {
            var packet = 0;
            IPEndPoint lastEndPoint = null;
            using (var client = SteamAgent.GetUdpClient(_endpoint))
            {
                while (!masterServerRequest.MaximumPackets.HasValue || packet < masterServerRequest.MaximumPackets.Value)
                {
                    var request = GetRequest(lastEndPoint, masterServerRequest.Region, masterServerRequest.Filters);
                    var response = AsyncHelper.RunSync(() => SteamAgent.RequestResponseAsync(client, request, IpEndPointLength));

                    packet++;

                    var packetEndPoints = ReadEndPointsFromPacket(response);
                    foreach (var endPoint in packetEndPoints)
                    {
                        if (endPoint.IsEmpty()) yield break;
                        yield return new GameServer(endPoint);
                        lastEndPoint = endPoint;
                    }
                }
            }
        }

        private static IEnumerable<IPEndPoint> ReadEndPointsFromPacket(BufferReader reader)
        {
            while (reader.Remaining >= IpEndPointLength)
                yield return ReadEndPoint(reader);
        }

        private static IPEndPoint ReadEndPoint(BufferReader reader)
        {
            var ipBytes = reader.ReadBytes(4);
            var port = ReadNetworkOrderShort(reader);

            return new IPEndPoint(new IPAddress(ipBytes.ToArray()), port);
        }

        private static byte[] GetRequest(IPEndPoint lastServerEndPoint, MasterServerRegion region, IEnumerable<IFilter> filters)
        {
            var packet = new BufferBuilder();
            
            packet.WriteEnum(MasterServerQueryPacketType.ServerListRequest);
            packet.WriteEnum(region);

            packet.WriteString(lastServerEndPoint?.ToString() ?? "0.0.0.0:0");
            
            packet.WriteString(filters.GetFilterCollectionString());
            
            return packet.ToArray();
        }

        private static ushort ReadNetworkOrderShort(BufferReader reader)
        {
            return BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
        }

    }
}