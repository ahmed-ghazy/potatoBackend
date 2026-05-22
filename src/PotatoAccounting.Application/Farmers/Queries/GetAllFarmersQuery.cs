using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Farmers.Dtos;

namespace PotatoAccounting.Application.Farmers.Queries;

public record GetAllFarmersQuery(string? Search = null) : IRequest<IReadOnlyList<FarmerDto>>;

public class GetAllFarmersQueryHandler : IRequestHandler<GetAllFarmersQuery, IReadOnlyList<FarmerDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllFarmersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FarmerDto>> Handle(GetAllFarmersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Farmers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(f => EF.Functions.Like(f.Name, $"%{s}%") || (f.PhoneNumber != null && EF.Functions.Like(f.PhoneNumber, $"%{s}%")));
        }

        var items = await query
            .OrderBy(f => f.Name)
            .Select(f => new FarmerDto
            {
                Id = f.Id,
                Name = f.Name,
                PhoneNumber = f.PhoneNumber,
                CreatedAt = f.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
