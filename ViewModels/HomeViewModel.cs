using Hackathon.Models;
using System.Collections.Generic;

namespace Hackathon.ViewModels
{
    public class HomeViewModel
    {
        public List<Post> MatchedPosts { get; set; } = new List<Post>();
        public List<Post> AllPosts { get; set; } = new List<Post>();
    }
}

