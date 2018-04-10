using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BirthdayManager.Core.Enums;

namespace BirthdayManager.Core.Models
{
    public class MoneyTransaction
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public string CreatedBy { get; set; }

        public bool IsRevertMade { get; set; }
    }
}