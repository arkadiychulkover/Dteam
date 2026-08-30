using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public class FriendRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SenderId { get; set; }
        public Duser Sender { get; set; } = null!;

        [Required]
        public Guid ReceiverId { get; set; }
        public Duser Receiver { get; set; } = null!;

        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }
    }
}
