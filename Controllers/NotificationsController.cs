using FcgNotificationsApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FcgNotificationsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationsDbContext _context;

    public NotificationsController(NotificationsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetAllNotifications()
    {
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.SentAt)
            .Take(100)
            .ToListAsync();

        return Ok(notifications);
    }
}
