using System.ComponentModel.DataAnnotations;

namespace CampusKonnect.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        
        public string? Location { get; set; } 

        public int MaximumParticipants { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;

        public ApplicationUser? CreatedByUser { get; set; }

        public string Status { get; set; } = "Available";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ActivityType { get; set; } = "OnCampus";

        public string? Platform { get; set; }

        public string? MeetingLink { get; set; }

        public List<ActivityParticipant> Participants { get; set; } = new List<ActivityParticipant>();
    }
}