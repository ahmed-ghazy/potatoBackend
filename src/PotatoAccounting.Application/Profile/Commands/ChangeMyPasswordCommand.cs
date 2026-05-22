using FluentValidation;
using MediatR;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;

namespace PotatoAccounting.Application.Profile.Commands;

public record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

public class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة");
    }
}

public class ChangeMyPasswordCommandHandler : IRequestHandler<ChangeMyPasswordCommand>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public ChangeMyPasswordCommandHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("لا يوجد سياق مستخدم");
        return _identity.ChangeMyPasswordAsync(userId, request.CurrentPassword, request.NewPassword);
    }
}
