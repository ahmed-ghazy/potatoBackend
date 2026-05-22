using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Profile.Commands;
using PotatoAccounting.Application.Profile.Dtos;
using PotatoAccounting.Application.Profile.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ISender _sender;

    public ProfileController(ISender sender) => _sender = sender;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMyProfile()
        => Ok(await _sender.Send(new GetMyProfileQuery()));

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
        => Ok(await _sender.Send(command));

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeMyPasswordCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }
}
