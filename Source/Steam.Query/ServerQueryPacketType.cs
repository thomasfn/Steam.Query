namespace Steam.Query
{
    public enum ServerQueryPacketType
    {
        
        InfoRequest = 0x54,
        RulesRequest = 0x56,
        RulesChallenge = 0x41,
        RulesResponse = 0x45,
        InfoResponse = 0x49
    }
}