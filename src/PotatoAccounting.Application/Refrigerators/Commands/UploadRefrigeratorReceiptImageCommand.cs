using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record UploadRefrigeratorReceiptImageCommand(
    int RefrigeratorId,
    string FileName,
    Stream Content,
    string ContentType,
    long Length) : IRequest<RefrigeratorDto>;

public class UploadRefrigeratorReceiptImageCommandHandler
    : IRequestHandler<UploadRefrigeratorReceiptImageCommand, RefrigeratorDto>
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _storage;

    public UploadRefrigeratorReceiptImageCommandHandler(IApplicationDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<RefrigeratorDto> Handle(UploadRefrigeratorReceiptImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الملف فارغ") });
        if (request.Length > MaxBytes)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الحد الأقصى لحجم الصورة 5 ميجابايت") });
        if (!AllowedTypes.Contains(request.ContentType))
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "نوع الملف غير مدعوم. المسموح: JPEG, PNG, WEBP, GIF") });

        var r = await _db.Refrigerators
            .FirstOrDefaultAsync(x => x.Id == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.RefrigeratorId);

        var savedUrl = await _storage.SaveAsync("refrigerator-receipts", request.FileName, request.Content, cancellationToken);

        var previous = r.ReceiptImagePath;
        r.AttachReceiptImage(savedUrl);
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(previous))
            _storage.Delete(previous);

        // Load items/expenses for the DTO so the frontend gets a fresh full state
        var items = await _db.RefrigeratorItems.Where(i => i.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        var expenses = await _db.RefrigeratorExpenses.Where(e => e.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        return RefrigeratorDto.From(r, items, expenses);
    }
}
