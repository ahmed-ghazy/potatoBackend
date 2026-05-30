using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record CreatePersonalAccountEntryCommand(
    int PersonalAccountId,
    DateTime Date,
    PersonalAccountEntryDirection Direction,
    decimal Amount,
    string? Description,
    PersonalAccountPaymentMethod? PaymentMethod) : IRequest<PersonalAccountEntryDto>;

public class CreatePersonalAccountEntryCommandValidator : AbstractValidator<CreatePersonalAccountEntryCommand>
{
    public CreatePersonalAccountEntryCommandValidator()
    {
        RuleFor(x => x.PersonalAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.PaymentMethod)
            .NotNull().IsInEnum()
            .When(x => x.Direction == PersonalAccountEntryDirection.Received, ApplyConditionTo.AllValidators)
            .WithMessage("نوع الاستلام مطلوب");
    }
}

public class CreatePersonalAccountEntryCommandHandler : IRequestHandler<CreatePersonalAccountEntryCommand, PersonalAccountEntryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreatePersonalAccountEntryCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PersonalAccountEntryDto> Handle(CreatePersonalAccountEntryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var account = await _db.PersonalAccounts
            .FirstOrDefaultAsync(a => a.Id == request.PersonalAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccount), request.PersonalAccountId);

        var entry = PersonalAccountEntry.Create(
            account.Id,
            request.Date,
            request.Direction,
            request.Amount,
            request.Description,
            request.PaymentMethod,
            _currentUser.UserId,
            tenantId);

        _db.PersonalAccountEntries.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);

        return PersonalAccountEntryDto.From(entry);
    }
}
