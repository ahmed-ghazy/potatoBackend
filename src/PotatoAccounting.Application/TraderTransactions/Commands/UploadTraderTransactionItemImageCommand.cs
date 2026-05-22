using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record UploadTraderTransactionItemImageCommand(
    int TransactionId,
    int ItemId,
    string FileName,
    Stream Content,
    string ContentType,
    long Length) : IRequest<TraderTransactionItemDto>;

public class UploadTraderTransactionItemImageCommandHandler
    : IRequestHandler<UploadTraderTransactionItemImageCommand, TraderTransactionItemDto>
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public UploadTraderTransactionItemImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<TraderTransactionItemDto> Handle(UploadTraderTransactionItemImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الملف فارغ") });
        if (request.Length > MaxBytes)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الحد الأقصى لحجم الصورة 5 ميجابايت") });
        if (!AllowedTypes.Contains(request.ContentType))
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "نوع الملف غير مدعوم. المسموح: JPEG, PNG, WEBP, GIF") });

        var tx = await _context.TraderTransactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), request.TransactionId);

        if (!_currentUser.IsAdmin && tx.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        tx.EnsureEditable();

        var item = tx.FindItem(request.ItemId)
            ?? throw new NotFoundException(nameof(TraderTransactionItem), request.ItemId);

        var savedUrl = await _storage.SaveAsync("traders", request.FileName, request.Content, cancellationToken);

        var previous = item.ImagePath;
        item.AttachImage(savedUrl);
        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(previous))
            _storage.Delete(previous);

        return TraderTransactionItemDto.From(item);
    }
}
