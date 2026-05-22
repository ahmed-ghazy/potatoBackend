using FluentValidation;
using MediatR;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Users.Dtos;

namespace PotatoAccounting.Application.Users.Commands;

public record CreateUserCommand(string PhoneNumber, string Password, string FullName, string Role) : IRequest<UserDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50)
            .WithMessage("رقم التليفون مطلوب");
        RuleFor(x => x.Password).NotEmpty()
            .WithMessage("كلمة المرور مطلوبة");
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).Must(r => r is "Admin" or "User")
            .WithMessage("الدور يجب أن يكون Admin أو User");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public CreateUserCommandHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يمكنك إضافة مستخدم بدون شركة");

        return _identity.CreateUserAsync(request.PhoneNumber, request.Password, request.FullName, request.Role, tenantId);
    }
}
