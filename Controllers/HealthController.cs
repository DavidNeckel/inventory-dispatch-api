using Microsoft.AspNetCore.Mvc;

namespace InventoryDispatchApi.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Get() => Ok(new { status = "ok" });
}
