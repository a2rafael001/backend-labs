using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() =>
        Ok(new { status = "ok", time = DateTime.UtcNow });
}
