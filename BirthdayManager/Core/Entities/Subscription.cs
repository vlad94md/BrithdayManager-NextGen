using System.ComponentModel.DataAnnotations;

namespace BirthdayManager.Core.Entities
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