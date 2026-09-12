using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class SendFriendRequestDto
    {
        public Guid? ReceiverId { get; set; }
        public string? ReceiverUsername { get; set; }
    }

    public class FriendRequestDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public Guid ReceiverId { get; set; }
        public string ReceiverUsername { get; set; } = string.Empty;
        public string? ReceiverAvatarUrl { get; set; }
        public FriendRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FriendDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime FriendsSince { get; set; }
    }

    public class FriendActionResponseDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
