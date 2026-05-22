using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Traders.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Traders.Commands;

public record UpdateTraderCommand(int Id, string Name, string? PhoneNumber) : IRequest<TraderDto>;

public class UpdateTraderCommandValidator : AbstractValidator<UpdateTraderCommand>
{
    public UpdateTraderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
    }
}

public class UpdateTraderCommandHandler : IRequestHandler<UpdateTraderCommand, TraderDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateTraderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<TraderDto> Handle(UpdateTraderCommand request, CancellationToken cancellationToken)
    {
        var trader = await _db.Traders.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Trader), request.Id);

        var newName = request.Name.Trim();
        if (!string.Equals(trader.Name, newName, StringComparison.Ordinal))
        {
            var dup = await _db.Traders.AnyAsync(t => t.Id != request.Id && t.Name == newName, cancellationToken);
            if (dup)
                throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم التاجر مستخدم بالفعل") });
        }

        trader.Update(newName, request.PhoneNumber);
        await _db.SaveChangesAsync(cancellationToken);

        return TraderDto.From(trader);
    }
}
