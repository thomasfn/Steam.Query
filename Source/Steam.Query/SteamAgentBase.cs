using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Steam.Query
{
    public abstract class SteamAgentBase
    {
        
        protected UdpClient GetUdpClient(IPEndPoint remote)
        {
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            client.Connect(remote);

            return client;
        }

        protected async Task SendAsync(UdpClient client, IEnumerable<byte> datagramEnumerable)
        {
            var datagram = datagramEnumerable as byte[] ?? datagramEnumerable.ToArray();

            await client.SendAsync(datagram, datagram.Length);
        }
        
        protected async Task<BufferReader> ReceiveBufferReaderAsync(UdpClient client, int headerSize)
        {
            var response = await client.ReceiveAsync();
            var reader = new BufferReader(response.Buffer);
            reader.Skip(headerSize);

            return reader;
        }

        protected async Task<BufferReader> RequestResponseAsync(UdpClient client, IEnumerable<byte> datagram, int responseHeaderSize)
        {
            await SendAsync(client, datagram);
            return await ReceiveBufferReaderAsync(client, responseHeaderSize);
        }

    }
}