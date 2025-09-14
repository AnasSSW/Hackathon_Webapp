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

            // Configure the many-to-many relationship via a join entity (PostParticipant)
            // The Id property in PostParticipant will be the primary key.
            builder.Entity<PostParticipant>()
                .HasOne(pp => pp.Post)
                .WithMany(p => p.Participants)
                .HasForeignKey(pp => pp.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostParticipant>()
                .HasOne(pp => pp.User)
                .WithMany(u => u.JoinedPosts)
                .HasForeignKey(pp => pp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure the relationship between Post and ApplicationUser (Author)
            builder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.CreatedPosts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix for ASP.NET Identity table names to remove "AspNet" prefix
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                var currentTableName = builder.Entity(entity.Name).Metadata.GetDefaultTableName();
                if (currentTableName != null && currentTableName.Contains("AspNet"))
                {
                    builder.Entity(entity.Name).ToTable(currentTableName.Replace("AspNet", ""));
                }
            }
        }
    }
}
