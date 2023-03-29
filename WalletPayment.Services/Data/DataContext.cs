using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Deposit> Deposits { get; set; }

        public DbSet<SystemAccount> SystemAccounts { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{

        //    // Transaction Table
        //    //builder.Entity<Transaction>().ToTable("ucap_t_users");
        //    //builder.Entity<Transaction>().Property(t => t.Reference).HasColumnType("varchar").HasMaxLength(20).IsRequired();
        //    //builder.Entity<Transaction>().HasMany(x => x.DestinationUser)

        //    //modelBuilder.Entity<EmploymentLog>()
        //    //.HasOne(e => e.InitiatedBy) // reference
        //    //.WithMany() // no collection
        //    //.OnDelete(DeleteBehavior.NoAction);
        //}

    }
}
