using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.FarmerTransactions.Commands;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Application.FarmerTransactions.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FarmerTransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public FarmerTransactionsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<PagedResult<FarmerTransactionDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
        => Ok(await _sender.Send(new GetAllFarmerTransactionsQuery(search, from, to, page, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FarmerTransactionDto>> Get(int id)
        => Ok(await _sender.Send(new GetFarmerTransactionByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<FarmerTransactionDto>> Create([FromBody] CreateFarmerTransactionCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FarmerTransactionDto>> Update(int id, [FromBody] UpdateFarmerTransactionCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteFarmerTransactionCommand(id));
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FarmerTransactionsSummaryDto>> Summary()
        => Ok(await _sender.Send(new GetFarmerTransactionsSummaryQuery()));

    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<FarmerTransactionDto>> Close(int id)
        => Ok(await _sender.Send(new CloseFarmerTransactionCommand(id)));

    [HttpPost("{id:int}/reopen")]
    public async Task<ActionResult<FarmerTransactionDto>> Reopen(int id)
        => Ok(await _sender.Send(new ReopenFarmerTransactionCommand(id)));

    // ===== Item images =====

    [HttpPost("{id:int}/items/{itemId:int}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<FarmerTransactionItemDto>> UploadItemImage(int id, int itemId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "لم يتم اختيار ملف" });

        await using var stream = file.OpenReadStream();
        var dto = await _sender.Send(new UploadFarmerTransactionItemImageCommand(
            id, itemId, file.FileName, stream, file.ContentType ?? "", file.Length));
        return Ok(dto);
    }

    [HttpDelete("{id:int}/items/{itemId:int}/image")]
    public async Task<IActionResult> DeleteItemImage(int id, int itemId)
    {
        await _sender.Send(new DeleteFarmerTransactionItemImageCommand(id, itemId));
        return NoContent();
    }

    // ===== Payments =====

    [HttpPost("{id:int}/payments")]
    public async Task<ActionResult<FarmerPaymentDto>> AddPayment(int id, [FromBody] AddPaymentPayload body)
    {
        var dto = await _sender.Send(new AddFarmerPaymentCommand(id, body.Date, body.Amount, body.Notes));
        return Ok(dto);
    }

    [HttpDelete("{id:int}/payments/{paymentId:int}")]
    public async Task<IActionResult> DeletePayment(int id, int paymentId)
    {
        await _sender.Send(new DeleteFarmerPaymentCommand(id, paymentId));
        return NoContent();
    }

    public record AddPaymentPayload(DateTime Date, decimal Amount, string? Notes);
}
