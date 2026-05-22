using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record AddRefrigeratorItemCommand(
    int RefrigeratorId,
    DateTime Date,
    int SackCount,
    decimal Weight,
    string? Description) : IRequest<RefrigeratorItemDto>;

public class AddRefrigeratorItemCommandValidator : AbstractValidator<AddRefrigeratorItemCommand>
{
    public AddRefrigeratorItemCommandValidator()
    {
        RuleFor(x => x.RefrigeratorId).GreaterThan(0);
        RuleFor(x => x.SackCount).GreaterThan(0).WithMessage("عدد الشكاير يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Weight).GreaterThan(0).WithMessage("الوزن يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Description).MaximumLength(200);
    }
}

public class AddRefrigeratorItemCommandHandler : IRequestHandler<AddRefrigeratorItemCommand, RefrigeratorItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddRefrigeratorItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefrigeratorItemDto> Handle(AddRefrigeratorItemCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var r = await _db.Refrigerators.FirstOrDefaultAsync(x => x.Id == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.RefrigeratorId);

        var item = RefrigeratorItem.Create(r.Id, request.Date, request.SackCount, request.Weight, request.Description, _currentUser.UserId, tenantId);
        _db.RefrigeratorItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);

        return RefrigeratorItemDto.From(item);
    }
}
