namespace Steam.Query.GameServers
{
    internal enum GameServerQueryPacketType
    {
        InfoRequest = 0x54,
        InfoResponse = 0x49,

        RulesRequest = 0x56,
        RulesChallenge = 0x41,
        RulesResponse = 0x45
    }
}