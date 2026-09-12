using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DteamBackend.Models
{
    public class ChatUpload
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual Duser User { get; set; } = null!;

        public Guid? MessageId { get; set; }
        [ForeignKey("MessageId")]
        public virtual ChatMessage? Message { get; set; }

        [Required]
        [MaxLength(256)]
        public string StorageKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public int? Duration { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsCommitted { get; set; } = false;
    }
}
