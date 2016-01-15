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

        private async Task<BufferReader> ReceiveBufferReaderAsync(UdpClient client, int headerSize)
        {
            var response = await client.ReceiveAsync();
            var reader = new BufferReader(response.Buffer);
            reader.Skip(headerSize);

            return reader;
        }
        
        public async Task<ServerRules> GetServerRulesAsync()
        {
            using (var client = GetLocalEndpointUdpClient())
            {
                var requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest, BitConverter.GetBytes(-1));
                await SendAsync(client, requestPacket);

                var reader = await ReceiveBufferReaderAsync(client, 4);
                var responseType = (ServerQueryPacketType) reader.ReadByte();

                if (responseType == ServerQueryPacketType.RulesResponse)
                    throw new NotImplementedException();

                if (responseType != ServerQueryPacketType.RulesChallenge)
                    throw new ProtocolViolationException();

                var challengeNumber = reader.ReadSegment(4);
                requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest, challengeNumber); 

                await SendAsync(client, requestPacket);

                reader = await ReceiveBufferReaderAsync(client, 16); //seems not to agree with protocol, would expect this 11 bytes earlier...

                responseType = (ServerQueryPacketType) reader.ReadByte();

                if (responseType != ServerQueryPacketType.RulesResponse)
                    throw new ProtocolViolationException();
                
                var ruleCount = reader.ReadShort();
                var rules = new List<ServerRule>(ruleCount);
                    
                var packetsReceived = 1;
                Func<Task<BufferReader>> sequelRequestAsyncFunc = async () =>
                {
                    var next = await ReceiveBufferReaderAsync(client, 12); //protocol unclear, header size determined manually
                    packetsReceived++;
                    return next;
                };

                var multiPacketStringReader = new MultiPacketStringReader(reader, sequelRequestAsyncFunc);

                for (var i = 0; i < ruleCount; i++)
                {
                    var key = await multiPacketStringReader.ReadStringAsync();
                    var value = await multiPacketStringReader.ReadStringAsync();

                    rules.Add(new ServerRule(key, value));
                }

                return new ServerRules(rules);
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