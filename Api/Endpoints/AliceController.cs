using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Api.Endpoints;

[Route("alice")]
public class AliceController(AliceService aliceService) : Controller
{
    [HttpPost]
    public IActionResult HandleCommand(AliceCommandDto commandDto)
    {
        var response = aliceService.HandleCommand(commandDto);
        return Ok(response);
    }
}