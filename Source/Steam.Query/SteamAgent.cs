using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Steam.Query
{
    internal static class SteamAgent
    {

        public static UdpClient GetUdpClient(IPEndPoint remote)
        {
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            client.Connect(remote);

            return client;
        }

        public static async Task SendAsync(UdpClient client, IEnumerable<byte> datagramEnumerable)
        {
            var datagram = datagramEnumerable as byte[] ?? datagramEnumerable.ToArray();

            await client.SendAsync(datagram, datagram.Length);
        }

        public static async Task<BufferReader> ReceiveBufferReaderAsync(UdpClient client, int headerSize)
        {
            var response = await client.ReceiveAsync();
            var reader = new BufferReader(response.Buffer);
            reader.Skip(headerSize);

            return reader;
        }

        public static async Task<BufferReader> RequestResponseAsync(UdpClient client, IEnumerable<byte> datagram, int responseHeaderSize)
        {
            await SendAsync(client, datagram);
            return await ReceiveBufferReaderAsync(client, responseHeaderSize);
        }

    }
}