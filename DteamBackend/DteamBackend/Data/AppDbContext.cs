using DteamBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace DteamBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Duser> Users => Set<Duser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UserGame> UserGames => Set<UserGame>();
        public DbSet<UserFriend> UserFriends => Set<UserFriend>();
        public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
        public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
        public DbSet<UserWishlist> UserWishlists => Set<UserWishlist>();
        public DbSet<UserCartItem> UserCartItems => Set<UserCartItem>();
        public DbSet<Tranxaction> Tranxactions => Set<Tranxaction>();
        public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
        public DbSet<CommunityComment> CommunityComments => Set<CommunityComment>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ChatUpload> ChatUploads => Set<ChatUpload>();
        public DbSet<UserActivity> UserActivities => Set<UserActivity>();
        public DbSet<NftItem> NftItems => Set<NftItem>();
        public DbSet<NftTransfer> NftTransfers => Set<NftTransfer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Duser>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.HasIndex(u => u.Username)
                    .IsUnique();

                entity.HasIndex(u => u.WalletAddress);

                entity.HasOne(u => u.FamilyOwner)
                    .WithMany(u => u.FamilyMembers)
                    .HasForeignKey(u => u.FamilyOwnerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(u => u.Friends)
                    .WithMany()
                    .UsingEntity<UserFriend>(
                        j => j.HasOne(uf => uf.Friend).WithMany().HasForeignKey(uf => uf.FriendId).OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne(uf => uf.User).WithMany(u => u.Friendships).HasForeignKey(uf => uf.UserId).OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey(uf => new { uf.UserId, uf.FriendId });
                            j.ToTable("UserFriends");
                        });

                entity.HasMany(u => u.BlockedUsers)
                    .WithMany()
                    .UsingEntity<UserBlock>(
                        j => j.HasOne(ub => ub.BlockedUser).WithMany().HasForeignKey(ub => ub.BlockedUserId).OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne(ub => ub.User).WithMany().HasForeignKey(ub => ub.UserId).OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey(ub => new { ub.UserId, ub.BlockedUserId });
                            j.ToTable("UserBlocks");
                        });
            });

            modelBuilder.Entity<UserGame>(entity =>
            {
                entity.HasKey(ug => new { ug.UserId, ug.GameId });

                entity.HasOne(ug => ug.User)
                    .WithMany(u => u.Library)
                    .HasForeignKey(ug => ug.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Game)
                    .WithMany(g => g.Owners)
                    .HasForeignKey(ug => ug.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FriendRequest>(entity =>
            {
                entity.HasKey(fr => fr.Id);

                entity.HasOne(fr => fr.Sender)
                    .WithMany(u => u.SentFriendRequests)
                    .HasForeignKey(fr => fr.SenderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fr => fr.Receiver)
                    .WithMany(u => u.ReceivedFriendRequests)
                    .HasForeignKey(fr => fr.ReceiverId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(fr => new { fr.SenderId, fr.ReceiverId });
            });

            modelBuilder.Entity<UserWishlist>(entity =>
            {
                entity.HasKey(w => new { w.UserId, w.GameId });

                entity.HasOne(w => w.User)
                    .WithMany(u => u.Wishlist)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Game)
                    .WithMany(g => g.WishlistedBy)
                    .HasForeignKey(w => w.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserCartItem>(entity =>
            {
                entity.HasKey(c => new { c.UserId, c.GameId });

                entity.HasOne(c => c.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Game)
                    .WithMany(g => g.InCartsOf)
                    .HasForeignKey(c => c.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            var languageListComparer = new ValueComparer<List<GameLanguageSupport>>(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.Id);

                entity.HasOne(g => g.Owner)
                    .WithMany(u => u.CreatedGames)
                    .HasForeignKey(g => g.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.ParentGame)
                    .WithMany(g => g.Dlcs)
                    .HasForeignKey(g => g.ParentGameId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(g => g.SupportedLanguages)
                    .IsRequired(false)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v ?? new List<GameLanguageSupport>(), (JsonSerializerOptions?)null),
                        v => string.IsNullOrEmpty(v) ? new List<GameLanguageSupport>() : (JsonSerializer.Deserialize<List<GameLanguageSupport>>(v, (JsonSerializerOptions?)null) ?? new List<GameLanguageSupport>())
                    )
                    .Metadata.SetValueComparer(languageListComparer);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => new { r.UserId, r.GameId })
                    .IsUnique()
                    .HasFilter("\"ParentReviewId\" IS NULL");

                entity.HasIndex(r => r.ParentReviewId);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Game)
                    .WithMany(g => g.Reviews)
                    .HasForeignKey(r => r.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.ParentReview)
                    .WithMany(r => r.Replies)
                    .HasForeignKey(r => r.ParentReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.LikedByUsers)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                    );

                entity.Property(r => r.LikedByUsers)
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    ));
            });

            modelBuilder.Entity<Tranxaction>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.HasIndex(t => t.TxhHash)
                    .IsUnique();

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CommunityPost>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.GameId);
                entity.HasIndex(p => p.Category);
                entity.HasIndex(p => p.CreatedAt);

                entity.OwnsOne(p => p.Author, a =>
                {
                    a.Property(x => x.Id).HasColumnName("AuthorId");
                    a.Property(x => x.Username).HasColumnName("AuthorUsername");
                    a.Property(x => x.AvatarUrl).HasColumnName("AuthorAvatarUrl");
                });

                entity.OwnsOne(p => p.Media, m =>
                {
                    m.Property(x => x.Type).HasColumnName("MediaType");
                    m.Property(x => x.Url).HasColumnName("MediaUrl");
                    m.Property(x => x.ThumbnailUrl).HasColumnName("MediaThumbnailUrl");
                });

                var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                );

                entity.Property(p => p.LikedByUsers)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => string.IsNullOrEmpty(v) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                    )
                    .Metadata.SetValueComparer(stringListComparer);

                entity.HasOne(p => p.Game)
                    .WithMany(g => g.CommunityPosts)
                    .HasForeignKey(p => p.GameGuidId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.Comments)
                    .WithOne(c => c.Post)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CommunityComment>(entity =>
            {
                var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                );

                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.PostId);
                entity.HasIndex(c => c.ParentCommentId);

                entity.OwnsOne(c => c.Author, a =>
                {
                    a.Property(x => x.Id).HasColumnName("AuthorId");
                    a.Property(x => x.Username).HasColumnName("AuthorUsername");
                    a.Property(x => x.AvatarUrl).HasColumnName("AuthorAvatarUrl");
                });

                entity.Property(c => c.LikedByUsers)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => string.IsNullOrEmpty(v) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                    )
                    .Metadata.SetValueComparer(stringListComparer);

                entity.HasMany(c => c.Replies)
                    .WithOne(r => r.ParentComment)
                    .HasForeignKey(r => r.ParentCommentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                    .WithMany()
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.CreatedAt)
                    .HasConversion(
                        v => v.ToUnixTimeMilliseconds(),
                        v => DateTimeOffset.FromUnixTimeMilliseconds(v)
                    );

                entity.Property(m => m.ReadAt)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                        v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null
                    );

                entity.HasIndex(m => new { m.SenderId, m.ClientMessageId })
                    .IsUnique();

                entity.HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt });
                entity.HasIndex(m => new { m.ReceiverId, m.SenderId, m.CreatedAt });
                entity.HasIndex(m => new { m.ReceiverId, m.Status, m.CreatedAt });
            });

            modelBuilder.Entity<ChatUpload>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.CreatedAt)
                    .HasConversion(
                        v => v.ToUnixTimeMilliseconds(),
                        v => DateTimeOffset.FromUnixTimeMilliseconds(v)
                    );

                entity.HasOne(u => u.User)
                    .WithMany()
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.Message)
                    .WithMany()
                    .HasForeignKey(u => u.MessageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(u => u.UserId);
            });

            modelBuilder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Activities)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => new { a.UserId, a.CreatedAt });
            });

            modelBuilder.Entity<NftItem>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => n.TokenId);
                entity.HasIndex(n => n.Rarity);
                entity.HasIndex(n => n.IsMinted);
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => new { n.BackgroundIndex, n.PatternIndex, n.ModelIndex }).IsUnique();

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Gifts)
                    .HasForeignKey(n => n.UserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<NftTransfer>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.NftItemId);
                entity.HasIndex(t => t.TokenId);
                entity.HasIndex(t => t.FromAddress);
                entity.HasIndex(t => t.ToAddress);
                entity.HasIndex(t => t.FromUserId);
                entity.HasIndex(t => t.ToUserId);
                entity.HasIndex(t => t.TransferredAt);

                entity.HasOne(t => t.NftItem)
                    .WithMany()
                    .HasForeignKey(t => t.NftItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.FromUser)
                    .WithMany()
                    .HasForeignKey(t => t.FromUserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.ToUser)
                    .WithMany()
                    .HasForeignKey(t => t.ToUserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
