using System;
using System.ComponentModel.DataAnnotations;

namespace SolutionSharingApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        [StringLength(6)]
        public string VerificationCode { get; set; } = string.Empty;

        public DateTime VerificationCodeExpiry { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "User";
    }
}
