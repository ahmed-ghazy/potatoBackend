using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Tenants.Commands;
using PotatoAccounting.Application.Tenants.Dtos;
using PotatoAccounting.Application.Tenants.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private readonly ISender _sender;

    public TenantsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> GetAll()
        => Ok(await _sender.Send(new GetAllTenantsQuery()));

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantCommand command)
        => Ok(await _sender.Send(command));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActivePayload body)
    {
        await _sender.Send(new SetTenantActiveCommand(id, body.IsActive));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteTenantCommand(id));
        return NoContent();
    }

    public record SetActivePayload(bool IsActive);
}
