using Hackathon.Data;
using Hackathon.Models;
using Hackathon.ViewModels; // <-- Add this
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Hackathon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var allPosts = await _context.Posts
                                         .Include(p => p.Author) // Include author data to get their name
                                         .OrderByDescending(p => p.CreatedAt)
                                         .ToListAsync();

            var matchedPosts = new List<Post>();

            // --- Post Matching Logic ---
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.SkillsJson))
            {
                // Note: This needs the SkillViewModel from the Manage page
                var userSkills = JsonSerializer.Deserialize<List<Areas.Identity.Pages.Account.Manage.IndexModel.SkillViewModel>>(currentUser.SkillsJson);
                var userSkillNames = userSkills.Select(s => s.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var post in allPosts)
                {
                    if (!string.IsNullOrEmpty(post.RequiredExpertise))
                    {
                        var requiredSkills = post.RequiredExpertise.Split(',').Select(s => s.Trim());
                        // If any of the post's required skills is in the user's skill list
                        if (requiredSkills.Any(rs => userSkillNames.Contains(rs)))
                        {
                            matchedPosts.Add(post);
                        }
                    }
                }
            }

            var viewModel = new HomeViewModel
            {
                AllPosts = allPosts,
                MatchedPosts = matchedPosts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

