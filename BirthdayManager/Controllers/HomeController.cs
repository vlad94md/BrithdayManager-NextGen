using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var users = _context.Users.ToList();

            var nextMonthBirthdays = users.Where(x => x.IsBirthdayNextMonth())
                .OrderBy(x => x.MonthOfBirth).ThenBy(x => x.DayOfBirth)
                .Select(x => x.GetFullname() + ", " + x.GetBirthdate())
                .ToList();

            return View(nextMonthBirthdays);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}