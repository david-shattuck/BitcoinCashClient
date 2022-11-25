using BitcoinCash.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IBlockChairClient
    {
        List<utxo> GetUtxos(List<string> addresses);
    }
}
