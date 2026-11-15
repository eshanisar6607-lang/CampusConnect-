
using System.ComponentModel.DataAnnotations;

namespace CampusKonnect.Models
{
    public class CreateActivityViewModel
    {
        [Required(ErrorMessage = "Activity title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date and time are required.")]
        public DateTime ScheduledAt { get; set; }

        public string? Location { get; set; }

        [Required]
        public string ActivityType { get; set; } = "OnCampus";

        public string? Platform { get; set; }

        public string? MeetingLink { get; set; }

        [Range(2, 500, ErrorMessage = "Maximum participants must be between 2 and 500.")]
        public int MaximumParticipants { get; set; }
    }
}

