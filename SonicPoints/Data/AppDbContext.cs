using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SonicPoints.Models;

namespace SonicPoints.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string> // Inherit from IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for each model
        public DbSet<IdentityUserClaim<string>> UserClaims { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        public DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        public DbSet<IdentityRoleClaim<string>> RoleClaims { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<RedeemableItem> RedeemableItems { get; set; }
        public DbSet<RedeemHistory> RedeemHistories { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite keys if needed
            modelBuilder.Entity<RedeemHistory>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RedeemHistory>()
                .HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RedeemHistory>()
                .HasOne(r => r.RedeemableItem)
                .WithMany()
                .HasForeignKey(r => r.RedeemableItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TaskItem relationships
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId);

            // Configure Project relationships
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Admin)
                .WithMany()
                .HasForeignKey(p => p.AdminId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Admin

            // Configure Leaderboard relationships
            modelBuilder.Entity<Leaderboard>()
                .HasKey(tc => tc.Id);  // Primary key for TaskCompletion
            modelBuilder.Entity<Leaderboard>()
                .HasOne(tc => tc.User)
                .WithMany(u => u.TaskCompletions)
                .HasForeignKey(tc => tc.UserId);
            modelBuilder.Entity<Leaderboard>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Leaderboards)
                .HasForeignKey(tc => tc.TaskId);

            base.OnModelCreating(modelBuilder); // Ensure to call base method
        }
    }
}
