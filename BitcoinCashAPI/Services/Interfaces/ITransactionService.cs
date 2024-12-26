namespace BitcoinCash.API.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<string>?> GetValidTxHashes(List<string> hashes);
    }
}
