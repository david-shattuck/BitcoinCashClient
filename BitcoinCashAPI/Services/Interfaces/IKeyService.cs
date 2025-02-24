namespace BitcoinCash.API.Services.Interfaces
{
    public interface IKeyService
    {
        string GetKey();
        int? GetCalls(string address);
        bool IsValid(string address);
        bool CanGetKey();
        Task CheckForPayments();
    }
}
