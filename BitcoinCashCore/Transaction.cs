using BitcoinCash.Core.Crypto;
using BitcoinCash.Core.DataEncoders;

namespace BitcoinCash.Core
{
    /// <summary>
    /// A reference to one output of an earlier transaction
    /// </summary>
    public class OutPoint
    {
        /// <summary>
        /// Create a reference to the given output
        /// </summary>
        /// <param name="hash">The hash of the transaction that created the output</param>
        /// <param name="n">The index of the output within that transaction</param>
        public OutPoint(uint256 hash, uint n)
        {
            Hash = hash;
            N = n;
        }

        /// <summary>
        /// The hash of the transaction that created the output
        /// </summary>
        public uint256 Hash { get; }

        /// <summary>
        /// The index of the output within that transaction
        /// </summary>
        public uint N { get; }

        /// <summary>
        /// Parse a reference written as a transaction hash and an output index separated by a colon
        /// </summary>
        /// <param name="value">The reference to parse, for example abcd...1234:0</param>
        /// <returns>The parsed reference</returns>
        /// <exception cref="FormatException">The reference is malformed</exception>
        public static OutPoint Parse(string value)
        {
            var parts = value.Split(':');

            if (parts.Length != 2 || !uint.TryParse(parts[1], out var index))
                throw new FormatException($"Invalid OutPoint {value}");

            return new OutPoint(uint256.Parse(parts[0]), index);
        }

        /// <summary>
        /// Write this reference in the wire format
        /// </summary>
        /// <param name="writer">The writer to append to</param>
        public void WriteTo(BitcoinWriter writer)
        {
            writer.Write(Hash);
            writer.WriteUInt32(N);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Hash}:{N}";
    }

    /// <summary>
    /// An input of a transaction, which spends one earlier output
    /// </summary>
    public class TxIn
    {
        /// <summary>
        /// The output this input spends
        /// </summary>
        public OutPoint? PrevOut { get; set; }

        /// <summary>
        /// The script that satisfies the spending conditions of that output
        /// </summary>
        public Script ScriptSig { get; set; } = new();

        /// <summary>
        /// The sequence number, left at its final value because this library does not
        /// use relative timelocks
        /// </summary>
        public uint Sequence { get; set; } = uint.MaxValue;

        /// <summary>
        /// Write this input in the wire format
        /// </summary>
        /// <param name="writer">The writer to append to</param>
        public void WriteTo(BitcoinWriter writer)
        {
            PrevOut!.WriteTo(writer);
            writer.Write(ScriptSig);
            writer.WriteUInt32(Sequence);
        }
    }

    /// <summary>
    /// An output of a transaction, which locks an amount behind a script
    /// </summary>
    public class TxOut
    {
        /// <summary>
        /// Create an output
        /// </summary>
        /// <param name="value">The amount to lock</param>
        /// <param name="scriptPubKey">The script that must be satisfied to spend it</param>
        public TxOut(Money value, Script scriptPubKey)
        {
            Value = value;
            ScriptPubKey = scriptPubKey;
        }

        /// <summary>
        /// The amount locked by this output
        /// </summary>
        public Money Value { get; set; }

        /// <summary>
        /// The script that must be satisfied to spend this output
        /// </summary>
        public Script ScriptPubKey { get; set; }

        /// <summary>
        /// Write this output in the wire format
        /// </summary>
        /// <param name="writer">The writer to append to</param>
        public void WriteTo(BitcoinWriter writer)
        {
            writer.WriteInt64(Value.Satoshi);
            writer.Write(ScriptPubKey);
        }
    }

    /// <summary>
    /// The inputs of a transaction
    /// </summary>
    public class TxInList : List<TxIn>
    {
    }

    /// <summary>
    /// The outputs of a transaction
    /// </summary>
    public class TxOutList : List<TxOut>
    {
        /// <summary>
        /// Append an output locking the given amount behind the given script
        /// </summary>
        /// <param name="value">The amount to lock</param>
        /// <param name="scriptPubKey">The script that must be satisfied to spend it</param>
        /// <returns>The output that was added</returns>
        public TxOut Add(Money value, Script scriptPubKey)
        {
            var output = new TxOut(value, scriptPubKey);

            Add(output);

            return output;
        }
    }

    /// <summary>
    /// An unspent output together with everything needed to sign a spend of it
    /// </summary>
    public class Coin
    {
        /// <summary>
        /// Describe an unspent output
        /// </summary>
        /// <param name="fromTxHash">The hash of the transaction that created the output</param>
        /// <param name="fromOutputIndex">The index of the output within that transaction</param>
        /// <param name="amount">The amount the output holds</param>
        /// <param name="scriptPubKey">The script that locks the output</param>
        public Coin(uint256 fromTxHash, uint fromOutputIndex, Money amount, Script scriptPubKey)
        {
            Outpoint = new OutPoint(fromTxHash, fromOutputIndex);
            TxOut = new TxOut(amount, scriptPubKey);
        }

        /// <summary>
        /// The output this coin refers to
        /// </summary>
        public OutPoint Outpoint { get; }

        /// <summary>
        /// The amount and locking script of the output
        /// </summary>
        public TxOut TxOut { get; }

        /// <summary>
        /// The amount the output holds
        /// </summary>
        public Money Amount => TxOut.Value;

        /// <summary>
        /// The script that locks the output
        /// </summary>
        public Script ScriptPubKey => TxOut.ScriptPubKey;
    }

    /// <summary>
    /// A Bitcoin Cash transaction.
    /// </summary>
    /// <remarks>
    /// Bitcoin Cash never adopted segregated witness. It instead signs with the fork id
    /// flag, which selects the same commitment scheme that BIP 143 introduced, so the
    /// amount being spent is covered by every signature.
    /// </remarks>
    public class Transaction
    {
        /// <summary>
        /// The flag Bitcoin Cash sets in a hash type to select the fork id signing scheme
        /// </summary>
        public const uint SigHashForkId = 0x40;

        /// <summary>
        /// The hash type used to sign an ordinary Bitcoin Cash transaction, committing to
        /// every input and output
        /// </summary>
        public const uint SigHashAll = 0x01;

        /// <summary>
        /// The fork id of Bitcoin Cash itself, which is zero
        /// </summary>
        public const uint ForkId = 0x00;

        /// <summary>
        /// The version of this transaction
        /// </summary>
        public uint Version { get; set; } = 1;

        /// <summary>
        /// The earliest time or block height at which this transaction may be mined
        /// </summary>
        public uint LockTime { get; set; }

        /// <summary>
        /// The inputs of this transaction
        /// </summary>
        public TxInList Inputs { get; } = [];

        /// <summary>
        /// The outputs of this transaction
        /// </summary>
        public TxOutList Outputs { get; } = [];

        /// <summary>
        /// The network this transaction belongs to
        /// </summary>
        public Network? Network { get; private set; }

        /// <summary>
        /// Create an empty transaction for the given network
        /// </summary>
        /// <param name="network">The network the transaction belongs to</param>
        /// <returns>The new transaction</returns>
        public static Transaction Create(Network network) => new() { Network = network };

        /// <summary>
        /// Serialize this transaction in the wire format
        /// </summary>
        /// <returns>The serialized transaction</returns>
        public byte[] ToBytes()
        {
            var writer = new BitcoinWriter();

            WriteTo(writer);

            return writer.ToBytes();
        }

        /// <summary>
        /// Write this transaction in the wire format
        /// </summary>
        /// <param name="writer">The writer to append to</param>
        public void WriteTo(BitcoinWriter writer)
        {
            writer.WriteUInt32(Version);

            writer.WriteVarInt((ulong)Inputs.Count);
            foreach (var input in Inputs)
                input.WriteTo(writer);

            writer.WriteVarInt((ulong)Outputs.Count);
            foreach (var output in Outputs)
                output.WriteTo(writer);

            writer.WriteUInt32(LockTime);
        }

        /// <summary>
        /// The hash that identifies this transaction
        /// </summary>
        /// <returns>The double SHA-256 of the serialized transaction</returns>
        public uint256 GetHash() => new(Hashes.Hash256(ToBytes()));

        /// <summary>
        /// The hexadecimal form of this transaction, as accepted by node consoles and explorers
        /// </summary>
        /// <returns>The serialized transaction in hexadecimal</returns>
        public string ToHex() => Hex.Encode(ToBytes());

        /// <summary>
        /// Sign every input that spends one of the given coins
        /// </summary>
        /// <param name="secret">The private key that owns the coins</param>
        /// <param name="coins">The outputs being spent</param>
        /// <exception cref="InvalidOperationException">An input cannot be matched to one of the coins</exception>
        public void Sign(BitcoinSecret? secret, IEnumerable<Coin> coins)
        {
            ArgumentNullException.ThrowIfNull(secret);
            ArgumentNullException.ThrowIfNull(coins);

            var byOutpoint = coins
                .GroupBy(c => c.Outpoint.ToString())
                .ToDictionary(g => g.Key, g => g.First());

            var hashType = SigHashAll | SigHashForkId;

            for (var index = 0; index < Inputs.Count; index++)
            {
                var input = Inputs[index];

                if (!byOutpoint.TryGetValue(input.PrevOut!.ToString(), out var coin))
                    throw new InvalidOperationException($"No coin was supplied for input {input.PrevOut}");

                var hash = GetSignatureHash(coin.ScriptPubKey, index, hashType, coin.Amount);

                byte[] signature = [.. secret.PrivateKey.Sign(hash).ToDER(), (byte)hashType];

                input.ScriptSig = Script.PayToPubkeyHashSignature(signature, secret.PubKey.ToBytes());
            }
        }

        /// <summary>
        /// Compute the hash an input signature commits to.
        /// </summary>
        /// <param name="scriptCode">The script being satisfied, which is the locking script of the output being spent</param>
        /// <param name="index">The index of the input being signed</param>
        /// <param name="hashType">The hash type, which must carry the fork id flag</param>
        /// <param name="amount">The amount held by the output being spent</param>
        /// <returns>The hash to sign</returns>
        public uint256 GetSignatureHash(Script scriptCode, int index, uint hashType, Money amount)
        {
            var writer = new BitcoinWriter();

            writer.WriteUInt32(Version);
            writer.Write(HashPrevOuts());
            writer.Write(HashSequence());
            Inputs[index].PrevOut!.WriteTo(writer);
            writer.Write(scriptCode);
            writer.WriteInt64(amount.Satoshi);
            writer.WriteUInt32(Inputs[index].Sequence);
            writer.Write(HashOutputs());
            writer.WriteUInt32(LockTime);
            writer.WriteUInt32(hashType | (ForkId << 8));

            return new uint256(Hashes.Hash256(writer.ToBytes()));
        }

        private uint256 HashPrevOuts()
        {
            var writer = new BitcoinWriter();

            foreach (var input in Inputs)
                input.PrevOut!.WriteTo(writer);

            return new uint256(Hashes.Hash256(writer.ToBytes()));
        }

        private uint256 HashSequence()
        {
            var writer = new BitcoinWriter();

            foreach (var input in Inputs)
                writer.WriteUInt32(input.Sequence);

            return new uint256(Hashes.Hash256(writer.ToBytes()));
        }

        private uint256 HashOutputs()
        {
            var writer = new BitcoinWriter();

            foreach (var output in Outputs)
                output.WriteTo(writer);

            return new uint256(Hashes.Hash256(writer.ToBytes()));
        }
    }
}
