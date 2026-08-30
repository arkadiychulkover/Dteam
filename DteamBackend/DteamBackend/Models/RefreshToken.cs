using System;

namespace DteamBackend.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;
    }
}