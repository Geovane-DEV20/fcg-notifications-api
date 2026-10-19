using Microsoft.AspNetCore.Mvc;

namespace FcgNotificationsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Notifications API is running" });
    }
}
