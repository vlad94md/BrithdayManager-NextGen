using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BirthdayManager.Core.Models;

namespace BirthdayManager.Core.Dtos
{
    public class ArrangementDto
    {
        public int Id { get; set; }

        [Required]
        public string BirthdayManUsername { get; set; }

        public decimal GiftPrice { get; set; }

        public string GiftDescription { get; set; }

        public List<SubscriberDto> SubscribersUseranmes { get; set; }

        public DateTime Birthday { get; set; }


        public bool IsComplete { get; set; }
    }
}