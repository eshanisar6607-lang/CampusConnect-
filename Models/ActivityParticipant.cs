
using System.ComponentModel.DataAnnotations;

namespace CampusKonnect.Models
{
    public class ActivityParticipant
    {
        [Key]
        public int ParticipationId { get; set; }

        public int ActivityId { get; set; }

        public Activity? Activity { get; set; }

        public string StudentId { get; set; } = string.Empty;

        public ApplicationUser? Student { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        public string ParticipationStatus { get; set; } = "Active";
    }
}
