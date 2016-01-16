using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Query.GameServers
{
    public sealed class GameServer : SteamAgentBase
    {
        
        public GameServer(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; private set; }

        public static readonly byte[] ServerQueryHeader = {0xFF, 0xFF, 0xFF, 0xFF};

        private static BufferBuilder GetRequestPacket(GameServerQueryPacketType packetType)
        {
            var builder = new BufferBuilder();
            builder.WriteBytes(ServerQueryHeader);
            builder.WriteByte((byte)packetType);
            
            return builder;
        }

        public async Task<GameServerRules> GetServerRulesAsync()
        {
            using (var client = GetUdpClient(EndPoint))
            {
                var requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest);
                requestPacket.WriteLong(-1);

                var reader = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                var responseType = (GameServerQueryPacketType) reader.ReadByte();

                if (responseType == GameServerQueryPacketType.RulesResponse)
                    throw new NotImplementedException();

                if (responseType != GameServerQueryPacketType.RulesChallenge)
                    throw new ProtocolViolationException();

                var challengeNumber = reader.ReadLong();
                requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest); 
                requestPacket.WriteLong(challengeNumber);

                reader = await RequestResponseAsync(client, requestPacket.ToArray(), 16); //seems not to agree with protocol, would expect this 11 bytes earlier...

                responseType = (GameServerQueryPacketType) reader.ReadByte();

                if (responseType != GameServerQueryPacketType.RulesResponse)
                    throw new ProtocolViolationException();
                
                var ruleCount = reader.ReadShort();
                var rules = new List<GameServerRule>(ruleCount);
                    
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

                    rules.Add(new GameServerRule(key, value));
                }

                return new GameServerRules(rules);
            }
        }

        public async Task<GameServerInfo> GetServerInfoAsync()
        {
            using (var client = GetUdpClient(EndPoint))
            {
                var requestPacket = GetRequestPacket(GameServerQueryPacketType.InfoRequest);
                requestPacket.WriteString("Source Engine Query");

                var response = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                var responseType = (GameServerQueryPacketType) response.ReadByte();

                if (responseType != GameServerQueryPacketType.InfoResponse)
                    throw new ProtocolViolationException();

                return GameServerInfo.Parse(response);
            }
        }
        
        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}