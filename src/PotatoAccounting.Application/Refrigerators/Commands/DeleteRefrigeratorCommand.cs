using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record DeleteRefrigeratorCommand(int Id) : IRequest;

public class DeleteRefrigeratorCommandHandler : IRequestHandler<DeleteRefrigeratorCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteRefrigeratorCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteRefrigeratorCommand request, CancellationToken cancellationToken)
    {
        var r = await _db.Refrigerators.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.Id);

        // Items + Expenses cascade-delete via FK
        _db.Refrigerators.Remove(r);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
