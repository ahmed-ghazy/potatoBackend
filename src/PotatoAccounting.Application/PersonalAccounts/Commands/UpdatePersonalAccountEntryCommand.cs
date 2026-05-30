using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record UpdatePersonalAccountEntryCommand(
    int PersonalAccountId,
    int Id,
    DateTime Date,
    PersonalAccountEntryDirection Direction,
    decimal Amount,
    string? Description,
    PersonalAccountPaymentMethod? PaymentMethod) : IRequest<PersonalAccountEntryDto>;

public class UpdatePersonalAccountEntryCommandValidator : AbstractValidator<UpdatePersonalAccountEntryCommand>
{
    public UpdatePersonalAccountEntryCommandValidator()
    {
        RuleFor(x => x.PersonalAccountId).GreaterThan(0);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.PaymentMethod)
            .NotNull().IsInEnum()
            .When(x => x.Direction == PersonalAccountEntryDirection.Received, ApplyConditionTo.AllValidators)
            .WithMessage("نوع الاستلام مطلوب");
    }
}

public class UpdatePersonalAccountEntryCommandHandler : IRequestHandler<UpdatePersonalAccountEntryCommand, PersonalAccountEntryDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePersonalAccountEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PersonalAccountEntryDto> Handle(UpdatePersonalAccountEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _db.PersonalAccountEntries
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.PersonalAccountId == request.PersonalAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccountEntry), request.Id);

        entry.Update(request.Date, request.Direction, request.Amount, request.Description, request.PaymentMethod);
        await _db.SaveChangesAsync(cancellationToken);

        return PersonalAccountEntryDto.From(entry);
    }
}
