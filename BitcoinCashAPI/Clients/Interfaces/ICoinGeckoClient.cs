namespace BitcoinCash.API.Clients.Interfaces
{
    public interface ICoinGeckoClient
    {
        Task<decimal> GetValue(string currency = "usd");
    }
}
