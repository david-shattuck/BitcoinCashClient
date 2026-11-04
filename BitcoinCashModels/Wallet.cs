using BitcoinCash.Core;
using BitcoinCash.Core.Protocol;
using Newtonsoft.Json;
using SharpCashAddr;
using System.Net.Sockets;

namespace BitcoinCash.Models
{
    /// <summary>
    /// A container for money and the tools to send it
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// A very large secret number known only by the owner
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>
        /// A safe-to-share unique identifier for this wallet
        /// </summary>
        public string? PublicAddress { get; set; }

        /// <summary>
        /// The token-aware form of the public address, for receiving CashTokens
        /// </summary>
        public string? TokenAddress { get; set; }

        /// <summary>
        /// The number of satoshis held in this wallet, excluding those in CashToken utxos
        /// </summary>
        public long? Balance => _bchUtxos?.Sum(u => u.value);

        /// <summary>
        /// The current market value of the balance of this wallet
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// The fiat money denominating the value of this wallet
        /// </summary>
        public string? ValueCurrency { get; set; }

        /// <summary>
        /// The list of all unspent transaction outputs in this wallet
        /// </summary>
        public List<utxo>? utxos { get; set; }

        /// <summary>
        /// The fungible CashTokens held in this wallet, one entry per token category
        /// </summary>
        public List<FungibleToken>? FungibleTokens { get; set; }

        /// <summary>
        /// The hash of the most recent transaction broadcast by this wallet
        /// </summary>
        public string? LastTxId { get; private set; }

        /// <summary>
        /// Send the specified amount to the specified address
        /// </summary>
        /// <param name="sendTo">A valid BCH address - the recipient of this send</param>
        /// <param name="sendAmount">The amount to send </param>
        /// <param name="sendCurrency">The currency units of the send amount</param>
        /// <param name="donateToDev">Set to true to donate 0.1% of send amount to the developer of this library</param>
        public async Task Send(string sendTo, decimal sendAmount, Currency sendCurrency, bool donateToDev = true)
        {
            var sends = new List<Send>
            {
                new() {
                    To = sendTo,
                    Amount = sendAmount,
                    Currency = sendCurrency
                }
            };

            await SendToMany(sends, donateToDev);
        }        

        /// <summary>
        /// Send the entire wallet balance to the specified address
        /// </summary>
        /// <param name="sendTo">A valid BCH address - the recipient of this send</param>
        /// <param name="donateToDev">Set to true to donate 0.1% of send amount to the developer of this library</param>
        public async Task SendAll(string sendTo, bool donateToDev = true)
        {
            _sendAll = true;
            _sends = [new() { To = sendTo }];
            _donateToDev = donateToDev;

            await BaseSend();
        }

        /// <summary>
        /// Send any amount to multiple addresses in a single transaction
        /// </summary>
        /// <param name="sends">The list of recipient addresses and how much each should receive</param>
        /// <param name="donateToDev">Set to true to donate 0.1% of send amount to the developer of this library</param>
        public async Task SendToMany(List<Send> sends, bool donateToDev = true)
        {
            _sendAll = false;
            _sends = sends;
            _donateToDev = donateToDev;

            await BaseSend();
        }

        private async Task BaseSend()
        {
            VerifyUtxos();
            VerifySends();
            await SetBaseFee();
            await SetSendSats();
            SetDevDonation();
            SetAddress();
            SetSendUtxos();
            SetTotalFee();
            Create();
            Inputs();
            await Outputs();
            Sign();
            await Broadcast();
            Cleanup();
        }

        private long _baseFee;
        private long _totalFee;
        private long _sendSats;
        private long _devDonation;
        private bool _donateToDev;
        private bool _sendAll;
        private uint? _changeIndex;
        private List<utxo>? _utxos;
        private List<utxo>? _bchUtxos => utxos?.Where(u => u.token_data == null).ToList();
        private BitcoinSecret? _secret;
        private BitcoinAddress? _address;
        private List<Send>? _sends;
        private Transaction? _transaction;

        private readonly Network _network = Networks.Mainnet;

        private decimal? _bchValue
        {
            get
            {
                if (Balance == null || Value == null || Value == 0)
                    return null;

                return Value / ((decimal)Balance / Constants.SatoshiMultiplier);
            }
        }

        private async Task SetBaseFee()
        {
            if (_bchValue == null)
                throw new Exception("Unable to calculate base fee"); ;

            var baseFeeInUSD = (decimal)0.001;

            var bchValueInUSD = await GetFiatValue(Currency.USDollar);

            _baseFee = (long)(baseFeeInUSD / bchValueInUSD * Constants.SatoshiMultiplier);
        }

        private async Task SetSendSats()
        {
            if (_sendAll)
            {
                _sendSats = _bchUtxos!.Sum(u => u.value);
                return;
            }

            _sendSats = 0;

            foreach (var send in _sends!)
                _sendSats += await GetSendSatsAmount(send);
        }

        private void SetDevDonation()
        {
            _devDonation = (long)(_donateToDev ? _sendSats * 0.001 : 0);
            if (_devDonation < _baseFee * 10)
                _devDonation = 0;

            _donateToDev = _devDonation > 0;

            if (_sendAll)
                _sendSats -= _devDonation;
        }

        private void SetAddress()
        {
            if (string.IsNullOrWhiteSpace(PrivateKey))
                throw new Exception("Cannot send transaction without private key");

            _secret = new BitcoinSecret(PrivateKey, _network);
            _address = _secret.GetAddress(ScriptPubKeyType.Legacy);
        }

        private void SetSendUtxos()
        {
            if (_sendAll)
            {
                _utxos = _bchUtxos;
                return;
            }

            _utxos = [];

            var totalSend = _sendSats + _devDonation + (_sends!.Count * _baseFee) + _baseFee;

            if (_donateToDev)
                totalSend += _baseFee;

            foreach (var utxo in _bchUtxos!.OrderBy(u => u.height <= 0).ThenBy(u => Math.Abs(u.height)))
            {
                if (_utxos.Sum(u => u.value) >= totalSend)
                    break;

                _utxos.Add(utxo);
                totalSend += _baseFee;
            }

            if (_utxos.Sum(u => u.value) < totalSend)
                throw new Exception("Insufficient funds");
        }

        private void SetTotalFee()
        {
            var ioCount = _utxos!.Count + _sends!.Count + (_donateToDev ? 1 : 0) + (_sendAll ? 0 : 1);
            _totalFee = _baseFee * ioCount;

            if (_sendAll)
                _sendSats -= _totalFee;
        }

        private void Create()
        {
            _transaction = Transaction.Create(_network);
        }

        private void Inputs()
        {
            foreach (var utxo in _utxos!)
            {
                var outPointToSpend = OutPoint.Parse($"{utxo.tx_hash}:{utxo.tx_pos}");
                _transaction!.Inputs.Add(new TxIn()
                {
                    PrevOut = outPointToSpend,
                    ScriptSig = _address!.ScriptPubKey
                });
            }
        }

        private async Task Outputs()
        {
            var devAddress = BitcoinAddress.Create(Constants.DevAddress, _network);

            _changeIndex = null;

            var utxoTotal = _utxos!.Sum(u => u.value);

            var inputMoney = new Money(utxoTotal, MoneyUnit.Satoshi);
            var sendMoney = new Money(_sendSats, MoneyUnit.Satoshi);
            var devMoney = new Money(_devDonation, MoneyUnit.Satoshi);
            var minerMoney = new Money(_totalFee, MoneyUnit.Satoshi);
            var changeMoney = inputMoney - sendMoney - devMoney - minerMoney;

            foreach(var send in _sends!)
            {
                var toAddress = BitcoinAddress.Create(GetCashAddr(send.To), _network);
                var toMoney = new Money(_sendAll ? _sendSats : await GetSendSatsAmount(send), MoneyUnit.Satoshi);

                _transaction!.Outputs.Add(toMoney, toAddress!.ScriptPubKey);
            }

            if (changeMoney.Satoshi > _baseFee)
            {
                _changeIndex = (uint)_transaction!.Outputs.Count;
                _transaction!.Outputs.Add(changeMoney, _address!.ScriptPubKey);
            }                

            if (_donateToDev)
                _transaction!.Outputs.Add(devMoney, devAddress.ScriptPubKey);            
        }        

        private void Sign()
        {
            var coins = new List<Coin>();

            foreach (var utxo in _utxos!)
            {
                var txInId = uint256.Parse(utxo.tx_hash!);
                var txAmount = new Money(utxo.value, MoneyUnit.Satoshi);
                var inCoin = new Coin(txInId, utxo.tx_pos, txAmount, _address!.ScriptPubKey);
                coins.Add(inCoin);
            }

            _transaction!.Sign(_secret, coins);
        }

        private async Task Broadcast()
        {
            try
            {
                using var node = await Node.ConnectAsync(_network, "bch.greyh.at:8333");

                node.VersionHandshake();

                await node.SendMessageAsync(new InvPayload(InventoryType.MSG_TX, _transaction!.GetHash()));

                await node.SendMessageAsync(new TxPayload(_transaction));

                Thread.Sleep(500);
            }
            catch (SocketException ex)
            {                
                throw new Exception($"Broadcast SocketException: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Broadcast Exception: {ex.Message}");
            }
        }

        private void VerifyUtxos()
        {
            if (_bchUtxos == null || _bchUtxos.Count == 0)
                throw new Exception("There are no utxos to spend");
        }

        private void VerifySends()
        {
            if (_sendAll)
                return;
            
            if (_sends == null || _sends.Count == 0)
                throw new Exception("There are no sends to broadcast");

            foreach (var send in _sends)
                if (string.IsNullOrWhiteSpace(send.To) || send.Amount == null || send.Currency == null)
                    throw new Exception("Improperly formed Send");
        }

        private async Task<long> GetSendSatsAmount(Send send)
        {
            if (send.Currency!.Value == Currency.BitcoinCash.Value)
                return (long)(send.Amount! * Constants.SatoshiMultiplier);            

            if (send.Currency.Value == Currency.Satoshis.Value)
                return (long)send.Amount!;

            if (send.Currency.Value == ValueCurrency)
                return (long)(send.Amount! / _bchValue! * Constants.SatoshiMultiplier);
            else
                return (long)(send.Amount! / await GetFiatValue(send.Currency) * Constants.SatoshiMultiplier);
        }

        private static string GetCashAddr(string address)
        {
            if (address.StartsWith("bitcoincash:"))
                return address;

            if (address.StartsWith("1") || address.StartsWith("3"))
                return address.ToCashAddress();

            return string.Concat("bitcoincash:", address);
        }

        private async Task<decimal> GetFiatValue(Currency currency)
        {
            if (ValueCurrency == currency.Value)
                return (decimal)_bchValue!;

            var baseUrl = Constants.ApiUrl;

            var url = $"{baseUrl}/fiat/getvalue?currency={currency.Value}";

            var client = new HttpClient();

            var response = await client.GetAsync(url);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<decimal>(result)!;
        }

        private void Cleanup()
        {
            var txHash = _transaction!.GetHash().ToString();
            LastTxId = txHash;

            if (_sendAll)
            {
                utxos!.RemoveAll(u => u.token_data == null);
                Value = 0;
                return;
            }

            var bchValue = _bchValue;

            foreach (var utxo in _utxos!)
                utxos!.RemoveAll(u => u.tx_hash == utxo.tx_hash && u.tx_pos == utxo.tx_pos);

            if (_changeIndex != null)
            {
                utxos!.Add(new utxo
                {
                    address = _address!.ToString(),
                    height = _utxos!.Any(u => u.height <= 0) ? -1 : 0,
                    tx_hash = txHash,
                    tx_pos = _changeIndex.Value,
                    value = _utxos!.Sum(u => u.value) - _sendSats - _devDonation - _totalFee
                });

                _changeIndex = null;
            }

            Value = Balance * bchValue / Constants.SatoshiMultiplier;
        }
    }
}