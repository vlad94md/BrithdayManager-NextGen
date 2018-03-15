using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.ViewModels;
using BirthdayManager.Persistence;

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

            var usersWithUpcommingBirthday = users.Where(x => x.IsBirthdayNextMonth())
                .OrderBy(x => x.MonthOfBirth).ThenBy(x => x.DayOfBirth)
                .ToList();

            var viewModel = new List<CallendarArrangementViewModel>();

            foreach (var user in usersWithUpcommingBirthday)
            {
                var arrangement = new CallendarArrangementViewModel()
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Birthday = user.GetBirthdayForCurrentYear()
                };

                var arrangementFromDb = _context.Arrangements
                    .Include(x => x.Subscribers)
                    .Include(x => x.ApplicationUser)
                    .FirstOrDefault(x => x.Birthday == arrangement.Birthday && x.ApplicationUserId == arrangement.ApplicationUserId);

                var isArrangementExists = arrangementFromDb != null;

                if (isArrangementExists)
                {
                    arrangement.Id = arrangementFromDb.Id;
                    arrangement.IsComplete = arrangementFromDb.IsComplete;
                }

                viewModel.Add(arrangement);
            }

            return View(viewModel);
        }
    }
}