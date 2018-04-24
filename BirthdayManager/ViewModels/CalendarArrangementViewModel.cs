using System;
using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Entities;

namespace BirthdayManager.ViewModels
{
    public class CalendarArrangementViewModel
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