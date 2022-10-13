using Microsoft.EntityFrameworkCore;
using WalletPayment.Models.Entites;

namespace WalletPayment.Services.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}
