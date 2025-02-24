using BitcoinCash.API.DAL.Contexts;
using BitcoinCash.API.DAL.Repositories.Interfaces;
using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.DAL.Repositories
{
    public class KeyRepository(BchApiContext dbContext) : IKeyRepository
    {
        private readonly BchApiContext _dbContext = dbContext;

        public List<Key> GetActive() => [.. _dbContext.Key.Where(k => k.LastActivity >= DateTime.UtcNow.AddMonths(-1))];

        public Key? Get(string address)
        {
            var key = _dbContext.Key.FirstOrDefault(k => k.Address == address);

            if (key == null) return key;

            key.LastActivity = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return key;
        }

        public void Add(Key key)
        {
            _dbContext.Add(key);
            _dbContext.SaveChanges();
        }

        public void UpdateCalls(string address, int change)
        {
            var key = _dbContext.Key.Single(k => k.Address == address);

            key.RemainingCalls += change;
            key.LastActivity = DateTime.UtcNow;
            _dbContext.SaveChanges();
        }
    }
}
