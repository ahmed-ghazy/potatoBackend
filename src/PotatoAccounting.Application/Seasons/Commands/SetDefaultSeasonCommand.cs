using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Seasons.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Seasons.Commands;

public record SetDefaultSeasonCommand(int Id) : IRequest<SeasonDto>;

public class SetDefaultSeasonCommandHandler : IRequestHandler<SetDefaultSeasonCommand, SeasonDto>
{
    private readonly IApplicationDbContext _db;

    public SetDefaultSeasonCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<SeasonDto> Handle(SetDefaultSeasonCommand request, CancellationToken cancellationToken)
    {
        var target = await _db.Seasons.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Season), request.Id);

        if (target.IsDefault)
            return SeasonDto.From(target);

        // Clear the existing default within this tenant before flipping the new one
        // — the unique index on (TenantId) WHERE IsDefault = 1 forbids two defaults.
        var currentDefaults = await _db.Seasons.Where(s => s.IsDefault).ToListAsync(cancellationToken);
        foreach (var s in currentDefaults)
            s.ClearDefault();
        await _db.SaveChangesAsync(cancellationToken);

        target.MarkAsDefault();
        await _db.SaveChangesAsync(cancellationToken);

        return SeasonDto.From(target);
    }
}
