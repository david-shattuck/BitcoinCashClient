using BitcoinCash.Models;

namespace BitcoinCash.API.Services.Interfaces
{
    public interface IWalletService
    {
        Task<List<Wallet>?> GetWalletInfo(List<string> addresses, string currency);
        Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses);
    }
}
