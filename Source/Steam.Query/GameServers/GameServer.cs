using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Steam.Query.GameServers
{
    internal class GameServer : IGameServer
    {
        
        public GameServer(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; }
        public ushort? Ping { get; private set; }
        public bool? IsReachable { get; private set; }

        private GameServerRules _rules;
        private GameServerInfo _info;
        private List<GameServerPlayer> _players;

        private static readonly byte[] ServerQueryHeader = {0xFF, 0xFF, 0xFF, 0xFF};

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

        private static BufferBuilder GetRequestPacket(GameServerQueryPacketType packetType)
        {
            var builder = new BufferBuilder();
            builder.WriteBytes(ServerQueryHeader);
            builder.WriteEnum(packetType);
            
            return builder;
        }

        public async Task<IGameServerRules> GetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            if (forceRefresh || _rules == null)
                return await QueryServerRulesAsync(timeout ?? DefaultTimeout);

            return _rules;
        }

        public async Task<IGameServerRules> TryGetServerRulesAsync(bool forceRefresh = false, TimeSpan? timeout = null)
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

        public async Task<IGameServerInfo> GetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            if (forceRefresh || _info == null)
                return await QueryServerInfoAsync(timeout ?? DefaultTimeout);

            return _info;
        }

        public async Task<IGameServerInfo> TryGetServerInfoAsync(bool forceRefresh = false, TimeSpan? timeout = null)
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

        public async Task<IEnumerable<IGameServerPlayer>> GetServerPlayersAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            if (forceRefresh || _players == null)
                return await QueryServerPlayersAsync(timeout ?? DefaultTimeout);

            return _players;
        }

        public async Task<IEnumerable<IGameServerPlayer>> TryGetServerPlayersAsync(bool forceRefresh = false, TimeSpan? timeout = null)
        {
            try
            {
                return await GetServerPlayersAsync(forceRefresh, timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        private async Task<IEnumerable<IGameServerPlayer>> QueryServerPlayersAsync(TimeSpan timeout)
        {
            var task = Task.Run(async () =>
            {
                using (var client = SteamAgent.GetUdpClient(EndPoint))
                {
                    var requestPacket = GetRequestPacket(GameServerQueryPacketType.PlayersRequest);
                    requestPacket.WriteLong(-1);

                    var reader = await SteamAgent.RequestResponseAsync(client, requestPacket.ToArray(), 4);

                    var responseType = reader.ReadEnum<GameServerQueryPacketType>();
                    
                    if (responseType != GameServerQueryPacketType.RequestChallenge)
                        throw new ProtocolViolationException();

                    var challengeNumber = reader.ReadLong();
                    requestPacket = GetRequestPacket(GameServerQueryPacketType.PlayersRequest);
                    requestPacket.WriteLong(challengeNumber);

                    reader = await SteamAgent.RequestResponseAsync(client, requestPacket.ToArray(), 4); //seems not to agree with protocol, would expect this 11 bytes earlier...
                    var receivedAt = DateTime.Now;

                    responseType = reader.ReadEnum<GameServerQueryPacketType>();

                    if (responseType != GameServerQueryPacketType.PlayersResponse)
                        throw new ProtocolViolationException();

                    var playerCount = reader.ReadByte();
                    var players = new List<GameServerPlayer>(playerCount);

                    //var packetsReceived = 1;
                    //Func<Task<BufferReader>> sequelRequestAsyncFunc = async () =>
                    //{
                    //    var next = await SteamAgent.ReceiveBufferReaderAsync(client, 12); //protocol unclear, header size determined manually
                    //    packetsReceived++;
                    //    return next;
                    //};

                    //var multiPacketStringReader = new MultiPacketStringReader(reader, sequelRequestAsyncFunc);

                    for (var i = 0; i < playerCount; i++)
                    {
                        var position = reader.ReadByte();
                        //if (position != i)
                        //    throw new ProtocolViolationException($"Expected literal byte {i}, got {position} (player index)");

                        var name = reader.ReadString();
                        var score = reader.ReadLong();
                        var duration = reader.ReadFloat();

                        var connectedAt = receivedAt.Subtract(TimeSpan.FromSeconds(duration));

                        players.Add(new GameServerPlayer(name, score, connectedAt));
                    }

                    if (players.Count != playerCount)
                        throw new ArgumentOutOfRangeException($"Expected {playerCount} player blocks, got {players.Count}");

                    _players = players;
                    return _players;
                }
            });

            return await task.TimeoutAfter(timeout);
        }

        private async Task<IGameServerRules> QueryServerRulesAsync(TimeSpan timeout)
        {
            var task = Task.Run(async () =>
            {
                using (var client = SteamAgent.GetUdpClient(EndPoint))
                {
                    var requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest);
                    requestPacket.WriteLong(-1);

                    var reader = await SteamAgent.RequestResponseAsync(client, requestPacket.ToArray(), 4);

                    var responseType = reader.ReadEnum<GameServerQueryPacketType>();

                    if (responseType == GameServerQueryPacketType.RulesResponse)
                        throw new NotImplementedException();

                    if (responseType != GameServerQueryPacketType.RequestChallenge)
                        throw new ProtocolViolationException();

                    var challengeNumber = reader.ReadLong();
                    requestPacket = GetRequestPacket(GameServerQueryPacketType.RulesRequest);
                    requestPacket.WriteLong(challengeNumber);

                    reader = await SteamAgent.RequestResponseAsync(client, requestPacket.ToArray(), 16); //seems not to agree with protocol, would expect this 11 bytes earlier...

                    responseType = reader.ReadEnum<GameServerQueryPacketType>();

                    if (responseType != GameServerQueryPacketType.RulesResponse)
                        throw new ProtocolViolationException();

                    var ruleCount = reader.ReadShort();
                    var rules = new List<GameServerRule>(ruleCount);

                    var packetsReceived = 1;
                    Func<Task<BufferReader>> sequelRequestAsyncFunc = async () =>
                    {
                        var next = await SteamAgent.ReceiveBufferReaderAsync(client, 12); //protocol unclear, header size determined manually
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

                    _rules = new GameServerRules(rules);
                    return _rules;
                }
            });

            return await task.TimeoutAfter(timeout);
        }

        private async Task<GameServerInfo> QueryServerInfoAsync(TimeSpan timeout)
        {
            var task = Task.Run(async () =>
            {
                using (var client = SteamAgent.GetUdpClient(EndPoint))
                {
                    var requestPacket = GetRequestPacket(GameServerQueryPacketType.InfoRequest);
                    requestPacket.WriteString("Source Engine Query");

                    var pingTask = SendPing();
                    var responseTask = SteamAgent.RequestResponseAsync(client, requestPacket.ToArray(), 4);

                    await Task.WhenAll(pingTask, responseTask);

                    var response = await responseTask;
                    var ping = await pingTask;

                    var responseType = response.ReadEnum<GameServerQueryPacketType>();

                    if (responseType != GameServerQueryPacketType.InfoResponse)
                        throw new ProtocolViolationException();

                    _info = GameServerInfo.Parse(this, ping, response);
                    return _info;
                }
            });

            return await task.TimeoutAfter(timeout);
        }

        public async Task<ushort> SendPing()
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(EndPoint.Address);

            if (reply.Status != IPStatus.Success)
                throw new TimeoutException();

            return (ushort)reply.RoundtripTime;
        }

        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}