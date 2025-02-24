using BitcoinCash.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IBitcoinClient
    {
        Wallet CreateWallet();
        Task<Wallet> GetWallet(string key);
        Task<List<Wallet>> GetWallets(List<string> keys);
        Task<List<Wallet>> GetWalletsByAddresses(List<string> addresses);
        Task<List<KeyValuePair<string, long>>> GetWalletBalances(List<string> addresses, bool getNullOnError = false);
        Task<List<string>> GetValidTxHashes(List<string> txHashes);
        Task<decimal> GetValue();
        Task<long> GetValueInSats(int usd);
        Task<int> GetValueInUsd(long sats);
        Task SendPayments(List<Wallet> wallets);
        Task SendPayments(List<Wallet> wallets, string address);
        bool IsAddressValid(string address);
    }
}
