using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Seasons.Commands;
using PotatoAccounting.Application.Seasons.Dtos;
using PotatoAccounting.Application.Seasons.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeasonsController : ControllerBase
{
    private readonly ISender _sender;

    public SeasonsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SeasonDto>>> GetAll()
        => Ok(await _sender.Send(new GetAllSeasonsQuery()));

    [HttpPost]
    public async Task<ActionResult<SeasonDto>> Create([FromBody] CreateSeasonCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SeasonDto>> Update(int id, [FromBody] UpdateSeasonCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteSeasonCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/set-default")]
    public async Task<ActionResult<SeasonDto>> SetDefault(int id)
        => Ok(await _sender.Send(new SetDefaultSeasonCommand(id)));
}
