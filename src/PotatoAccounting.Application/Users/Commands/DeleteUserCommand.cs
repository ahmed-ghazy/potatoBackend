using MediatR;
using PotatoAccounting.Application.Common.Interfaces;

namespace PotatoAccounting.Application.Users.Commands;

public record DeleteUserCommand(string Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => _identity.DeleteUserAsync(request.Id, _currentUser.UserId, _currentUser.TenantId, _currentUser.IsSuperAdmin);
}
