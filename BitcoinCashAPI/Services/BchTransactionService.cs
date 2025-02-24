using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.DAL.Repositories.Interfaces;
using BitcoinCash.API.Models;
using BitcoinCash.API.Models.DatabaseModels;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.API.Utilities.Interfaces;
using BitcoinCash.Models;

namespace BitcoinCash.API.Services
{
    public class BchTransactionService(ICipher cipher,
                                        IConfiguration configuration,
                                        IBitcoinClient bitcoinClient,
                                        IBlockChairClient blockChairClient,
                                        IBchTransactionRepository bchTransactionRepository) : IBchTransactionService
    {
        private readonly ICipher _cipher = cipher;
        private readonly IConfiguration Configuration = configuration;
        private readonly IBitcoinClient _bitcoinClient = bitcoinClient;
        private readonly IBlockChairClient _blockChairClient = blockChairClient;
        private readonly IBchTransactionRepository _bchTransactionRepository = bchTransactionRepository;

        public async Task<List<KeyValuePair<string, long>>?> GetFundedKeyBalances(List<Key> keys)
        {
            if (IsDev())
                return [];

            await CheckForFailedTxs();

            var addresses = keys.Select(k => k.Address).ToList();

            var balances = await _bitcoinClient.GetWalletBalances(addresses);

            if (balances == null || balances.Count == 0)
                return [];

            var fundedAddresses = balances.Select(b => b.Key.ToString()).ToList();

            var fundedWallets = await _bitcoinClient.GetWalletsByAddresses(fundedAddresses);

            var bchPrice = await _bitcoinClient.GetValue();
            var bchTxs = _bchTransactionRepository.GetBchTransactions(BchTransactionTypes.FundKey);

            foreach (var wallet in fundedWallets)
            {
                var key = keys.First(k => k.Address == wallet.PublicAddress);

                var utxoTxIds = wallet.utxos?.Select(u => u.transaction_hash).ToList();

                if (utxoTxIds == null || utxoTxIds.Count == 0)
                    continue;

                var keyFundingTxs = bchTxs.Where(bt => bt.ToID == key.ID).ToList();
                var keyFundingTxIds = keyFundingTxs.Where(kft => kft.Status == BchTransactionStatus.Confirmed).Select(kft => kft.TxId).ToList();

                var anyNewTxs = utxoTxIds.Any(uti => !keyFundingTxIds.Contains(uti!));

                if (!anyNewTxs)
                    continue;

                foreach (var utxo in wallet.utxos!)
                {
                    var hashTxs = bchTxs.Where(bt => bt.TxId == utxo.transaction_hash).ToList();

                    if (hashTxs.Count == 0)
                    {
                        var bchTransaction = new BchTransaction()
                        {
                            Date = DateTime.UtcNow,
                            PaymentType = BchTransactionTypes.FundKey,
                            FromType = BchTxFromToTypes.External,
                            FromID = null,
                            ToType = BchTxFromToTypes.Key,
                            ToID = key.ID,
                            TxId = utxo.transaction_hash!,
                            BchAmount = (decimal)utxo.value / Constants.SatoshiMultiplier,
                            BchPrice = bchPrice,
                            Status = BchTransactionStatus.Confirmed
                        };

                        _bchTransactionRepository.Add(bchTransaction);
                    }
                    else
                    {
                        foreach (var hashTx in hashTxs)
                            if (hashTx.Status == BchTransactionStatus.Pending)
                                _bchTransactionRepository.SetStatus(hashTx.ID, BchTransactionStatus.Confirmed);
                    }
                }                
            }

            return balances;
        }

        public async Task BuyRequests(List<Key> keys)
        {
            if (IsDev())
                return;

            var encryptedKeys = keys.Select(k => k.PrivateKey).ToList();

            var decryptedKeys = encryptedKeys.Select(_cipher.Decrypt).ToList();

            var wallets = await _bitcoinClient.GetWallets(decryptedKeys);

            var bchPrice = await _bitcoinClient.GetValue();

            foreach (var wallet in wallets)
            {
                var balance = wallet.Balance;

                if (!(balance > 0))
                    continue;

                await wallet.SendAll(Addresses.DevWallet);

                var bchTransaction = new BchTransaction()
                {
                    Date = DateTime.UtcNow,
                    PaymentType = BchTransactionTypes.BuyRequests,
                    FromType = BchTxFromToTypes.Key,
                    FromID = keys.First(k => k.Address == wallet.PublicAddress).ID,
                    ToType = BchTxFromToTypes.External,
                    ToID = null,
                    TxId = wallet.LastTxId!,
                    BchAmount = (decimal)balance / Constants.SatoshiMultiplier,
                    BchPrice = bchPrice,
                    Status = BchTransactionStatus.Pending
                };

                _bchTransactionRepository.Add(bchTransaction);
            }
        }

        private async Task CheckForFailedTxs()
        {
            if (IsDev())
                return;

            var staleTransactions = _bchTransactionRepository.GetStaleBchTransactions();

            if (staleTransactions.Count == 0)
                return;

            var staleHashes = staleTransactions.Select(st => st.TxId).ToList();

            var validHashes = await _blockChairClient.GetValidTxHashes(staleHashes);

            if (validHashes == null)
                return;

            var successTxs = staleTransactions.Where(st => validHashes.Contains(st.TxId)).ToList();
            var failedTxs = staleTransactions.Where(st => !validHashes.Contains(st.TxId)).ToList();

            foreach (var tx in successTxs)
                _bchTransactionRepository.SetStatus(tx.ID, BchTransactionStatus.Confirmed);

            foreach (var tx in failedTxs)
                _bchTransactionRepository.SetStatus(tx.ID, BchTransactionStatus.Failed);
        }

        private bool IsDev() => Configuration["Environment"] != Models.Environments.Production;
    }
}
