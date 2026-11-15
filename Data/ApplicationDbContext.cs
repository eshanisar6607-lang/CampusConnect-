
using CampusKonnect.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusKonnect.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Activity> Activities { get; set; }

        public DbSet<ActivityParticipant> ActivityParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Activity Creator Relationship
            builder.Entity<Activity>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity and Participants Relationship
            builder.Entity<ActivityParticipant>()
                .HasOne(p => p.Activity)
                .WithMany(a => a.Participants)
                .HasForeignKey(p => p.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student and Participation Relationship
            builder.Entity<ActivityParticipant>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate active participation
            builder.Entity<ActivityParticipant>()
                .HasIndex(p => new { p.ActivityId, p.StudentId })
                .IsUnique()
                .HasFilter("[ParticipationStatus] = 'Active'");
        }
    }
}
