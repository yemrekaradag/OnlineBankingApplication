using Microsoft.EntityFrameworkCore;
using OnlineBankingApplication.Entities;

namespace OnlineBankingApplication.Context
{
    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
