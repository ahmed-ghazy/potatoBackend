using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Farmers.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Farmers.Commands;

public record UpdateFarmerCommand(int Id, string Name, string? PhoneNumber) : IRequest<FarmerDto>;

public class UpdateFarmerCommandValidator : AbstractValidator<UpdateFarmerCommand>
{
    public UpdateFarmerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^01[0125][0-9]{8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
    }
}

public class UpdateFarmerCommandHandler : IRequestHandler<UpdateFarmerCommand, FarmerDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateFarmerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<FarmerDto> Handle(UpdateFarmerCommand request, CancellationToken cancellationToken)
    {
        var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.Id);

        var newName = request.Name.Trim();
        if (!string.Equals(farmer.Name, newName, StringComparison.Ordinal))
        {
            var dup = await _db.Farmers.AnyAsync(f => f.Id != request.Id && f.Name == newName, cancellationToken);
            if (dup)
                throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم الفلاح مستخدم بالفعل") });
        }

        farmer.Update(newName, request.PhoneNumber);
        await _db.SaveChangesAsync(cancellationToken);

        return FarmerDto.From(farmer);
    }
}
