using BitcoinCash.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IBlockChairClient
    {
        List<utxo>? GetUtxos(List<string> addresses);
        List<string>? GetValidTxHashes(List<string> hashes);
        List<KeyValuePair<string, long>>? GetWalletBalances(List<string> addresses);
    }
}
