using System;
using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Models;

namespace BirthdayManager.Core.ViewModels
{
    public class CallendarArrangementViewModel
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public bool IsComplete { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}