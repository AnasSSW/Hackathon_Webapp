using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hackathon.Models;

namespace Hackathon.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Hackathon.Models.Post> Posts { get; set; }
        public DbSet<Hackathon.Models.PostParticipant> PostParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite primary key for PostParticipant
            builder.Entity<PostParticipant>()
                .HasKey(pp => new { pp.PostId, pp.UserId });

            // Configure the many-to-many relationship
            builder.Entity<PostParticipant>()
                .HasOne(pp => pp.Post)
                .WithMany(p => p.Participants)
                .HasForeignKey(pp => pp.PostId);

            builder.Entity<PostParticipant>()
                .HasOne(pp => pp.User)
                .WithMany()
                .HasForeignKey(pp => pp.UserId);

            // Configure the relationship between Post and ApplicationUser, preventing cascade delete
            builder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
