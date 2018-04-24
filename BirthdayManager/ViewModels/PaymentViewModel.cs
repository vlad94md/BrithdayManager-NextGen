using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Enums;

namespace BirthdayManager.ViewModels
{
    public class PaymentViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "User should be selected.")]
        public string FullName { get; set; }

        public string Username { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionType Type { get; set; }
    }
}