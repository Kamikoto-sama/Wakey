using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

[Route("/logs")]
public class LogsController(LogManager logManager) : Controller
{
    [HttpGet]
    public IActionResult GetLogs(int count = 100)
    {
        return Ok(logManager.GetLogs(count));
    }
}