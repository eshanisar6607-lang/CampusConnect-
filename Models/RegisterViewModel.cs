
using System.ComponentModel.DataAnnotations;

namespace CampusKonnect.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [RegularExpression(
            @"^\d{4}-[A-Za-z0-9]+-\d+@student\.uet\.edu\.pk$",
            ErrorMessage = "Please enter a valid UET student email, e.g. 2024-SE-255@student.uet.edu.pk"
        )]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
