using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(64)]
        public string ClientMessageId { get; set; } = string.Empty;

        [Required]
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual Duser Sender { get; set; } = null!;

        [Required]
        public Guid ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual Duser Receiver { get; set; } = null!;

        [MaxLength(4000)]
        public string? Content { get; set; }

        public ChatMessageType Type { get; set; } = ChatMessageType.Text;
        public MessageDeliveryStatus Status { get; set; } = MessageDeliveryStatus.Sent;

        [MaxLength(256)]
        public string? StorageKey { get; set; }

        [MaxLength(256)]
        public string? OriginalFileName { get; set; }

        [MaxLength(128)]
        public string? ContentType { get; set; }

        public long? FileSize { get; set; }
        public int? Duration { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReadAt { get; set; }

        public bool IsDeletedForSender { get; set; } = false;
        public bool IsDeletedForReceiver { get; set; } = false;
    }
}
