using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BirthdayManager.Core.Models
{
    public class Arrangement
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        public decimal GiftPrice { get; set; }

        public string GiftDescription { get; set; }

        public List<Subscription> Subscribers { get; set; }

        public DateTime Birthday { get; set; }

        public bool IsComplete { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}