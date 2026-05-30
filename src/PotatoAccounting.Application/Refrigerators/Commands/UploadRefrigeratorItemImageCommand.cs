using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record UploadRefrigeratorItemImageCommand(
    int RefrigeratorId,
    int ItemId,
    string FileName,
    Stream Content,
    string ContentType,
    long Length) : IRequest<RefrigeratorItemDto>;

public class UploadRefrigeratorItemImageCommandHandler
    : IRequestHandler<UploadRefrigeratorItemImageCommand, RefrigeratorItemDto>
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _storage;

    public UploadRefrigeratorItemImageCommandHandler(IApplicationDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<RefrigeratorItemDto> Handle(UploadRefrigeratorItemImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الملف فارغ") });
        if (request.Length > MaxBytes)
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "الحد الأقصى لحجم الصورة 5 ميجابايت") });
        if (!AllowedTypes.Contains(request.ContentType))
            throw new Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("file", "نوع الملف غير مدعوم. المسموح: JPEG, PNG, WEBP, GIF") });

        var item = await _db.RefrigeratorItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorItem), request.ItemId);

        var savedUrl = await _storage.SaveAsync("refrigerators", request.FileName, request.Content, cancellationToken);

        var previous = item.ImagePath;
        item.AttachImage(savedUrl);
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(previous))
            _storage.Delete(previous);

        return RefrigeratorItemDto.From(item);
    }
}
