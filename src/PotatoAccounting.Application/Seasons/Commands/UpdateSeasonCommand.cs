using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Seasons.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Seasons.Commands;

public record UpdateSeasonCommand(int Id, string Name, string? Notes) : IRequest<SeasonDto>;

public class UpdateSeasonCommandValidator : AbstractValidator<UpdateSeasonCommand>
{
    public UpdateSeasonCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class UpdateSeasonCommandHandler : IRequestHandler<UpdateSeasonCommand, SeasonDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateSeasonCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<SeasonDto> Handle(UpdateSeasonCommand request, CancellationToken cancellationToken)
    {
        var season = await _db.Seasons.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Season), request.Id);

        var name = request.Name.Trim();
        if (name != season.Name)
        {
            var clash = await _db.Seasons.AnyAsync(s => s.Id != request.Id && s.Name == name, cancellationToken);
            if (clash)
                throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("name", "اسم الموسم مستخدم بالفعل") });
        }

        season.Update(name, request.Notes);
        await _db.SaveChangesAsync(cancellationToken);

        return SeasonDto.From(season);
    }
}
