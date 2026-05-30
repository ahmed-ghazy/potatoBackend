using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Refrigerators.Commands;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Application.Refrigerators.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RefrigeratorsController : ControllerBase
{
    private readonly ISender _sender;

    public RefrigeratorsController(ISender sender) => _sender = sender;

    // ===== Refrigerator CRUD =====

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RefrigeratorDto>>> GetAll([FromQuery] string? search = null)
        => Ok(await _sender.Send(new GetAllRefrigeratorsQuery(search)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RefrigeratorDto>> Get(int id)
        => Ok(await _sender.Send(new GetRefrigeratorByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<RefrigeratorDto>> Create([FromBody] CreateRefrigeratorCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RefrigeratorDto>> Update(int id, [FromBody] UpdateRefrigeratorCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteRefrigeratorCommand(id));
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<RefrigeratorsSummaryDto>> Summary()
        => Ok(await _sender.Send(new GetRefrigeratorsSummaryQuery()));

    // ===== Items =====

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<RefrigeratorItemDto>> AddItem(int id, [FromBody] AddItemPayload body)
        => Ok(await _sender.Send(new AddRefrigeratorItemCommand(id, body.Date, body.SackCount, body.Weight, body.Description)));

    [HttpPut("{id:int}/items/{itemId:int}")]
    public async Task<ActionResult<RefrigeratorItemDto>> UpdateItem(int id, int itemId, [FromBody] AddItemPayload body)
        => Ok(await _sender.Send(new UpdateRefrigeratorItemCommand(id, itemId, body.Date, body.SackCount, body.Weight, body.Description)));

    [HttpDelete("{id:int}/items/{itemId:int}")]
    public async Task<IActionResult> DeleteItem(int id, int itemId)
    {
        await _sender.Send(new DeleteRefrigeratorItemCommand(id, itemId));
        return NoContent();
    }

    // ===== Item images =====

    [HttpPost("{id:int}/items/{itemId:int}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<RefrigeratorItemDto>> UploadItemImage(int id, int itemId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "لم يتم اختيار ملف" });

        await using var stream = file.OpenReadStream();
        var dto = await _sender.Send(new UploadRefrigeratorItemImageCommand(
            id, itemId, file.FileName, stream, file.ContentType ?? "", file.Length));
        return Ok(dto);
    }

    [HttpDelete("{id:int}/items/{itemId:int}/image")]
    public async Task<IActionResult> DeleteItemImage(int id, int itemId)
    {
        await _sender.Send(new DeleteRefrigeratorItemImageCommand(id, itemId));
        return NoContent();
    }

    // ===== Expenses =====

    [HttpPost("{id:int}/expenses")]
    public async Task<ActionResult<RefrigeratorExpenseDto>> AddExpense(int id, [FromBody] AddExpensePayload body)
        => Ok(await _sender.Send(new AddRefrigeratorExpenseCommand(id, body.Date, body.Amount, body.Description, body.Notes)));

    [HttpPut("{id:int}/expenses/{expenseId:int}")]
    public async Task<ActionResult<RefrigeratorExpenseDto>> UpdateExpense(int id, int expenseId, [FromBody] AddExpensePayload body)
        => Ok(await _sender.Send(new UpdateRefrigeratorExpenseCommand(id, expenseId, body.Date, body.Amount, body.Description, body.Notes)));

    [HttpDelete("{id:int}/expenses/{expenseId:int}")]
    public async Task<IActionResult> DeleteExpense(int id, int expenseId)
    {
        await _sender.Send(new DeleteRefrigeratorExpenseCommand(id, expenseId));
        return NoContent();
    }

    public record AddItemPayload(DateTime Date, int SackCount, decimal Weight, string? Description);
    public record AddExpensePayload(DateTime Date, decimal Amount, string Description, string? Notes);
}
