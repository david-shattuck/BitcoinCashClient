using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Services.Interfaces;

namespace BitcoinCash.API.Services
{
    public class FiatService(ICoinGeckoClient coinGeckoClient) : IFiatService
    {
        private readonly ICoinGeckoClient _coinGeckoClient = coinGeckoClient;

        public decimal GetValue(string currency) => _coinGeckoClient.GetValue(currency);
    }
}
