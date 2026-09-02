using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class ReviewParentInfoDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid? ParentReviewId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public Guid GameId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRecommended { get; set; } = true;
        public double PlayTimeHoursAtReview { get; set; }
        public int LikesCount { get; set; }
        public bool IsLiked { get; set; }
        public int RepliesCount { get; set; }
        public List<ReviewDto> Replies { get; set; } = new();
        public ReviewParentInfoDto? ParentReview { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public Guid? ParentReviewId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Текст відгуку або коментаря обов'язковий")]
        [MinLength(1, ErrorMessage = "Текст має містити щонайменше 1 символ")]
        [MaxLength(4000, ErrorMessage = "Текст не може перевищувати 4000 символів")]
        public string Content { get; set; } = string.Empty;

        public bool IsRecommended { get; set; } = true;
    }

    public class CreateReviewCommentDto
    {
        [Required(ErrorMessage = "Текст коментаря обов'язковий")]
        [MinLength(1, ErrorMessage = "Текст має містити щонайменше 1 символ")]
        [MaxLength(4000, ErrorMessage = "Текст не може перевищувати 4000 символів")]
        public string Content { get; set; } = string.Empty;
    }
}
