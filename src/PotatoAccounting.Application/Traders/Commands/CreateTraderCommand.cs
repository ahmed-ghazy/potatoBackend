using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Traders.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Traders.Commands;

public record CreateTraderCommand(string Name, string? PhoneNumber) : IRequest<TraderDto>;

public class CreateTraderCommandValidator : AbstractValidator<CreateTraderCommand>
{
    public CreateTraderCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
    }
}

public class CreateTraderCommandHandler : IRequestHandler<CreateTraderCommand, TraderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateTraderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TraderDto> Handle(CreateTraderCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var name = request.Name.Trim();
        var exists = await _db.Traders.AnyAsync(t => t.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم التاجر مستخدم بالفعل") });

        var trader = Trader.Create(name, request.PhoneNumber, _currentUser.UserId, tenantId);
        _db.Traders.Add(trader);
        await _db.SaveChangesAsync(cancellationToken);

        return TraderDto.From(trader);
    }
}
