using System.Net;

namespace BitcoinCash.Core.Protocol
{
    /// <summary>
    /// The kinds of object a peer can advertise or request
    /// </summary>
    public enum InventoryType
    {
        /// <summary>
        /// A transaction
        /// </summary>
        MSG_TX = 1,

        /// <summary>
        /// A block
        /// </summary>
        MSG_BLOCK = 2,

        /// <summary>
        /// A block sent without its transactions
        /// </summary>
        MSG_FILTERED_BLOCK = 3
    }

    /// <summary>
    /// The body of a peer to peer message
    /// </summary>
    public abstract class Payload
    {
        /// <summary>
        /// The name that identifies this kind of message on the wire
        /// </summary>
        public abstract string Command { get; }

        /// <summary>
        /// Write the body of this message
        /// </summary>
        /// <param name="writer">The writer to append to</param>
        public abstract void WriteTo(BitcoinWriter writer);

        /// <summary>
        /// The serialized body of this message
        /// </summary>
        /// <returns>The serialized payload</returns>
        public byte[] ToBytes()
        {
            var writer = new BitcoinWriter();

            WriteTo(writer);

            return writer.ToBytes();
        }
    }

    /// <summary>
    /// The message that opens a connection, telling the peer which protocol version
    /// and services this client offers
    /// </summary>
    public class VersionPayload : Payload
    {
        /// <inheritdoc/>
        public override string Command => "version";

        /// <summary>
        /// The protocol version this client speaks
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// The services this client offers, none of which are offered by a client that only broadcasts
        /// </summary>
        public ulong Services { get; set; }

        /// <summary>
        /// The current time
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// The address of the peer being connected to
        /// </summary>
        public IPEndPoint? AddressReceiver { get; set; }

        /// <summary>
        /// The address this client believes it can be reached on
        /// </summary>
        public IPEndPoint? AddressFrom { get; set; }

        /// <summary>
        /// A random value that lets a node recognise a connection to itself
        /// </summary>
        public ulong Nonce { get; set; }

        /// <summary>
        /// The name of this client
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// The height of the best chain this client knows about
        /// </summary>
        public int StartHeight { get; set; }

        /// <summary>
        /// Whether the peer should forward transactions to this client
        /// </summary>
        public bool Relay { get; set; } = true;

        /// <inheritdoc/>
        public override void WriteTo(BitcoinWriter writer)
        {
            writer.WriteUInt32(Version);
            writer.WriteUInt64(Services);
            writer.WriteInt64(Timestamp);

            WriteAddress(writer, AddressReceiver);
            WriteAddress(writer, AddressFrom);

            writer.WriteUInt64(Nonce);
            writer.WriteVarString(UserAgent);
            writer.WriteUInt32((uint)StartHeight);
            writer.Write((byte)(Relay ? 1 : 0));
        }

        /// <summary>
        /// Write a network address as the services, sixteen byte address and port that a
        /// version message carries. Addresses are always mapped into IPv6, and the port
        /// is the one field of the protocol written big endian.
        /// </summary>
        private static void WriteAddress(BitcoinWriter writer, IPEndPoint? endpoint)
        {
            writer.WriteUInt64(0);

            var address = (endpoint?.Address ?? IPAddress.IPv6None).MapToIPv6();

            writer.Write(address.GetAddressBytes());

            var port = (ushort)(endpoint?.Port ?? 0);

            writer.Write((byte)(port >> 8));
            writer.Write((byte)(port & 0xFF));
        }
    }

    /// <summary>
    /// The message that acknowledges a version message and completes the handshake
    /// </summary>
    public class VerAckPayload : Payload
    {
        /// <inheritdoc/>
        public override string Command => "verack";

        /// <inheritdoc/>
        public override void WriteTo(BitcoinWriter writer)
        {
        }
    }

    /// <summary>
    /// The message that advertises objects a peer may want to fetch
    /// </summary>
    public class InvPayload : Payload
    {
        private readonly List<(InventoryType Type, uint256 Hash)> _inventory = [];

        /// <summary>
        /// Advertise a single object
        /// </summary>
        /// <param name="type">The kind of object</param>
        /// <param name="hash">The hash that identifies it</param>
        public InvPayload(InventoryType type, uint256 hash) => _inventory.Add((type, hash));

        /// <summary>
        /// Advertise several objects of the same kind
        /// </summary>
        /// <param name="type">The kind of object</param>
        /// <param name="hashes">The hashes that identify them</param>
        public InvPayload(InventoryType type, params uint256[] hashes) => _inventory.AddRange(hashes.Select(h => (type, h)));

        /// <inheritdoc/>
        public override string Command => "inv";

        /// <inheritdoc/>
        public override void WriteTo(BitcoinWriter writer)
        {
            writer.WriteVarInt((ulong)_inventory.Count);

            foreach (var (type, hash) in _inventory)
            {
                writer.WriteUInt32((uint)type);
                writer.Write(hash);
            }
        }
    }

    /// <summary>
    /// The message that carries a whole transaction to a peer
    /// </summary>
    /// <param name="transaction">The transaction to send</param>
    public class TxPayload(Transaction transaction) : Payload
    {
        /// <summary>
        /// The transaction this message carries
        /// </summary>
        public Transaction Transaction { get; } = transaction;

        /// <inheritdoc/>
        public override string Command => "tx";

        /// <inheritdoc/>
        public override void WriteTo(BitcoinWriter writer) => Transaction.WriteTo(writer);
    }
}
