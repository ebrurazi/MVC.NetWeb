using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SolutionSharingApp.Models
{
    public class Solution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        [Required]
        public string Problem { get; set; } = string.Empty;

        [Required]
        public string SolutionContent { get; set; } = string.Empty;

        public List<string>? Photos { get; set; } = new List<string>();
        public List<string>? PhotoDescriptions { get; set; } = new List<string>();

        public List<string>? Files { get; set; } = new List<string>();
        public List<string>? FileDescriptions { get; set; } = new List<string>();

        public string? Photo2 { get; set; }
        public string? Photo2Description { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string? Status { get; set; } = "Pending";
        public string? RejectReason { get; set; } = string.Empty;
        public string? ReturnReason { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; } = 0;

        public string? Keywords { get; set; } // Anahtar kelimeleri virgülle ayrılmış dize olarak saklayacağız
        public float[]? Vector { get; set; }
    }
}
