using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Queries;

public record GetRefrigeratorByIdQuery(int Id) : IRequest<RefrigeratorDto>;

public class GetRefrigeratorByIdQueryHandler : IRequestHandler<GetRefrigeratorByIdQuery, RefrigeratorDto>
{
    private readonly IApplicationDbContext _db;

    public GetRefrigeratorByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorDto> Handle(GetRefrigeratorByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _db.Refrigerators.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.Id);

        var items = await _db.RefrigeratorItems.AsNoTracking()
            .Where(i => i.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        var expenses = await _db.RefrigeratorExpenses.AsNoTracking()
            .Where(e => e.RefrigeratorId == r.Id).ToListAsync(cancellationToken);

        return RefrigeratorDto.From(r, items, expenses);
    }
}
