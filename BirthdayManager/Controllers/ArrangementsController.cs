using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayManager.Core.Constants;
using BirthdayManager.Data;
using BirthdayManager.ViewModels;

namespace BirthdayManager.Controllers
{
    public class ArrangementsController : Controller
    {
        private ApplicationDbContext _context;

        public ArrangementsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Arrangements
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult Calendar()
        {
            var users = _context.Users.ToList();

            var usersWithUpcommingBirthday = users.Where(x => x.IsBirthdayUppcommingForDaysPeriod(20))
                .OrderBy(x => x.MonthOfBirth).ThenBy(x => x.DayOfBirth)
                .ToList();

            var usersWithRecentPastBirthday = users.Where(x => x.IsBirthdayPastForDaysPeriod(20))
                .OrderByDescending(x => x.MonthOfBirth).ThenByDescending(x => x.DayOfBirth)
                .ToList();


            //Remove current usery so user cant see his own birthday
            var currrentUserName = User.Identity.Name;
            usersWithUpcommingBirthday.RemoveAll(x => x.UserName == currrentUserName);
            usersWithRecentPastBirthday.RemoveAll(x => x.UserName == currrentUserName);

            var upcommingBirthdays = new List<CalendarArrangementViewModel>();
            var recentPastBirthdays = new List<CalendarArrangementViewModel>();

            foreach (var user in usersWithUpcommingBirthday)
            {
                var arrangement = new CalendarArrangementViewModel()
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Birthday = user.GetBirthdayForCurrentYear()
                };

                var arrangementFromDb = _context.Arrangements
                    .FirstOrDefault(x => x.Birthday == arrangement.Birthday && x.ApplicationUserId == arrangement.ApplicationUserId);

                var isArrangementExists = arrangementFromDb != null;

                if (isArrangementExists)
                {
                    arrangement.Id = arrangementFromDb.Id;
                    arrangement.IsComplete = arrangementFromDb.IsComplete;
                }

                upcommingBirthdays.Add(arrangement);
            }

            foreach (var user in usersWithRecentPastBirthday)
            {
                var arrangement = new CalendarArrangementViewModel()
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Birthday = user.GetBirthdayForCurrentYear()
                };

                var arrangementFromDb = _context.Arrangements
                    .FirstOrDefault(x => x.Birthday == arrangement.Birthday && x.ApplicationUserId == arrangement.ApplicationUserId);

                var isArrangementExists = arrangementFromDb != null;

                if (isArrangementExists)
                {
                    arrangement.Id = arrangementFromDb.Id;
                    arrangement.IsComplete = arrangementFromDb.IsComplete;
                }

                recentPastBirthdays.Add(arrangement);
            }

            var viewModel = new CalendarViewModel()
            {
                UpcommingBirthdays = upcommingBirthdays,
                RecentPastBirthdays = recentPastBirthdays
            };

            return View(viewModel);
        }
    }
}