using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hackathon.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Existing properties
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        [StringLength(100)]
        public string? Address { get; set; }
        [StringLength(100)]
        public string? GitHubUsername { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        [StringLength(200)]
        public string? Languages { get; set; }
        [StringLength(200)]
        public string? Education { get; set; }
        [StringLength(200)]
        public string? Experience { get; set; }

        // --- UPDATED SKILLS STORAGE ---
        // We will now store skills as a single JSON string
        // to accommodate the skill name and its description.
        public string? SkillsJson { get; set; }

        // Navigation properties for relationships
        // Collection of posts the user has created
        public ICollection<Post>? CreatedPosts { get; set; }

        // Collection of posts the user has joined
        public ICollection<PostParticipant>? JoinedPosts { get; set; }
    }
}
