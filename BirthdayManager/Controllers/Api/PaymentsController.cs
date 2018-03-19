using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Enums;
using BirthdayManager.Core.Models;
using BirthdayManager.Persistence;

namespace BirthdayManager.Controllers.Api
{
    [Authorize(Roles = RoleNames.Admin)]
    public class PaymentsController : ApiController
    {
        private ApplicationDbContext _context;

        public PaymentsController()
        {
            _context = new ApplicationDbContext();
        }

        //DELETE /api/payments/1
        [HttpDelete]
        public void DeletePayment(int id)
        {
            var transaction = _context.MoneyTransactions.SingleOrDefault(c => c.Id == id);

            if (transaction == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            _context.MoneyTransactions.Remove(transaction);
            _context.SaveChanges();
        }

        //POST /api/payments/revert/1
        [HttpPost]
        [Route("api/payments/revert/{id}")]
        public IHttpActionResult RevertPayment(int id)
        {
            var transaction = _context.MoneyTransactions
                .Include(x => x.ApplicationUser)
                .SingleOrDefault(c => c.Id == id);

            if (transaction == null)
                return NotFound();

            if (transaction.IsRevertMade)
                return BadRequest("Transaction was already reverted once.");

            if (transaction.Type == TransactionType.Revert || transaction.Type == TransactionType.Withdraw)
                return BadRequest("This transaction type can't be reverted.");

            var revertedTransaction = new MoneyTransaction()
            {
                ApplicationUser = transaction.ApplicationUser,
                Amount = -transaction.Amount,
                Date = DateTime.Now,
                Description = "Revert for " + transaction.Id,
                Type = TransactionType.Revert
            };

            transaction.ApplicationUser.Balance += revertedTransaction.Amount;
            transaction.IsRevertMade = true;

            _context.MoneyTransactions.Add(revertedTransaction);
            _context.SaveChanges();

            return Ok(true);
        }    
    }
}
