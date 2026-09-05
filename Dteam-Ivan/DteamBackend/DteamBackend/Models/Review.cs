using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review content cannot be empty")]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public bool IsRecommended { get; set; } = true;

        public double PlayTimeHoursAtReview { get; set; } = 0.0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

