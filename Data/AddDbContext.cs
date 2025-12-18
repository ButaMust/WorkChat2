using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Models;

namespace WorkChat2.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ChatRoom> ChatRooms { get; set; } = null!;
        public DbSet<ChatRoomParticipant> ChatRoomParticipants { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==========================
            // CHATROOM
            // ==========================
            builder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(cr => cr.Id);

                entity.Property(cr => cr.Name)
                      .HasMaxLength(100);

                entity.HasOne(cr => cr.CreatedByUser)
                      .WithMany() // later you can add ICollection<ChatRoom> CreatedChatRooms to AppUser
                      .HasForeignKey(cr => cr.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // CHATROOM PARTICIPANT (JOIN TABLE)
            // Composite key: ChatRoomId + UserId
            // ==========================
            builder.Entity<ChatRoomParticipant>(entity =>
            {
                entity.HasKey(cp => new { cp.ChatRoomId, cp.UserId });

                entity.HasOne(cp => cp.ChatRoom)
                      .WithMany(cr => cr.Participants)
                      .HasForeignKey(cp => cp.ChatRoomId);

                entity.HasOne(cp => cp.User)
                      .WithMany()
                      .HasForeignKey(cp => cp.UserId);
            });

            // ==========================
            // MESSAGE
            // ==========================
            builder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Text)
                      .IsRequired();

                entity.HasOne(m => m.ChatRoom)
                      .WithMany(cr => cr.Messages)
                      .HasForeignKey(m => m.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // ANNOUNCMENT
            // ==========================

            builder.Entity<Announcement>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                      .HasMaxLength(120);

                entity.Property(a => a.Body)
                      .HasMaxLength(2000);

                entity.Property(a => a.IsPinned)
                      .HasDefaultValue(false);

                entity.Property(a => a.IsPublished)
                      .HasDefaultValue(true);
                
                entity.HasOne(a => a.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ==========================
            // DEFAULT VALUES FOR CreatedAt / UpdatedAt
            // (SQL Server – GETUTCDATE())
            // ==========================
            builder.Entity<ChatRoom>()
                .Property(cr => cr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<ChatRoom>()
                .Property(cr => cr.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ChatRoomParticipant>()
                .Property(cp => cp.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<ChatRoomParticipant>()
                .Property(cp => cp.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Message>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Message>()
                .Property(m => m.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<AppUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<AppUser>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Announcement>()
                   .Property(a => a.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Announcement>()
                   .Property(a => a.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // ==========================
            // SEED ADMIN ROLE + ADMIN USER
            // ==========================
            var adminRoleId = "ADMIN_ROLE_ID";
            var adminUserId = "ADMIN_USER_ID";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            );

            var hasher = new PasswordHasher<AppUser>();

            var adminUser = new AppUser
            {
                Id = adminUserId,
                UserName = "admin@local",
                NormalizedUserName = "ADMIN@LOCAL",
                Email = "admin@local",
                NormalizedEmail = "ADMIN@LOCAL",
                EmailConfirmed = true,
                Name = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

            builder.Entity<AppUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                }
            );
        }

        // ==========================
        // AUTO UPDATE UpdatedAt / CreatedAt
        // ==========================
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var now = DateTime.UtcNow;

            // For entities inheriting BaseEntity
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }

            // For AppUser (does NOT inherit BaseEntity)
            foreach (var entry in ChangeTracker.Entries<AppUser>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
    }
}
