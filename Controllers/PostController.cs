using Hackathon.Data;
using Hackathon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hackathon.Controllers
{
    // กำหนดให้ทุกเมธอดใน Controller นี้ต้องมีการล็อกอิน ยกเว้นที่ระบุ [AllowAnonymous]
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

        // อนุญาตให้เข้าถึงหน้า Index ได้โดยไม่ต้องล็อกอิน
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts.Include(p => p.Author).OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }

        // อนุญาตให้เข้าถึงหน้ารายละเอียดโพสต์ได้โดยไม่ต้องล็อกอิน
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

            // ตรวจสอบสถานะผู้ใช้เพื่อส่งข้อมูลไปยัง View
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = _userManager.GetUserId(User);
            }
            var isAuthor = post.AuthorId == userId;
            var isParticipant = post.Participants.Any(pp => pp.UserId == userId);

            ViewBag.IsAuthor = isAuthor;
            ViewBag.IsParticipant = isParticipant;

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
            var post = await _context.Posts.FindAsync(id);

            // ตรวจสอบว่าผู้ใช้เข้าร่วมแล้วหรือโพสต์เต็มหรือไม่
            var isAlreadyApplied = await _context.PostParticipants
                .AnyAsync(pp => pp.PostId == id && pp.UserId == userId);

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
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveParticipant(int id)
        {
            var postParticipant = await _context.PostParticipants
                .Include(pp => pp.Post)
                .FirstOrDefaultAsync(pp => pp.Id == id);

            // ตรวจสอบว่าพบผู้เข้าร่วมและผู้ใช้ปัจจุบันคือเจ้าของโพสต์
            if (postParticipant == null || postParticipant.Post.AuthorId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            // ตั้งค่าสถานะเป็นอนุมัติ
            postParticipant.IsApproved = true;
            _context.Update(postParticipant);
            await _context.SaveChangesAsync();

            // สร้าง Notification ใหม่สำหรับผู้ใช้งานที่ได้รับการอนุมัติ
            var notification = new Notification
            {
                UserId = postParticipant.UserId,
                Message = $"ผู้เขียนโพสต์ '{postParticipant.Post.Title}' ได้อนุมัติคุณเข้าร่วมแล้ว!",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // เปลี่ยนเส้นทางกลับไปยังหน้ารายละเอียดของโพสต์
            return RedirectToAction("Details", new { id = postParticipant.PostId });
        }
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
