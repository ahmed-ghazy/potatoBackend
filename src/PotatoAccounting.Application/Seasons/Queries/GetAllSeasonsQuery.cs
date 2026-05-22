using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Seasons.Dtos;

namespace PotatoAccounting.Application.Seasons.Queries;

public record GetAllSeasonsQuery : IRequest<IReadOnlyList<SeasonDto>>;

public class GetAllSeasonsQueryHandler : IRequestHandler<GetAllSeasonsQuery, IReadOnlyList<SeasonDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllSeasonsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SeasonDto>> Handle(GetAllSeasonsQuery request, CancellationToken cancellationToken)
    {
        var seasons = await _db.Seasons
            .AsNoTracking()
            .OrderByDescending(s => s.IsDefault)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return seasons.Select(SeasonDto.From).ToList();
    }
}
