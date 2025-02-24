using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.Services.Interfaces
{
    public interface IBchTransactionService
    {
        Task<List<KeyValuePair<string, long>>?> GetFundedKeyBalances(List<Key> keys);
        Task BuyRequests(List<Key> keys);
    }
}
