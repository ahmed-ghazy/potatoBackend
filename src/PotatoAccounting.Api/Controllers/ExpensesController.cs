using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.Expenses.Commands;
using PotatoAccounting.Application.Expenses.Dtos;
using PotatoAccounting.Application.Expenses.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly ISender _sender;

    public ExpensesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
        => Ok(await _sender.Send(new GetAllExpensesQuery(search, from, to, page, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Get(int id)
        => Ok(await _sender.Send(new GetExpenseByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Update(int id, [FromBody] UpdateExpenseCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteExpenseCommand(id));
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ExpensesSummaryDto>> Summary()
        => Ok(await _sender.Send(new GetExpensesSummaryQuery()));
}
