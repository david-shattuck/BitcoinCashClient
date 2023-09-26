using BitcoinCash.Models;

namespace BitcoinCash.API.Services.Interfaces
{
    public interface IWalletService
    {
        List<Wallet> GetWalletInfo(List<string> addresses, string currency);
        List<KeyValuePair<string, long>> GetWalletBalances(List<string> addresses);
    }
}
