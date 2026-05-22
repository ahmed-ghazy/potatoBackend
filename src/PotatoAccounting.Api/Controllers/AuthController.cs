using MediatR;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Auth.Commands;
using PotatoAccounting.Application.Auth.Dtos;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
        => Ok(await _sender.Send(command));
}
