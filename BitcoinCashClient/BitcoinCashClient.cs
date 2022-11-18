using BitcoinCash.Models;
using NBitcoin;
using NBitcoin.Altcoins;

namespace BitcoinCash
{
    public class BitcoinCashClient
    {
        private readonly BlockChairClient _blockChairClient;

        private readonly Network _network = BCash.Instance.Mainnet;        

        public BitcoinCashClient()
        {
            _blockChairClient = new BlockChairClient();
        }

        public Wallet GetWallet()
        {
            var rawKey = new Key();
            var secret = rawKey.GetBitcoinSecret(_network);
            var address = secret.GetAddress(ScriptPubKeyType.Legacy);

            return new Wallet
            {
                PrivateKey = secret.ToString(),
                PublicAddress = address.ToString(),
                Balance = 0
            };
        }

        public Wallet GetWallet(string privateKey)
        {
            var secret = new BitcoinSecret(privateKey, _network);
            var address = secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

            return new Wallet
            {
                PrivateKey = privateKey,
                PublicAddress = address,
                Balance = _blockChairClient.GetBalance(address)
            };
        }
    }
}