using System;
using System.ComponentModel.DataAnnotations;
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

        [Range(1,31)]
        public byte DayOfBirth { get; set; }

        [Range(1, 12)]
        public byte MonthOfBirth { get; set; }

        public decimal Balance { get; set; }

        public Location Location { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }

        public string GetFullname()
        {
            return FirstName + " " + LastName;
        }

        public string GetBirthdate()
        {
            var date = new DateTime(1999, MonthOfBirth, DayOfBirth);
            return date.ToString("MMMM dd");
        }

        public DateTime GetBirthdayForCurrentYear()
        {
            return new DateTime(DateTime.Now.Year, MonthOfBirth, DayOfBirth);
        }

        public bool IsBirthdayUppcommingForDaysPeriod(int period = 20)
        {
            if (MonthOfBirth == 0 || DayOfBirth == 0)
                return false;

            var date = new DateTime(DateTime.Now.Year, MonthOfBirth, DayOfBirth);

            return DateTime.Now < date && DateTime.Now.AddDays(period) > date;
        }

        public bool IsBirthdayPastForDaysPeriod(int period = 20)
        {
            if (MonthOfBirth == 0 || DayOfBirth == 0)
                return false;

            var date = new DateTime(DateTime.Now.Year, MonthOfBirth, DayOfBirth);

            return DateTime.Now > date && DateTime.Now.AddDays(-period) < date;
        }

        public string GetLocation()
        {
            switch (Location)
            {
                case Location.Tower:
                    return "Tower";
                case Location.NBC:
                    return "NBC";
                case Location.Bulgara:
                    return "Bulgara";
                default:
                     return "None";
            }
        }
    }
}