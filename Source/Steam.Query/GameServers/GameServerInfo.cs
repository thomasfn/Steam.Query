namespace Steam.Query.GameServers
{
    public class GameServerInfo
    {
        public int Id { get; private set; }
        public byte Vac { get; private set; }
        public int Protocol { get; private set; }
        public string Name { get; private set; }
        public string Map { get; private set; }
        public string Folder { get; private set; }
        public string Game { get; private set; }
        public byte Players { get; private set; }
        public byte MaxPlayers { get; private set; }
        public byte Bots { get; private set; }
        public char Type { get; private set; }
        public char Environment { get; private set; }
        public byte Visibility { get; private set; }
        public string Version { get; private set; }
        public int Port { get; private set; }

        internal static GameServerInfo Parse(BufferReader reader)
        {

            var result = new GameServerInfo
            {
                Protocol = reader.ReadByte(),
                Name = reader.ReadString(),
                Map = reader.ReadString(),
                Folder = reader.ReadString(),
                Game = reader.ReadString(),
                Id = reader.ReadShort(),
                Players = reader.ReadByte(),
                MaxPlayers = reader.ReadByte(),
                Bots = reader.ReadByte(),
                Type = reader.ReadChar(),
                Environment = reader.ReadChar(),
                Visibility = reader.ReadByte(),
                Vac = reader.ReadByte(),
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
    }
}