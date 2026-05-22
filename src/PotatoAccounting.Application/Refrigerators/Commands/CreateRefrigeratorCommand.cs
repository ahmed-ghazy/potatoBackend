using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record CreateRefrigeratorCommand(string Name, string? Notes) : IRequest<RefrigeratorDto>;

public class CreateRefrigeratorCommandValidator : AbstractValidator<CreateRefrigeratorCommand>
{
    public CreateRefrigeratorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreateRefrigeratorCommandHandler : IRequestHandler<CreateRefrigeratorCommand, RefrigeratorDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateRefrigeratorCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefrigeratorDto> Handle(CreateRefrigeratorCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var name = request.Name.Trim();
        var exists = await _db.Refrigerators.AnyAsync(r => r.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم التلاجة مستخدم بالفعل") });

        var r = Refrigerator.Create(name, request.Notes, _currentUser.UserId, tenantId);
        _db.Refrigerators.Add(r);
        await _db.SaveChangesAsync(cancellationToken);

        return RefrigeratorDto.From(r);
    }
}
