using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Steam.Query
{
    public partial class MasterServer
    {
        private const string FIRST_AND_LAST_SERVER = "0.0.0.0:0";

        private static readonly IPEndPoint NullEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);

        private const int ADDRESS_LENGTH = 6;
        private readonly IPAddress _steamIpAddress;
        private readonly int _steamPort;

        public MasterServer()
            : this("hl2master.steampowered.com", 27011)
        {
        }

        public MasterServer(string hostname, int steamPort)
        {
            _steamIpAddress = Dns.GetHostEntry(hostname).AddressList[0];
            _steamPort = steamPort;
        }
        
        public async Task<IEnumerable<Server>> GetServersAsync(
            MasterServerRegion region = MasterServerRegion.All,
            params MasterServerFilter[] masterServerFilters)
        {
            var servers = new List<Server>();

            using (var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
            {
                client.Connect(_steamIpAddress, _steamPort);

                IPEndPoint lastServer = null;
                while (!NullEndPoint.Equals(lastServer))
                {
                    var requestPacket = CreateRequestPacket(lastServer ?? NullEndPoint, region, masterServerFilters);
                    await client.SendAsync(requestPacket, requestPacket.Length);

                    var response = await client.ReceiveAsync();
                    var responseData = response.Buffer.ToList();
                    for (var i = ADDRESS_LENGTH; i < responseData.Count; i += ADDRESS_LENGTH)
                    {
                        var ip = new IPAddress(responseData.GetRange(i, 4).ToArray());
                        var port = responseData[i + 4] << 8 | responseData[i + 5];
                        lastServer = new IPEndPoint(ip, port);
                        
                        if (!lastServer.Equals(NullEndPoint))
                        {
                            servers.Add(new Server(lastServer));
                        }
                    }
                }
            }

            return servers;
        }

        private static byte[] CreateRequestPacket(IPEndPoint lastServerEndPoint, MasterServerRegion region, IEnumerable<MasterServerFilter> filters)
        {
            var buffer = new List<byte> { 0x31, (byte)region };
            buffer.AddRange(System.Text.Encoding.ASCII.GetBytes(lastServerEndPoint.ToString()));
            buffer.Add(0x00); 
            var filtersString = string.Join("\\", filters.Select(x => x.Key + "\\" + x.Value));
            buffer.AddRange(System.Text.Encoding.ASCII.GetBytes(filtersString));
            return buffer.ToArray();
        }

    }
}