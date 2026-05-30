using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.PersonalAccounts.Commands;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Application.PersonalAccounts.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonalAccountsController : ControllerBase
{
    private readonly ISender _sender;

    public PersonalAccountsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonalAccountDto>>> GetAll([FromQuery] string? search = null)
        => Ok(await _sender.Send(new GetAllPersonalAccountsQuery(search)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonalAccountDto>> Get(int id)
        => Ok(await _sender.Send(new GetPersonalAccountByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<PersonalAccountDto>> Create([FromBody] CreatePersonalAccountCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PersonalAccountDto>> Update(int id, [FromBody] UpdatePersonalAccountCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeletePersonalAccountCommand(id));
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PersonalAccountsSummaryDto>> Summary()
        => Ok(await _sender.Send(new GetPersonalAccountsSummaryQuery()));

    [HttpGet("{id:int}/entries")]
    public async Task<ActionResult<IReadOnlyList<PersonalAccountEntryDto>>> GetEntries(int id)
        => Ok(await _sender.Send(new GetPersonalAccountEntriesQuery(id)));

    [HttpPost("{id:int}/entries")]
    public async Task<ActionResult<PersonalAccountEntryDto>> AddEntry(int id, [FromBody] CreatePersonalAccountEntryCommand command)
    {
        if (id != command.PersonalAccountId) return BadRequest(new { message = "Account id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpPut("{id:int}/entries/{entryId:int}")]
    public async Task<ActionResult<PersonalAccountEntryDto>> UpdateEntry(int id, int entryId, [FromBody] UpdatePersonalAccountEntryCommand command)
    {
        if (id != command.PersonalAccountId) return BadRequest(new { message = "Account id mismatch" });
        if (entryId != command.Id) return BadRequest(new { message = "Entry id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}/entries/{entryId:int}")]
    public async Task<IActionResult> DeleteEntry(int id, int entryId)
    {
        await _sender.Send(new DeletePersonalAccountEntryCommand(id, entryId));
        return NoContent();
    }
}
