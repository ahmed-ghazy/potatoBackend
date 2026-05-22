using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Users.Commands;
using PotatoAccounting.Application.Users.Dtos;
using PotatoAccounting.Application.Users.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll()
        => Ok(await _sender.Send(new GetAllUsersQuery()));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserCommand command)
        => Ok(await _sender.Send(command));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _sender.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}
