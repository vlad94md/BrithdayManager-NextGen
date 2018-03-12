using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        public ActionResult Calendar()
        {
            return View();
        }
    }
}