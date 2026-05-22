using FluentValidation;
using MediatR;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Profile.Dtos;

namespace PotatoAccounting.Application.Profile.Commands;

public record UpdateMyProfileCommand(string FullName, string PhoneNumber, string? Email) : IRequest<ProfileDto>;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50)
            .WithMessage("رقم التليفون مطلوب");
        RuleFor(x => x.Email).MaximumLength(200);
    }
}

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ProfileDto>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyProfileCommandHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task<ProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("لا يوجد سياق مستخدم");
        return _identity.UpdateMyProfileAsync(userId, request.FullName, request.PhoneNumber, request.Email);
    }
}
