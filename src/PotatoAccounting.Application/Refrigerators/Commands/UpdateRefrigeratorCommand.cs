using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record UpdateRefrigeratorCommand(int Id, string Name, string? Notes) : IRequest<RefrigeratorDto>;

public class UpdateRefrigeratorCommandValidator : AbstractValidator<UpdateRefrigeratorCommand>
{
    public UpdateRefrigeratorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class UpdateRefrigeratorCommandHandler : IRequestHandler<UpdateRefrigeratorCommand, RefrigeratorDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateRefrigeratorCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorDto> Handle(UpdateRefrigeratorCommand request, CancellationToken cancellationToken)
    {
        var r = await _db.Refrigerators.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.Id);

        var newName = request.Name.Trim();
        if (!string.Equals(r.Name, newName, StringComparison.Ordinal))
        {
            var dup = await _db.Refrigerators.AnyAsync(x => x.Id != request.Id && x.Name == newName, cancellationToken);
            if (dup)
                throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم التلاجة مستخدم بالفعل") });
        }

        r.Update(newName, request.Notes);
        await _db.SaveChangesAsync(cancellationToken);

        var items = await _db.RefrigeratorItems.AsNoTracking().Where(i => i.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        var expenses = await _db.RefrigeratorExpenses.AsNoTracking().Where(e => e.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        return RefrigeratorDto.From(r, items, expenses);
    }
}
