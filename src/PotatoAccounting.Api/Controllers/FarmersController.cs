using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Farmers.Commands;
using PotatoAccounting.Application.Farmers.Dtos;
using PotatoAccounting.Application.Farmers.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FarmersController : ControllerBase
{
    private readonly ISender _sender;

    public FarmersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FarmerDto>>> GetAll([FromQuery] string? search = null)
        => Ok(await _sender.Send(new GetAllFarmersQuery(search)));

    [HttpPost]
    public async Task<ActionResult<FarmerDto>> Create([FromBody] CreateFarmerCommand command)
        => Ok(await _sender.Send(command));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FarmerDto>> Update(int id, [FromBody] UpdateFarmerCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteFarmerCommand(id));
        return NoContent();
    }
}
