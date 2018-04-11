using System.Collections.Generic;
using BirthdayManager.Core.Models;

namespace BirthdayManager.Core.ViewModels
{
    public class DetailsAdminViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        public List<MoneyTransaction> Payments { get; set; }
        public decimal ForecastBalance { get; set; }
    }
}