using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Enums;

namespace BirthdayManager.Core.ViewModels
{
    public class UserFormViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Range(1, 31)]
        [Display(Name = "Day of Birth")]
        public byte DayOfBirth { get; set; }

        [Required]
        [Range(1, 12)]
        [Display(Name = "Month of Birth")]
        public byte MonthOfBirth { get; set; }

        [Required]
        public decimal Balance { get; set; }

        [Required]
        public Location Location { get; set; }
    }
}