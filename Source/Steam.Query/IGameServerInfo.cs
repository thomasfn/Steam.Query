using Steam.Query.GameServers;

namespace Steam.Query
{
    public interface IGameServerInfo
    {
        IGameServer Server { get; }

        ushort AppId { get; }
        bool Vac { get; }
        byte ProtocolVersion { get; }
        string Name { get; }
        string Map { get; }
        string Gamedir { get; }
        string Game { get; }
        byte Players { get; }
        byte MaxPlayers { get; }
        byte Bots { get; }
        GameServerType Type { get; }
        GameServerEnvironment Environment { get; }
        bool HasPassword { get; }
        string Version { get; }
        ushort Ping { get; set; }
    }
}