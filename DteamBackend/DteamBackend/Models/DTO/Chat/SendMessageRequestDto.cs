using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO.Chat
{
    public class SendMessageRequestDto
    {
        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        [MaxLength(64)]
        public string ClientMessageId { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Content { get; set; }

        public ChatMessageType Type { get; set; } = ChatMessageType.Text;

        public Guid? UploadId { get; set; }
    }
}
