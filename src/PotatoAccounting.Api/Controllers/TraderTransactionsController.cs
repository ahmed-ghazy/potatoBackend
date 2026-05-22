using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.TraderTransactions.Commands;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Application.TraderTransactions.Queries;

namespace PotatoAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TraderTransactionsController : ControllerBase
{
    private readonly ISender _sender;

    public TraderTransactionsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<ActionResult<PagedResult<TraderTransactionDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
        => Ok(await _sender.Send(new GetAllTraderTransactionsQuery(search, from, to, page, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TraderTransactionDto>> Get(int id)
        => Ok(await _sender.Send(new GetTraderTransactionByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<TraderTransactionDto>> Create([FromBody] CreateTraderTransactionCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TraderTransactionDto>> Update(int id, [FromBody] UpdateTraderTransactionCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id mismatch" });
        return Ok(await _sender.Send(command));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sender.Send(new DeleteTraderTransactionCommand(id));
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TraderTransactionsSummaryDto>> Summary()
        => Ok(await _sender.Send(new GetTraderTransactionsSummaryQuery()));

    [HttpPost("{id:int}/close")]
    public async Task<ActionResult<TraderTransactionDto>> Close(int id)
        => Ok(await _sender.Send(new CloseTraderTransactionCommand(id)));

    [HttpPost("{id:int}/reopen")]
    public async Task<ActionResult<TraderTransactionDto>> Reopen(int id)
        => Ok(await _sender.Send(new ReopenTraderTransactionCommand(id)));

    [HttpPost("{id:int}/items/{itemId:int}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<TraderTransactionItemDto>> UploadItemImage(int id, int itemId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "لم يتم اختيار ملف" });

        await using var stream = file.OpenReadStream();
        var dto = await _sender.Send(new UploadTraderTransactionItemImageCommand(
            id, itemId, file.FileName, stream, file.ContentType ?? "", file.Length));
        return Ok(dto);
    }

    [HttpDelete("{id:int}/items/{itemId:int}/image")]
    public async Task<IActionResult> DeleteItemImage(int id, int itemId)
    {
        await _sender.Send(new DeleteTraderTransactionItemImageCommand(id, itemId));
        return NoContent();
    }

    [HttpPost("{id:int}/payments")]
    public async Task<ActionResult<TraderPaymentDto>> AddPayment(int id, [FromBody] AddTraderPaymentPayload body)
    {
        var dto = await _sender.Send(new AddTraderPaymentCommand(id, body.Date, body.Amount, body.Notes));
        return Ok(dto);
    }

    [HttpDelete("{id:int}/payments/{paymentId:int}")]
    public async Task<IActionResult> DeletePayment(int id, int paymentId)
    {
        await _sender.Send(new DeleteTraderPaymentCommand(id, paymentId));
        return NoContent();
    }

    public record AddTraderPaymentPayload(DateTime Date, decimal Amount, string? Notes);
}
