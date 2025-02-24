using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.DAL.Repositories.Interfaces
{
    public interface IKeyRepository
    {
        List<Key> GetActive();
        public Key? Get(string address);
        void Add(Key key);
        void UpdateCalls(string address, int change);
    }
}
