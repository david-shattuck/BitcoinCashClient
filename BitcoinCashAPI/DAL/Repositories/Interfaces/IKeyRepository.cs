using BitcoinCash.API.Models.DatabaseModels;

namespace BitcoinCash.API.DAL.Repositories.Interfaces
{
    public interface IKeyRepository
    {
        List<Key> GetActive();
        public Key? Get(string secret);
        void Add(Key key);
        void UpdateCallsBySecret(string secret, int change);
        void UpdateCallsByAddress(string address, int change);
        void PurgeIdle();
    }
}
