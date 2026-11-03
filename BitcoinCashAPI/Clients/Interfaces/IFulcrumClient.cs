using BitcoinCash.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IFulcrumClient
    {
        Task<List<utxo>?> GetUtxos(List<string> addresses);
    }
}
