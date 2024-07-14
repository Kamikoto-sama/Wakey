using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

[Route("/")]
public class StatusController(StatusManager statusManager) : Controller
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(statusManager.GetStatus());
    }

    [HttpPost("awake")]
    public IActionResult Awake()
    {
        statusManager.Awake();
        return Ok();
    }

    [HttpPost("awake/reset")]
    public IActionResult ResetAwake()
    {
        statusManager.ResetAwake();
        return Ok();
    }

    [HttpPost("vpn/enable")]
    public IActionResult EnableVpn()
    {
        statusManager.EnableVpn();
        return Ok();
    }

    [HttpPost("vpn/disable")]
    public IActionResult DisableVpn()
    {
        statusManager.DisableVpn();
        return Ok();
    }
}