namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IBlockChairClient
    {
        Task<List<string>?> GetValidTxHashes(List<string> hashes);
        Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses);
    }
}
