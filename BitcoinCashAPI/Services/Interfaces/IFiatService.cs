namespace BitcoinCash.API.Services.Interfaces
{
    public interface IFiatService
    {
        decimal GetValue(string currency);
    }
}
