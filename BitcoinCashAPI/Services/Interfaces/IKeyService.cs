using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.Services.Interfaces
{
    public interface IKeyService
    {
        Key Get();
        Key? Get(string secret);
        bool IsValid(string secret);
        bool CanGet();
        Task CheckForPayments();
    }
}
