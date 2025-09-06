using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

[Route("alice")]
public class AliceController(AliceService aliceService) : Controller
{
    [HttpPost]
    public IActionResult HandleCommand([FromBody] AliceCommandDto commandDto)
    {
        var response = aliceService.HandleCommand(commandDto);
        return Ok(response);
    }
}