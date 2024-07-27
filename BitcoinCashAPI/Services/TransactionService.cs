using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;

namespace BitcoinCash.API.Services
{
    public class TransactionService(IBlockChairClient blockChairClient) : ITransactionService
    {
        private readonly IBlockChairClient _blockChairClient = blockChairClient;

        public List<string> GetValidTxHashes(List<string> hashes) => _blockChairClient.GetValidTxHashes(hashes);
    }
}
