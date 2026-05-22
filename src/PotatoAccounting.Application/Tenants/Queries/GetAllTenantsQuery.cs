using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Tenants.Dtos;

namespace PotatoAccounting.Application.Tenants.Queries;

public record GetAllTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTenantsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return tenants;
    }
}
