using BitcoinCash.API.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace BitcoinCash.API.DAL.Contexts
{
    public class BchApiContext(DbContextOptions<BchApiContext> options) : DbContext(options)
    {
        public DbSet<BchTransaction> BchTransaction { get; set; }
        public DbSet<Key> Key { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BchTransaction>().Property(p => p.BchAmount).HasPrecision(18, 8);
            modelBuilder.Entity<BchTransaction>().Property(p => p.BchPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Key>().HasKey(k => k.ID).IsClustered(false);
            modelBuilder.Entity<Key>().HasIndex(k => k.Address).IsClustered(true);
        }
    }
}
