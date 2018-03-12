using System.Data.Entity;
using BirthdayManager.Core.Models;
using BirthdayManager.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BirthdayManager.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Arrangement> Arrangements { get; set; }
        public DbSet<MoneyTransaction> MoneyTransactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}