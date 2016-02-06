using System.Net;

namespace Steam.Query.GameServers
{
    internal class GameServerInfo : IGameServerInfo
    {
        public IGameServer Server { get; private set; }
        public ushort AppId { get; private set; }
        public bool Vac { get; private set; }
        public byte ProtocolVersion { get; private set; }
        public string Name { get; private set; }
        public string Map { get; private set; }
        public string Gamedir { get; private set; }
        public string Game { get; private set; }
        public byte Players { get; private set; }
        public byte MaxPlayers { get; private set; }
        public byte Bots { get; private set; }
        public GameServerType Type { get; private set; }
        public GameServerEnvironment Environment { get; private set; }
        public bool HasPassword { get; private set; }
        public string Version { get; private set; }
        public ushort Port { get; private set; }

        internal static GameServerInfo Parse(IGameServer server, BufferReader reader)
        {
            
            var result = new GameServerInfo
            {
                Server = server,
                ProtocolVersion = reader.ReadByte(),
                Name = reader.ReadString(),
                Map = reader.ReadString(),
                Gamedir = reader.ReadString(),
                Game = reader.ReadString(),
                AppId = reader.ReadShort(),
                Players = reader.ReadByte(),
                MaxPlayers = reader.ReadByte(),
                Bots = reader.ReadByte(),
                Type = ReadType(reader),
                Environment = ReadEnvironment(reader),
                HasPassword = reader.ReadBool(),
                Vac = reader.ReadBool(),
                Version = reader.ReadString()
            };

            //get EDF
            var edf = reader.ReadByte();

            if ((edf & 0x80) != 0) //has port number
            {
                result.Port = reader.ReadShort();
            }

            return result;
        }

        private static GameServerType ReadType(BufferReader reader)
        {
            var e = reader.ReadChar();

            switch (e)
            {
                case 'd':
                    return GameServerType.Dedicated;
                case 'l':
                    return GameServerType.NonDedicated;
                case 'p':
                    return GameServerType.SpectatorProxy;
                default:
                    throw new ProtocolViolationException($"Tried to read server type, but {e} is not a legal value.");
            }

        }

        private static GameServerEnvironment ReadEnvironment(BufferReader reader)
        {
            var e = reader.ReadChar();

            switch (e)
            {
                case 'l':
                    return GameServerEnvironment.Linux;
                case 'w':
                    return GameServerEnvironment.Windows;
                case 'm':
                case 'o':
                    return GameServerEnvironment.Mac;
                default:
                    throw new ProtocolViolationException($"Tried to read server environment, but {e} is not a legal value.");
            }
            
        }

    }
}