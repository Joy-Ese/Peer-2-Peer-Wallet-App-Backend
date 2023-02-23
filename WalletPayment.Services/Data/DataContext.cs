using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Data
{
    public class DataContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder
        //            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Peer2PeerWalletPaymentdb;Trusted_Connection=True;");
        //    }
        //}

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Deposit> Deposits { get; set; }

    }
}
