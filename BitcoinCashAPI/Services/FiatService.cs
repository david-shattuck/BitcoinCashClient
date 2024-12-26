using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;

namespace BitcoinCash.API.Services
{
    public class FiatService(ICoinGeckoClient coinGeckoClient) : IFiatService
    {
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;

        public async Task<decimal> GetValue(string currency) => await _coinGeckoClient.GetValue(currency);
    }
}
