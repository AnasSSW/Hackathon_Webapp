// /ViewModels/PostDetailsViewModel.cs
namespace Hackathon.ViewModels
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? RequiredExpertise { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsClosed { get; set; }
        public string? AuthorName { get; set; }
        public int MaxParticipants { get; set; }
    }
}
