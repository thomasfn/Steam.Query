using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Query
{
    public sealed class Server : SteamAgentBase
    {
        
        public Server(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; private set; }

        public static readonly byte[] ServerQueryHeader = {0xFF, 0xFF, 0xFF, 0xFF};

        private static BufferBuilder GetRequestPacket(ServerQueryPacketType packetType)
        {
            var builder = new BufferBuilder();
            builder.WriteBytes(ServerQueryHeader);
            builder.WriteByte((byte)packetType);
            
            return builder;
        }

        public async Task<ServerRules> GetServerRulesAsync()
        {
            using (var client = GetUdpClient(EndPoint))
            {
                var requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest);
                requestPacket.WriteLong(-1);

                var reader = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                var responseType = (ServerQueryPacketType) reader.ReadByte();

                if (responseType == ServerQueryPacketType.RulesResponse)
                    throw new NotImplementedException();

                if (responseType != ServerQueryPacketType.RulesChallenge)
                    throw new ProtocolViolationException();

                var challengeNumber = reader.ReadLong();
                requestPacket = GetRequestPacket(ServerQueryPacketType.RulesRequest); 
                requestPacket.WriteLong(challengeNumber);

                reader = await RequestResponseAsync(client, requestPacket.ToArray(), 16); //seems not to agree with protocol, would expect this 11 bytes earlier...

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
            using (var client = GetUdpClient(EndPoint))
            {
                var requestPacket = GetRequestPacket(ServerQueryPacketType.InfoRequest);
                requestPacket.WriteString("Source Engine Query");

                var response = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                var responseType = (ServerQueryPacketType) response.ReadByte();

                if (responseType != ServerQueryPacketType.InfoResponse)
                    throw new ProtocolViolationException();

                return ServerInfo.Parse(response);
            }
        }
        
        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}