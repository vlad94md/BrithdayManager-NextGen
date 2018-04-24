using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Enums;

namespace BirthdayManager.Core.Dtos
{
    public class UserDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

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