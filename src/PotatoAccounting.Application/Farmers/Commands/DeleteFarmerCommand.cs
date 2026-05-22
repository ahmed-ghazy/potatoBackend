using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Farmers.Commands;

public record DeleteFarmerCommand(int Id) : IRequest;

public class DeleteFarmerCommandHandler : IRequestHandler<DeleteFarmerCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteFarmerCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteFarmerCommand request, CancellationToken cancellationToken)
    {
        var farmer = await _db.Farmers.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.Id);

        var hasTransactions = await _db.FarmerTransactions.AnyAsync(t => t.FarmerId == request.Id, cancellationToken);
        if (hasTransactions)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("id", "لا يمكن حذف الفلاح لوجود فواتير مرتبطة به") });

        _db.Farmers.Remove(farmer);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
