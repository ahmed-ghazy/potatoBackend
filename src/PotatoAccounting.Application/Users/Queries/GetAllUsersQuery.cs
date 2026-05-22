using MediatR;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Users.Dtos;

namespace PotatoAccounting.Application.Users.Queries;

public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public GetAllUsersQueryHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // SuperAdmin sees everyone, Admin sees only their tenant
        var filter = _currentUser.IsSuperAdmin ? (int?)null : _currentUser.TenantId;
        return _identity.GetUsersAsync(filter);
    }
}
