using DteamBackend.Models;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
        public DbSet<UserWishlist> UserWishlists => Set<UserWishlist>();
        public DbSet<UserCartItem> UserCartItems => Set<UserCartItem>();
        public DbSet<Tranxaction> Tranxactions => Set<Tranxaction>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ChatUpload> ChatUploads => Set<ChatUpload>();

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

            modelBuilder.Entity<UserFriend>(entity =>
            {
                entity.HasKey(uf => new { uf.UserId, uf.FriendId });

                entity.HasOne(uf => uf.User)
                    .WithMany(u => u.Friendships)
                    .HasForeignKey(uf => uf.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uf => uf.Friend)
                    .WithMany()
                    .HasForeignKey(uf => uf.FriendId)
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
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => new { r.UserId, r.GameId })
                    .IsUnique();

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Game)
                    .WithMany(g => g.Reviews)
                    .HasForeignKey(r => r.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
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
        }
    }
}
