using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Tenants.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Tenants.Commands;

public record CreateTenantCommand(
    string Name,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName) : IRequest<TenantDto>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.AdminFullName).NotEmpty().MaximumLength(200);
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public CreateTenantCommandHandler(IApplicationDbContext db, IIdentityService identity, ICurrentUserService currentUser)
    {
        _db = db;
        _identity = identity;
        _currentUser = currentUser;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _db.Tenants.AnyAsync(t => t.Name == request.Name.Trim(), cancellationToken);
        if (nameExists)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("name", "اسم الشركة مستخدم بالفعل") });

        var tenant = Tenant.Create(request.Name, _currentUser.UserId);
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(cancellationToken);

        await _identity.CreateUserAsync(request.AdminEmail, request.AdminPassword, request.AdminFullName, "Admin", tenant.Id);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UsersCount = 1,
        };
    }
}
