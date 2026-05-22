using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Traders.Dtos;

namespace PotatoAccounting.Application.Traders.Queries;

public record GetAllTradersQuery(string? Search = null) : IRequest<IReadOnlyList<TraderDto>>;

public class GetAllTradersQueryHandler : IRequestHandler<GetAllTradersQuery, IReadOnlyList<TraderDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllTradersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TraderDto>> Handle(GetAllTradersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Traders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{s}%") || (t.PhoneNumber != null && EF.Functions.Like(t.PhoneNumber, $"%{s}%")));
        }

        var items = await query
            .OrderBy(t => t.Name)
            .Select(t => new TraderDto
            {
                Id = t.Id,
                Name = t.Name,
                PhoneNumber = t.PhoneNumber,
                CreatedAt = t.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
