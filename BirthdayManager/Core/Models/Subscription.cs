using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BirthdayManager.Core.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public int ArrangementId { get; set; }

        public ApplicationUser User { get; set; }

        public Arrangement Arrangement { get; set; }
    }
}