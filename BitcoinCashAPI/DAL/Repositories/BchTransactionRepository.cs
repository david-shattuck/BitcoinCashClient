using BitcoinCash.API.DAL.Contexts;
using BitcoinCash.API.DAL.Repositories.Interfaces;
using BitcoinCash.API.Models;
using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.DAL.Repositories
{
    public class BchTransactionRepository(BchApiContext dbContext) : IBchTransactionRepository
    {
        private readonly BchApiContext _dbContext = dbContext;

        public List<BchTransaction> GetBchTransactions(int typeID) => [.. _dbContext.BchTransaction.Where(bt => bt.PaymentType == typeID
                                                                                                                && (bt.Status == BchTransactionStatus.Confirmed
                                                                                                                || bt.Status == BchTransactionStatus.Pending))];

        public List<BchTransaction> GetStaleBchTransactions()
        {
            return [.. _dbContext.BchTransaction.Where(bt =>
                        bt.Status == BchTransactionStatus.Pending
                        && bt.Date < DateTime.UtcNow.AddHours(-1))];
        }

        public void Add(BchTransaction tx)
        {
            _dbContext.Add(tx);
            _dbContext.SaveChanges();
        }

        public void SetStatus(int id, int status)
        {
            var bchTx = _dbContext.BchTransaction.First(bt => bt.ID == id);

            bchTx.Status = status;

            _dbContext.SaveChanges();
        }
    }
}
