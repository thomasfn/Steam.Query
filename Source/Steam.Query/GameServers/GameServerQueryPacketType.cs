namespace Steam.Query.GameServers
{
    internal enum GameServerQueryPacketType
    {
        InfoRequest = 0x54,
        InfoResponse = 0x49,

        RulesRequest = 0x56,
        RulesResponse = 0x45,

        PlayersRequest = 0x55,
        PlayersResponse = 0x44,

        RequestChallenge = 0x41
    }
}