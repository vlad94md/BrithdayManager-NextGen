using System.Data.Entity;
using BirthdayManager.Core.Models;
using BirthdayManager.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BirthdayManager.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        
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