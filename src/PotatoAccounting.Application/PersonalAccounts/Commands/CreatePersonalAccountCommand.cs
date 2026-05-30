using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record CreatePersonalAccountCommand(string Name, string? PhoneNumber, string? Notes) : IRequest<PersonalAccountDto>;

public class CreatePersonalAccountCommandValidator : AbstractValidator<CreatePersonalAccountCommand>
{
    public CreatePersonalAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreatePersonalAccountCommandHandler : IRequestHandler<CreatePersonalAccountCommand, PersonalAccountDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreatePersonalAccountCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PersonalAccountDto> Handle(CreatePersonalAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var name = request.Name.Trim();
        var exists = await _db.PersonalAccounts.AnyAsync(a => a.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "هذا الاسم مستخدم بالفعل") });

        var entity = PersonalAccount.Create(name, request.PhoneNumber, request.Notes, _currentUser.UserId, tenantId);
        _db.PersonalAccounts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new PersonalAccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            TotalGiven = 0,
            TotalReceived = 0,
            EntriesCount = 0,
        };
    }
}
