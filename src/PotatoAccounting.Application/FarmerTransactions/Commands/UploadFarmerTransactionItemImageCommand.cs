using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record UploadFarmerTransactionItemImageCommand(
    int TransactionId,
    int ItemId,
    string FileName,
    Stream Content,
    string ContentType,
    long Length) : IRequest<FarmerTransactionItemDto>;

public class UploadFarmerTransactionItemImageCommandHandler
    : IRequestHandler<UploadFarmerTransactionItemImageCommand, FarmerTransactionItemDto>
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public UploadFarmerTransactionItemImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<FarmerTransactionItemDto> Handle(UploadFarmerTransactionItemImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الملف فارغ") });
        if (request.Length > MaxBytes)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الحد الأقصى لحجم الصورة 5 ميجابايت") });
        if (!AllowedTypes.Contains(request.ContentType))
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "نوع الملف غير مدعوم. المسموح: JPEG, PNG, WEBP, GIF") });

        var tx = await _context.FarmerTransactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.TransactionId);

        if (!_currentUser.IsAdmin && tx.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        tx.EnsureEditable();

        var item = tx.FindItem(request.ItemId)
            ?? throw new NotFoundException(nameof(FarmerTransactionItem), request.ItemId);

        var savedUrl = await _storage.SaveAsync("farmers", request.FileName, request.Content, cancellationToken);

        var previous = item.ImagePath;
        item.AttachImage(savedUrl);
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(previous))
            _storage.Delete(previous);

        return FarmerTransactionItemDto.From(item);
    }
}
