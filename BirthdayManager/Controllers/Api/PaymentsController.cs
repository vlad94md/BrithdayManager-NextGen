using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BirthdayManager.Core.Constants;
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
            var customerFromDb = _context.MoneyTransactions.SingleOrDefault(c => c.Id == id);

            if (customerFromDb == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            _context.MoneyTransactions.Remove(customerFromDb);
            _context.SaveChanges();
        }
    }
}
