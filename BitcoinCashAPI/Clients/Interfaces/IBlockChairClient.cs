using BitcoinCash.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IBlockChairClient
    {
        Task<List<utxo>?> GetUtxos(List<string> addresses);
        Task<List<string>?> GetValidTxHashes(List<string> hashes);
        Task<List<KeyValuePair<string, long>>?> GetWalletBalances(List<string> addresses);
    }
}
