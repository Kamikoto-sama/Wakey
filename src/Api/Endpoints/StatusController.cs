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
    
    [HttpPost("proxy/reboot")]
    public IActionResult RebootProxy()
    {
        statusManager.RebootProxy();
        return Ok();
    }

    [HttpPost("rdp-forwarding/enable")]
    public IActionResult EnableRdpForwarding()
    {
        statusManager.EnableRdpForwarding();
        return Ok();
    }

    [HttpPost("rdp-forwarding/disable")]
    public IActionResult DisableRdpForwarding()
    {
        statusManager.DisableRdpForwarding();
        return Ok();
    }
}