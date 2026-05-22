using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Farmers.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Farmers.Commands;

public record CreateFarmerCommand(string Name, string? PhoneNumber) : IRequest<FarmerDto>;

public class CreateFarmerCommandValidator : AbstractValidator<CreateFarmerCommand>
{
    public CreateFarmerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
    }
}

public class CreateFarmerCommandHandler : IRequestHandler<CreateFarmerCommand, FarmerDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateFarmerCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FarmerDto> Handle(CreateFarmerCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var name = request.Name.Trim();
        var exists = await _db.Farmers.AnyAsync(f => f.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم الفلاح مستخدم بالفعل") });

        var farmer = Farmer.Create(name, request.PhoneNumber, _currentUser.UserId, tenantId);
        _db.Farmers.Add(farmer);
        await _db.SaveChangesAsync(cancellationToken);

        return FarmerDto.From(farmer);
    }
}
