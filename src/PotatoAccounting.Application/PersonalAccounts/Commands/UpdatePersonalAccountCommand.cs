using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record UpdatePersonalAccountCommand(int Id, string Name, string? PhoneNumber, string? Notes) : IRequest<PersonalAccountDto>;

public class UpdatePersonalAccountCommandValidator : AbstractValidator<UpdatePersonalAccountCommand>
{
    public UpdatePersonalAccountCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class UpdatePersonalAccountCommandHandler : IRequestHandler<UpdatePersonalAccountCommand, PersonalAccountDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePersonalAccountCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PersonalAccountDto> Handle(UpdatePersonalAccountCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.PersonalAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccount), request.Id);

        var name = request.Name.Trim();
        var exists = await _db.PersonalAccounts.AnyAsync(a => a.Id != request.Id && a.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "هذا الاسم مستخدم بالفعل") });

        entity.Update(name, request.PhoneNumber, request.Notes);
        await _db.SaveChangesAsync(cancellationToken);

        var totalGiven = await _db.PersonalAccountEntries
            .Where(e => e.PersonalAccountId == entity.Id && e.Direction == PersonalAccountEntryDirection.Given)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
        var totalReceived = await _db.PersonalAccountEntries
            .Where(e => e.PersonalAccountId == entity.Id && e.Direction == PersonalAccountEntryDirection.Received)
            .SumAsync(e => (decimal?)(e.Amount - e.Fee), cancellationToken) ?? 0m;
        var entriesCount = await _db.PersonalAccountEntries
            .CountAsync(e => e.PersonalAccountId == entity.Id, cancellationToken);

        return new PersonalAccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            TotalGiven = totalGiven,
            TotalReceived = totalReceived,
            EntriesCount = entriesCount,
        };
    }
}
