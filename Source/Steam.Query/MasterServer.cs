using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Query
{
    public sealed class MasterServer : SteamAgentBase
    {
        private readonly IPEndPoint _endpoint;
        private static readonly IPEndPoint NullEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);

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

        public async Task<IEnumerable<Server>> GetServersAsync(MasterServerRegion region = MasterServerRegion.All, params MasterServerFilter[] masterServerFilters)
        {
            var servers = new List<Server>();
            using (var client = GetUdpClient(_endpoint))
            {
                IPEndPoint lastServerEndPoint = null;

                while (!NullEndPoint.Equals(lastServerEndPoint))
                {
                    var request = GetRequest(lastServerEndPoint ?? NullEndPoint, region, masterServerFilters);
                    var response = await RequestResponseAsync(client, request, IpEndPointLength);

                    while (response.Remaining >= IpEndPointLength)
                    {
                        lastServerEndPoint = ReadEndPoint(response);

                        if (!lastServerEndPoint.Equals(NullEndPoint))
                        {
                            servers.Add(new Server(lastServerEndPoint));
                        }
                    }
                }
            }

            return servers;
        }

        private static IPEndPoint ReadEndPoint(BufferReader reader)
        {
            var ipBytes = reader.ReadBytes(4);
            var port = ReadNetworkOrderShort(reader);

            return new IPEndPoint(new IPAddress(ipBytes.ToArray()), port);
        }

        private static byte[] GetRequest(IPEndPoint lastServerEndPoint, MasterServerRegion region, IEnumerable<MasterServerFilter> filters)
        {
            var packet = new BufferBuilder();
            
            packet.WriteByte((byte)MasterServerQueryPacketType.ServerListRequest);
            packet.WriteByte((byte)region);

            packet.WriteString(lastServerEndPoint.ToString());
            ;
            var filterStrings = new[] {""}.Concat(filters.Select(x => x.Key + "\\" + x.Value));
            var filterList = string.Join("\\", filterStrings);
            packet.WriteString(filterList);
            
            return packet.ToArray();
        }

        private static ushort ReadNetworkOrderShort(BufferReader reader)
        {
            return BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
        }

    }
}