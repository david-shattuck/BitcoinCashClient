using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;

namespace BitcoinCash.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IBlockChairClient _blockChairClient;

        public TransactionService(IBlockChairClient blockChairClient)
        {
            _blockChairClient = blockChairClient;
        }

        public List<string> GetValidTxHashes(List<string> hashes) => _blockChairClient.GetValidTxHashes(hashes);
    }
}
