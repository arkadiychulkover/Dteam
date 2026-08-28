using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid GameId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRecommended { get; set; } = true;
        public double PlayTimeHoursAtReview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Review content is required")]
        [MinLength(3, ErrorMessage = "Review content must be at least 3 characters")]
        [MaxLength(4000, ErrorMessage = "Review content cannot exceed 4000 characters")]
        public string Content { get; set; } = string.Empty;

        public bool IsRecommended { get; set; } = true;
    }
}
