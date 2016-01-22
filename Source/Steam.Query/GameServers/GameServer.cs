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

        public IPEndPoint EndPoint { get; }

        private GameServerRules _rules;
        private GameServerInfo _info;

        private static readonly byte[] ServerQueryHeader = {0xFF, 0xFF, 0xFF, 0xFF};

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

        private static BufferBuilder GetRequestPacket(GameServerQueryPacketType packetType)
        {
            var builder = new BufferBuilder();
            builder.WriteBytes(ServerQueryHeader);
            builder.WriteEnum(packetType);
            
            return builder;
        }

        public async Task<GameServerRules> GetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            if (forceRefresh || _rules == null)
                return await QueryServerRulesAsync(timeout ?? DefaultTimeout);

            return _rules;
        }

        public async Task<GameServerRules> TryGetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            try
            {
                return await GetServerRulesAsync(forceRefresh, timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public async Task<GameServerInfo> GetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            if (forceRefresh || _info == null)
                return await QueryServerInfoAsync(timeout ?? DefaultTimeout);

            return _info;
        }

        public async Task<GameServerInfo> TryGetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            try
            {
                return await GetServerInfoAsync(forceRefresh, timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        private async Task<GameServerRules> QueryServerRulesAsync(TimeSpan timeout)
        {
            var task = Task.Run(async () =>
            {
                using (var client = GetUdpClient(EndPoint))
                {
                    var requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest);
                    requestPacket.WriteLong(-1);

                    var reader = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                    var responseType = reader.ReadEnum<GameServerQueryPacketType>();

                    if (responseType == GameServerQueryPacketType.RulesResponse)
                        throw new NotImplementedException();

                    if (responseType != GameServerQueryPacketType.RulesChallenge)
                        throw new ProtocolViolationException();

                    var challengeNumber = reader.ReadLong();
                    requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest);
                    requestPacket.WriteLong(challengeNumber);

                    reader = await RequestResponseAsync(client, requestPacket.ToArray(), 16); //seems not to agree with protocol, would expect this 11 bytes earlier...

                    responseType = reader.ReadEnum<GameServerQueryPacketType>();

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
            });

            return await task.TimeoutAfter(timeout);
        }

        private async Task<GameServerInfo> QueryServerInfoAsync(TimeSpan timeout)
        {
            var task = Task.Run(async () =>
            {
                using (var client = GetUdpClient(EndPoint))
                {
                    var requestPacket = GetRequestPacket(GameServerQueryPacketType.InfoRequest);
                    requestPacket.WriteString("Source Engine Query");

                    var response = await RequestResponseAsync(client, requestPacket.ToArray(), 4);

                    var responseType = response.ReadEnum<GameServerQueryPacketType>();

                    if (responseType != GameServerQueryPacketType.InfoResponse)
                        throw new ProtocolViolationException();

                    return GameServerInfo.Parse(response);
                }
            });

            return await task.TimeoutAfter(timeout);
        }
        
        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}