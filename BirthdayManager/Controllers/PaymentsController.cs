using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext _context;

        public PaymentsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Payments
        public ActionResult List()
        {
            return View();
        }
    }
}