using System.Security.Claims;
using System.Threading.Tasks;
using BirthdayManager.Core.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BirthdayManager.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte DayOfBirth { get; set; }
        public byte MonthOfBirth { get; set; }
        public decimal Count { get; set; }
        public Location Location { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}