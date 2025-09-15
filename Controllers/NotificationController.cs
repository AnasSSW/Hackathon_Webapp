using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hackathon.Data;
using Hackathon.Models;
using System.Security.Claims;

namespace Hackathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("list")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> List()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new object[0]);
            }

            var notifications = await _context.Notifications
                                            .Where(n => n.UserId == userId)
                                            .OrderByDescending(n => n.CreatedAt)
                                            .Select(n => new
                                            {
                                                n.Id,
                                                n.Message,
                                                n.CreatedAt,
                                                n.IsRead
                                            })
                                            .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("unread")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { count = 0 });
            }

            var unreadCount = await _context.Notifications
                                            .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new { count = unreadCount });
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notifications = await _context.Notifications
                                            .Where(n => n.UserId == userId && !n.IsRead)
                                            .ToListAsync();

            if (notifications.Any())
            {
                foreach (var notif in notifications)
                {
                    notif.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
