using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Traders.Commands;
using PotatoAccounting.Application.Traders.Dtos;
using PotatoAccounting.Application.Traders.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TradersController : ControllerBase
{
    private readonly ISender _sender;

    public TradersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TraderDto>>> GetAll([FromQuery] string? search = null)
        => Ok(await _sender.Send(new GetAllTradersQuery(search)));

    [HttpPost]
    public async Task<ActionResult<TraderDto>> Create([FromBody] CreateTraderCommand command)
        => Ok(await _sender.Send(command));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TraderDto>> Update(int id, [FromBody] UpdateTraderCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteTraderCommand(id));
        return NoContent();
    }
}
