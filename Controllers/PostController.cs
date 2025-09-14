using Hackathon.Data;
using Hackathon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hackathon.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts.Include(p => p.Author).OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Participants)
                    .ThenInclude(pp => pp.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageUrl,RequiredExpertise,ExpirationDate,MaxParticipants")] Post post)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                post.AuthorId = userId;
                post.CreatedAt = DateTime.UtcNow;
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(post);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (post.AuthorId != currentUserId)
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageUrl,RequiredExpertise,ExpirationDate,MaxParticipants")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            var postToUpdate = await _context.Posts.FindAsync(id);
            if (postToUpdate == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (postToUpdate.AuthorId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    postToUpdate.Title = post.Title;
                    postToUpdate.Content = post.Content;
                    postToUpdate.ImageUrl = post.ImageUrl;
                    postToUpdate.RequiredExpertise = post.RequiredExpertise;
                    postToUpdate.ExpirationDate = post.ExpirationDate;
                    postToUpdate.MaxParticipants = post.MaxParticipants;

                    _context.Update(postToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = postToUpdate.Id });
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);
            var isAlreadyApplied = await _context.PostParticipants
                .AnyAsync(pp => pp.PostId == id && pp.UserId == userId);

            var post = await _context.Posts.FindAsync(id);

            if (isAlreadyApplied || post == null || post.Participants.Count >= post.MaxParticipants)
            {
                return RedirectToAction("Details", new { id });
            }

            var postParticipant = new PostParticipant
            {
                PostId = id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.PostParticipants.Add(postParticipant);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveParticipant(int id)
        {
            var postParticipant = await _context.PostParticipants
                .Include(pp => pp.Post)
                .FirstOrDefaultAsync(pp => pp.Id == id);

            if (postParticipant == null || postParticipant.Post.AuthorId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            postParticipant.IsApproved = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = postParticipant.PostId });
        }
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
