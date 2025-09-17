using Hackathon.Data;
using Hackathon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hackathon.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["DisableMainCss"] = true;
            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Handle case where user is not found
                return NotFound();
            }

            // Correctly load MyPosts with related data.
            // Use Include() and ThenInclude() before calling ToListAsync().
            var myPosts = await _context.Posts
                                       .Include(p => p.Author)
                                       .Include(p => p.Participants)
                                            .ThenInclude(pp => pp.User) // To load participant user details
                                       .Where(p => p.AuthorId == user.Id)
                                       .ToListAsync();

            // Correctly load JoinedPosts with related data.
            // The query starts from PostParticipants and includes Post and Author.
            var joinedPosts = await _context.PostParticipants
                                          .Include(pp => pp.Post)
                                            .ThenInclude(p => p.Author) // To load the author of the joined post
                                          .Include(pp => pp.Post.Participants) // To load participants of the joined post
                                          .Where(pp => pp.UserId == user.Id && pp.IsApproved)
                                          .Select(pp => pp.Post) // Select the actual Post object
                                          .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                MyPosts = myPosts,
                JoinedPosts = joinedPosts
            };

            return View(viewModel);
        }
    }
}
