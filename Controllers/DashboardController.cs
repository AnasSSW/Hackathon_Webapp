using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Hackathon.Models;
using Hackathon.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hackathon.Controllers
{
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // If the user is not authenticated, redirect to the login page
                return RedirectToPage("/Account/Login");
            }

            // Fetch posts created by the current user
            var myPosts = await _context.Posts
                                        .Where(p => p.AuthorId == user.Id)
                                        .Include(p => p.Author)
                                        .ToListAsync();

            // Fetch posts the user has joined using a many-to-many relationship.
            // This assumes a 'PostParticipant' join table exists in your database context.
            var joinedPosts = await _context.PostParticipants
                                            .Where(pp => pp.UserId == user.Id)
                                            .Select(pp => pp.Post)
                                            .Include(p => p.Author)
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
