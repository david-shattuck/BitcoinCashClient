namespace BitcoinCash.API.Services.Interfaces
{
    public interface IFiatService
    {
        Task<decimal> GetValue(string currency);
    }
}
