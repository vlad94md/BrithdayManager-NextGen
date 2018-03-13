using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Enums;
using BirthdayManager.Core.Models;
using BirthdayManager.Core.ViewModels;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
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
            var payments = _context.MoneyTransactions.Include(x => x.ApplicationUser).ToList();

            return View(payments);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult New()
        {
            return View("PaymentForm", new PaymentViewModel());
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult Save(PaymentViewModel paymentViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("PaymentForm");
            }

            if (paymentViewModel.Amount == 0)
            {
                ModelState.AddModelError("", WebValidationErrorMessages.AmountIsZero);
                return View("PaymentForm");
            }

            if (paymentViewModel.Type == TransactionType.None)
            {
                ModelState.AddModelError("", WebValidationErrorMessages.InvalidTransactionType);
                return View("PaymentForm");
            }


            if (paymentViewModel.Id == 0)
            {
                var user = _context.Users.SingleOrDefault(x => x.UserName == paymentViewModel.Username);

                if (user == null || user.GetFullname() != paymentViewModel.FullName)
                {
                    ModelState.AddModelError("", WebValidationErrorMessages.UserNotFound);
                    return View("PaymentForm");
                }

                user.Balance += paymentViewModel.Amount;

                var newPayment = new MoneyTransaction()
                {
                    ApplicationUser = user,
                    Date = DateTime.Now,
                    Type = paymentViewModel.Type,
                    Amount = paymentViewModel.Amount,
                    Description = paymentViewModel.Description
                };

                _context.MoneyTransactions.Add(newPayment);
            }
            else
            {
                //TODO: add edit for payments
            }

            _context.SaveChanges();

            return RedirectToAction("List");
        }
    }
}
