using System;

namespace Steam.Query
{
    public class ServerInfo
    {
        public int ID;
        public byte VAC;
        public int Protocol { get; set; }
        public string Name { get; set; }
        public string Map { get; set; }
        public string Folder { get; set; }
        public string Game { get; set; }
        public byte Players { get; set; }
        public byte MaxPlayers { get; set; }
        public byte Bots { get; set; }
        public char Type { get; set; }
        public char Environment { get; set; }
        public byte Visibility { get; set; }
        public string Version { get; set; }
        public int Port { get; set; }

        public static ServerInfo Parse(byte[] data)
        {
            var parser = new BufferReader(data);
            parser.CurrentPosition += 5; //Header

            var result = new ServerInfo
            {
                Protocol = parser.ReadByte(),
                Name = parser.ReadString(),
                Map = parser.ReadString(),
                Folder = parser.ReadString(),
                Game = parser.ReadString(),
                ID = parser.ReadShort(),
                Players = parser.ReadByte(),
                MaxPlayers = parser.ReadByte(),
                Bots = parser.ReadByte(),
                Type = parser.ReadChar(),
                Environment = parser.ReadChar(),
                Visibility = parser.ReadByte(),
                VAC = parser.ReadByte(),
                Version = parser.ReadString()
            };

            //get EDF
            uint edf = parser.ReadByte();

            if ((edf & 0x80) != 0) //has port number
            {
                result.Port = parser.ReadShort();
            }

            return result;
        }
    }
}