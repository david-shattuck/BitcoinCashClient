using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.DAL.Repositories.Interfaces
{
    public interface IBchTransactionRepository
    {
        List<BchTransaction> GetBchTransactions(int typeID);
        List<BchTransaction> GetStaleBchTransactions();
        void Add(BchTransaction tx);
        void SetStatus(int id, int status);
    }
}
