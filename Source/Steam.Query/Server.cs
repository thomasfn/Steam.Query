using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Query
{
    public class Server
    {
        
        public Server(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; private set; }

        private UdpClient GetLocalEndpointUdpClient()
        {
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            client.Connect(EndPoint);

            return client;
        }

        private List<byte> GetRequestPacket(ServerQueryPacketType packetType, IEnumerable<byte> bytes)
        {
            var requestPacket = new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF, (byte)packetType };
            requestPacket.AddRange(bytes);

            return requestPacket;
        }
        
        private async Task SendAsync(UdpClient client, IEnumerable<byte> datagramEnumerable)
        {
            var datagram = datagramEnumerable as byte[] ?? datagramEnumerable.ToArray();

            await client.SendAsync(datagram, datagram.Length);
        }
        
        public async Task<ServerRules> GetServerRulesAsync()
        {
            using (var client = GetLocalEndpointUdpClient())
            {
                var requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest, BitConverter.GetBytes(-1));
                await SendAsync(client, requestPacket);

                var response = await client.ReceiveAsync();
                var responseType = (ServerQueryPacketType) response.Buffer[4];

                if (responseType == ServerQueryPacketType.RulesResponse)
                    throw new NotImplementedException();

                if (responseType != ServerQueryPacketType.RulesChallenge)
                    throw new ProtocolViolationException();
                
                requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest, response.Buffer.Skip(5).Take(4)); //reply with challenge number
                await SendAsync(client, requestPacket);
                
                response = await client.ReceiveAsync();
                responseType = (ServerQueryPacketType)response.Buffer[16]; //seems not to agree with protocol, would expect this 11 bytes earlier...

                if (responseType != ServerQueryPacketType.RulesResponse)
                    throw new ProtocolViolationException();
                
                return ServerRules.Parse(response.Buffer); //TODO: handle multi-packet responses, which should include... most of them
            }
        }
        
        public async Task<ServerInfo> GetServerInfoAsync()
        {
            using (var client = GetLocalEndpointUdpClient())
            {
                var requestPacket = GetRequestPacket(ServerQueryPacketType.InfoRequest, Encoding.ASCII.GetBytes("Source Engine Query"));
                requestPacket.Add(0x00);

                await SendAsync(client, requestPacket);

                var response = await client.ReceiveAsync();

                var responseType = (ServerQueryPacketType)response.Buffer[4];

                if (responseType != ServerQueryPacketType.InfoResponse)
                    throw new ProtocolViolationException();

                return ServerInfo.Parse(response.Buffer);
            }
        }
        
        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}