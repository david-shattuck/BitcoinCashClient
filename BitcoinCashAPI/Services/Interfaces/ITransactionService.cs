namespace BitcoinCash.API.Services.Interfaces
{
    public interface ITransactionService
    {
        List<string>? GetValidTxHashes(List<string> hashes);
    }
}
