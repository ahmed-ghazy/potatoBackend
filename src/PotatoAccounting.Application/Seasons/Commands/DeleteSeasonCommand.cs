using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Application.Seasons.Commands;

public record DeleteSeasonCommand(int Id) : IRequest;

public class DeleteSeasonCommandHandler : IRequestHandler<DeleteSeasonCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteSeasonCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteSeasonCommand request, CancellationToken cancellationToken)
    {
        var season = await _db.Seasons.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Season), request.Id);

        if (season.IsDefault)
            throw new DomainException("لا يمكن حذف الموسم الافتراضى");

        // Block deletion when records still reference this season — protects historical data.
        var hasFarmerTx = await _db.FarmerTransactions.IgnoreQueryFilters()
            .AnyAsync(t => t.SeasonId == request.Id, cancellationToken);
        var hasTraderTx = await _db.TraderTransactions.IgnoreQueryFilters()
            .AnyAsync(t => t.SeasonId == request.Id, cancellationToken);
        var hasExpense = await _db.Expenses.IgnoreQueryFilters()
            .AnyAsync(e => e.SeasonId == request.Id, cancellationToken);
        var hasRefrigerator = await _db.Refrigerators.IgnoreQueryFilters()
            .AnyAsync(r => r.SeasonId == request.Id, cancellationToken);

        if (hasFarmerTx || hasTraderTx || hasExpense || hasRefrigerator)
            throw new DomainException("لا يمكن حذف الموسم لأن فيه بيانات مرتبطة به");

        _db.Seasons.Remove(season);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
