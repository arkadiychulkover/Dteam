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
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UserGame> UserGames => Set<UserGame>();
        public DbSet<UserFriend> UserFriends => Set<UserFriend>();

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

            modelBuilder.Entity<UserFriend>(entity =>
            {
                entity.HasKey(f => new { f.UserId, f.FriendId });

                entity.HasOne(f => f.User)
                    .WithMany(u => u.Friendships)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Friend)
                    .WithMany()
                    .HasForeignKey(f => f.FriendId)
                    .OnDelete(DeleteBehavior.Restrict);
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
        }
    }
}
