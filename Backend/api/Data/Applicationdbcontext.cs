using Microsoft.EntityFrameworkCore;
using InterviewScheduler.API.Models;

namespace InterviewScheduler.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.GoogleId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.GoogleId).HasMaxLength(255);
            });

            // Interview configuration
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobTitle).HasMaxLength(255);
                entity.Property(e => e.CandidateName).HasMaxLength(255);
                entity.Property(e => e.CandidateEmail).HasMaxLength(255);
                entity.Property(e => e.InterviewerName).HasMaxLength(255);
                entity.Property(e => e.InterviewerEmail).HasMaxLength(255);

                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Interviews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

