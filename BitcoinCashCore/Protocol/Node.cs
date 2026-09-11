using System.Net;
using System.Net.Sockets;
using System.Text;
using BitcoinCash.Core.Crypto;

namespace BitcoinCash.Core.Protocol
{
    /// <summary>
    /// A connection to a Bitcoin Cash peer, enough to complete a version handshake and
    /// hand it a transaction to relay
    /// </summary>
    public sealed class Node : IDisposable
    {
        private const int HeaderSize = 24;

        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        private Node(Network network, TcpClient client, IPEndPoint peer)
        {
            Network = network;
            Peer = peer;

            _client = client;
            _stream = client.GetStream();
        }

        /// <summary>
        /// The network this connection speaks
        /// </summary>
        public Network Network { get; }

        /// <summary>
        /// The peer at the far end of this connection
        /// </summary>
        public IPEndPoint Peer { get; }

        /// <summary>
        /// True once the version handshake has completed
        /// </summary>
        public bool IsHandShaked { get; private set; }

        /// <summary>
        /// How long to wait for a peer to answer before giving up
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Open a connection to the given peer
        /// </summary>
        /// <param name="network">The network to speak</param>
        /// <param name="endpoint">The peer to connect to, as a host name or address optionally followed by a colon and a port</param>
        /// <returns>The open connection</returns>
        public static async Task<Node> ConnectAsync(Network network, string endpoint)
        {
            ArgumentNullException.ThrowIfNull(network);
            ArgumentNullException.ThrowIfNull(endpoint);

            var (host, port) = Split(endpoint, network.DefaultPort);

            var addresses = await Dns.GetHostAddressesAsync(host);

            if (addresses.Length == 0)
                throw new SocketException((int)SocketError.HostNotFound);

            var client = new TcpClient();

            try
            {
                await client.ConnectAsync(addresses, port);

                return new Node(network, client, (IPEndPoint)client.Client.RemoteEndPoint!);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        private static (string Host, int Port) Split(string endpoint, int defaultPort)
        {
            var separator = endpoint.LastIndexOf(':');

            if (separator > 0 && int.TryParse(endpoint[(separator + 1)..], out var port))
                return (endpoint[..separator], port);

            return (endpoint, defaultPort);
        }

        /// <summary>
        /// Exchange version messages with the peer, which every node requires before it
        /// will accept any other message
        /// </summary>
        public void VersionHandshake() => VersionHandshakeAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Exchange version messages with the peer, which every node requires before it
        /// will accept any other message
        /// </summary>
        /// <returns>A task that completes once the handshake is done</returns>
        public async Task VersionHandshakeAsync()
        {
            if (IsHandShaked)
                throw new InvalidOperationException("Already handshaked");

            await SendMessageAsync(CreateVersionPayload());

            await ReceiveAsync("version");

            await SendMessageAsync(new VerAckPayload());

            await ReceiveAsync("verack");

            IsHandShaked = true;
        }

        private VersionPayload CreateVersionPayload()
        {
            Span<byte> nonce = stackalloc byte[8];

            Hashes.GetRandomBytes(nonce);

            return new VersionPayload
            {
                Version = Network.MaxP2PVersion,
                Nonce = BitConverter.ToUInt64(nonce),
                UserAgent = $"/BitcoinCash.Core:{typeof(Node).Assembly.GetName().Version?.ToString(3)}/",
                AddressReceiver = Peer,
                AddressFrom = new IPEndPoint(IPAddress.Any.MapToIPv6(), Network.DefaultPort)
            };
        }

        /// <summary>
        /// Send a message to the peer
        /// </summary>
        /// <param name="payload">The message to send</param>
        /// <returns>A task that completes once the message has been written</returns>
        public async Task SendMessageAsync(Payload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);

            var body = payload.ToBytes();

            var writer = new BitcoinWriter();

            writer.WriteUInt32(Network.Magic);
            writer.Write(CommandBytes(payload.Command));
            writer.WriteUInt32((uint)body.Length);
            writer.Write(Hashes.Hash256(body).AsSpan(0, 4));
            writer.Write(body);

            await _stream.WriteAsync(writer.ToBytes());
            await _stream.FlushAsync();
        }

        private static byte[] CommandBytes(string command)
        {
            var result = new byte[12];

            Encoding.ASCII.GetBytes(command).CopyTo(result, 0);

            return result;
        }

        /// <summary>
        /// Read messages until the named one arrives, discarding anything else the peer sends
        /// </summary>
        private async Task ReceiveAsync(string command)
        {
            using var cancellation = new CancellationTokenSource(Timeout);

            while (true)
            {
                var header = new byte[HeaderSize];

                await _stream.ReadExactlyAsync(header, cancellation.Token);

                if (BitConverter.ToUInt32(ReadUInt32Bytes(header, 0)) != Network.Magic)
                    throw new FormatException("The message comes from another network");

                var received = Encoding.ASCII.GetString(header, 4, 12).TrimEnd('\0');

                var length = BitConverter.ToUInt32(ReadUInt32Bytes(header, 16));

                if (length > 0x02000000)
                    throw new FormatException("The message is too big");

                var body = new byte[length];

                if (length > 0)
                    await _stream.ReadExactlyAsync(body, cancellation.Token);

                if (received == command)
                    return;
            }
        }

        private static byte[] ReadUInt32Bytes(byte[] buffer, int offset)
        {
            var value = buffer.AsSpan(offset, 4).ToArray();

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value);

            return value;
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
            _client.Dispose();
        }
    }
}
