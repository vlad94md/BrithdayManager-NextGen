using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext _context;

        public UsersController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpGet]
        [Route("Users")]
        public ActionResult List()
        {
            var users = _context.Users.ToList();

            return View(users);
        }

        [HttpGet]
        public ActionResult Details(string username)
        {
            var user = _context.Users.SingleOrDefault(x => x.UserName == username);

            if (user == null)
                return HttpNotFound();


            return View(user);
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
    }
}