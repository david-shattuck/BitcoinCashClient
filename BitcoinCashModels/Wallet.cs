using NBitcoin;
using NBitcoin.Altcoins;
using NBitcoin.Protocol;

namespace BitcoinCash.Models
{
    public class Wallet
    {
        public string? PrivateKey { get; set; }
        public string? PublicAddress { get; set; }
        public uint? Balance
        {
            get
            {
                return utxos == null ? null : (uint)utxos.Sum(u => u.value);
            }
        }
        public decimal? Value { get; set; }
        public static string ValueCurrency 
        { 
            get 
            {
                return "usd";
            } 
        }
        public List<utxo>? utxos { get; set; }

        public void Send(string sendTo, decimal sendAmount, Currency sendCurrency, bool donateToDev = false)
        {
            SetBaseFee();
            SetSendSats(sendAmount, sendCurrency);
            SetDevDonation(donateToDev);
            SetAddresses(sendTo);

            BaseSend();
        }

        public void SendAll(string sendTo, bool donateToDev = false)
        {
            _sendAll = true;
            SetBaseFee();
            SetSendSats(0, Currency.Satoshis);
            SetDevDonation(donateToDev);
            SetAddresses(sendTo);

            BaseSend();
        }

        private void BaseSend()
        {
            SetSendUtxos();
            SetTotalFee();
            Create();
            Inputs();
            Outputs();
            Sign();
            Broadcast();
            Cleanup();
        }

        private uint _baseFee;
        private uint _totalFee;
        private uint _sendSats;
        private uint _devDonation;
        private bool _donateToDev;
        private bool _sendAll = false;
        private bool _makeChange = false;
        private List<utxo>? _utxos;
        private BitcoinSecret? _secret;
        private BitcoinAddress? _address;
        private BitcoinAddress? _toAddress;
        private Transaction? _transaction;

        private readonly Network _network = BCash.Instance.Mainnet;

        private decimal? _bchValue
        {
            get
            {
                if (Balance == null || Value == null || Value == 0)
                    return null;

                return Value / ((decimal)Balance / Constants.SatoshiMultiplier);
            }
        }

        private void SetBaseFee()
        {
            if (_bchValue == null)
                throw new Exception("Unable to calculate base fee"); ;

            var baseFeeInUSD = (decimal)0.001;

            _baseFee = (uint)(baseFeeInUSD / _bchValue * Constants.SatoshiMultiplier);
        }

        private void SetSendSats(decimal sendAmount, Currency sendCurrency)
        {
            if (_sendAll)
            {
                VerifyUtxos();
                _sendSats = (uint)utxos!.Sum(u => u.value);
                return;
            }

            _sendSats = sendCurrency.Value switch
            {
                "bch" => (uint)(sendAmount * Constants.SatoshiMultiplier),
                "sat" => (uint)sendAmount,
                "usd" => (uint)(sendAmount / _bchValue! * Constants.SatoshiMultiplier),
                _ => throw new Exception("Unrecognized currency code"),
            };
        }

        private void SetDevDonation(bool donate)
        {
            _devDonation = (uint)(donate ? _sendSats * 0.001 : 0);
            if (_devDonation < _baseFee * 10)
                _devDonation = 0;

            _donateToDev = _devDonation > 0;

            if (_sendAll)
                _sendSats -= _devDonation;
        }

        private void SetAddresses(string sendTo)
        {
            if (string.IsNullOrWhiteSpace(PrivateKey))
                throw new Exception("Cannot send transaction without private key");

            _secret = new BitcoinSecret(PrivateKey, _network);
            _address = _secret.GetAddress(ScriptPubKeyType.Legacy);
            _toAddress = BitcoinAddress.Create(sendTo, _network);
        }

        private void SetSendUtxos()
        {
            VerifyUtxos();

            if (_sendAll)
            {
                _utxos = utxos;
                return;
            }

            _utxos = new List<utxo>();

            var totalSend = _sendSats + _devDonation + _baseFee;

            if (_donateToDev)
                totalSend += _baseFee;

            foreach (var utxo in utxos!.OrderBy(u => u.block_id))
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
            var ioCount = _utxos!.Count + (_donateToDev ? 1 : 0) + 1;
            _totalFee = (uint)(_baseFee * ioCount);

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
                var outPointToSpend = OutPoint.Parse($"{utxo.transaction_hash}:{utxo.index}");
                _transaction!.Inputs.Add(new TxIn()
                {
                    PrevOut = outPointToSpend,
                    ScriptSig = _address!.ScriptPubKey
                });
            }
        }

        private void Outputs()
        {
            var devAddress = BitcoinAddress.Create(Constants.DevAddress, _network);

            var utxoTotal = _utxos!.Sum(u => u.value);

            var inputMoney = new Money(utxoTotal, MoneyUnit.Satoshi);
            var sendMoney = new Money(_sendSats, MoneyUnit.Satoshi);
            var devMoney = new Money(_devDonation, MoneyUnit.Satoshi);
            var minerFee = new Money(_totalFee, MoneyUnit.Satoshi);
            var changeMoney = inputMoney - sendMoney - devMoney - minerFee;

            _transaction!.Outputs.Add(sendMoney, _toAddress!.ScriptPubKey);

            if (changeMoney.Satoshi > 0)
            {
                _transaction.Outputs.Add(changeMoney, _address!.ScriptPubKey);
                _makeChange = true;
            }                

            if (_donateToDev)
                _transaction.Outputs.Add(devMoney, devAddress.ScriptPubKey);            
        }

        private void Sign()
        {
            var coins = new List<Coin>();

            foreach (var utxo in _utxos!)
            {
                var txInId = uint256.Parse(utxo.transaction_hash);
                var txAmount = new Money(utxo.value, MoneyUnit.Satoshi);
                var inCoin = new Coin(txInId, utxo.index, txAmount, _address!.ScriptPubKey);
                coins.Add(inCoin);
            }

            _transaction!.Sign(_secret, coins);
        }

        private void Broadcast()
        {
            using var node = Node.Connect(_network, "seed.bchd.cash:8333");

            node.VersionHandshake();

            node.SendMessage(new InvPayload(InventoryType.MSG_TX, _transaction!.GetHash()));

            node.SendMessage(new TxPayload(_transaction));

            Thread.Sleep(5000);
        }

        private void VerifyUtxos()
        {
            if (utxos == null || utxos.Count == 0)
                throw new Exception("There are no utxos to send");
        }

        private void Cleanup()
        {
            if (_sendAll)
            {
                utxos = new List<utxo>();
                Value = 0;
                return;
            }

            var bchValue = _bchValue;

            foreach (var utxo in _utxos!)
                utxos!.RemoveAll(u => u.transaction_hash == utxo.transaction_hash && u.index == utxo.index);

            if (_makeChange)
            {
                utxos!.Add(new utxo
                {
                    address = _address!.ToString()[12..],
                    block_id = 10000000,
                    transaction_hash = _transaction!.GetHash().ToString(),
                    index = 1,
                    value = (uint)_utxos!.Sum(u => u.value) - _sendSats - _devDonation - _totalFee
                });

                _makeChange = false;
            }

            Value = Balance * bchValue / Constants.SatoshiMultiplier;
        }
    }
}