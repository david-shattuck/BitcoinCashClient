using BitcoinCash.API.Models;

namespace BitcoinCash.API.Clients.Interfaces
{
    public interface IRiftenClient
    {
        Task<Bcmr?> GetTokenBcmr(string category);
    }
}
