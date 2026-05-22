using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Seasons.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Seasons.Commands;

public record CreateSeasonCommand(string Name, string? Notes) : IRequest<SeasonDto>;

public class CreateSeasonCommandValidator : AbstractValidator<CreateSeasonCommand>
{
    public CreateSeasonCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreateSeasonCommandHandler : IRequestHandler<CreateSeasonCommand, SeasonDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateSeasonCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SeasonDto> Handle(CreateSeasonCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var name = request.Name.Trim();
        var exists = await _db.Seasons.AnyAsync(s => s.Name == name, cancellationToken);
        if (exists)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم الموسم مستخدم بالفعل") });

        var season = Season.Create(name, request.Notes, _currentUser.UserId, tenantId);
        _db.Seasons.Add(season);
        await _db.SaveChangesAsync(cancellationToken);

        return SeasonDto.From(season);
    }
}
