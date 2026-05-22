using FluentValidation;
using MediatR;
using PotatoAccounting.Application.Auth.Dtos;
using PotatoAccounting.Application.Common.Interfaces;

namespace PotatoAccounting.Application.Auth.Commands;

public record LoginCommand(string PhoneNumber, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("رقم التليفون مطلوب");
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        => _identityService.LoginAsync(request.PhoneNumber, request.Password);
}
