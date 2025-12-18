using System.ComponentModel.DataAnnotations;

namespace WorkChat2.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        public bool IsPinned { get; set; } = false;
        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }
    }
}
