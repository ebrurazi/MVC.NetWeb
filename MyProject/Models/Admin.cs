using System.ComponentModel.DataAnnotations;

namespace SolutionSharingApp.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;
    }
}
