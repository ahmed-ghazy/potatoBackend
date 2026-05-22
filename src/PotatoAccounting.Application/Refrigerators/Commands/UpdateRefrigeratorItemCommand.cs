using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record UpdateRefrigeratorItemCommand(
    int RefrigeratorId,
    int ItemId,
    DateTime Date,
    int SackCount,
    decimal Weight,
    string? Description) : IRequest<RefrigeratorItemDto>;

public class UpdateRefrigeratorItemCommandValidator : AbstractValidator<UpdateRefrigeratorItemCommand>
{
    public UpdateRefrigeratorItemCommandValidator()
    {
        RuleFor(x => x.RefrigeratorId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.SackCount).GreaterThan(0);
        RuleFor(x => x.Weight).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}

public class UpdateRefrigeratorItemCommandHandler : IRequestHandler<UpdateRefrigeratorItemCommand, RefrigeratorItemDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateRefrigeratorItemCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorItemDto> Handle(UpdateRefrigeratorItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.RefrigeratorItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorItem), request.ItemId);

        item.Update(request.Date, request.SackCount, request.Weight, request.Description);
        await _db.SaveChangesAsync(cancellationToken);

        return RefrigeratorItemDto.From(item);
    }
}
