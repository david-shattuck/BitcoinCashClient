namespace BitcoinCash.API.Clients.Interfaces
{
    public interface ICoinGeckoClient
    {
        decimal GetValue(string currency = "usd");
    }
}
