using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record UpdateFarmerTransactionCommand(
    int Id,
    DateTime Date,
    int FarmerId,
    string? Notes,
    IReadOnlyList<FarmerTransactionItemInput> Items) : IRequest<FarmerTransactionDto>;

public class UpdateFarmerTransactionCommandValidator : AbstractValidator<UpdateFarmerTransactionCommand>
{
    public UpdateFarmerTransactionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FarmerId).GreaterThan(0).WithMessage("الفلاح مطلوب");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).MaximumLength(200);
            item.RuleFor(i => i.Weight).GreaterThan(0).WithMessage("وزن العنصر يجب أن يكون أكبر من صفر");
            item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateFarmerTransactionCommandHandler
    : IRequestHandler<UpdateFarmerTransactionCommand, FarmerTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public UpdateFarmerTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<FarmerTransactionDto> Handle(UpdateFarmerTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FarmerTransactions
            .Include(t => t.Farmer)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.EnsureEditable();

        if (entity.FarmerId != request.FarmerId)
        {
            var farmerExists = await _context.Farmers.AnyAsync(f => f.Id == request.FarmerId, cancellationToken);
            if (!farmerExists)
                throw new NotFoundException(nameof(Farmer), request.FarmerId);
        }

        entity.UpdateHeader(request.Date, request.FarmerId, request.Notes);

        var incoming = request.Items ?? Array.Empty<FarmerTransactionItemInput>();

        // Diff items by Id: keep+update existing, add new, delete missing
        var keepIds = incoming.Where(i => i.Id is > 0).Select(i => i.Id!.Value).ToList();
        var removedItems = entity.RemoveItemsNotIn(keepIds);

        foreach (var input in incoming)
        {
            if (input.Id is int id && id > 0)
            {
                var existing = entity.FindItem(id);
                if (existing is null)
                    throw new NotFoundException(nameof(FarmerTransactionItem), id);
                existing.Update(input.Description, input.Weight, input.Price, input.HasTare);
            }
            else
            {
                entity.AddItem(FarmerTransactionItem.Create(input.Description, input.Weight, input.Price, input.HasTare));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clean up image files for items that were removed
        foreach (var removed in removedItems)
        {
            if (!string.IsNullOrEmpty(removed.ImagePath))
                _storage.Delete(removed.ImagePath);
        }

        var farmer = await _context.Farmers.AsNoTracking().FirstAsync(f => f.Id == entity.FarmerId, cancellationToken);
        var payments = await _context.FarmerPayments
            .AsNoTracking()
            .Where(p => p.FarmerTransactionId == entity.Id)
            .ToListAsync(cancellationToken);

        var dto = FarmerTransactionDto.From(entity, payments);
        dto.FarmerName = farmer.Name;
        dto.FarmerPhoneNumber = farmer.PhoneNumber;
        return dto;
    }
}
