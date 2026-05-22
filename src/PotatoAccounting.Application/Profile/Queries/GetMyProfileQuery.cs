using MediatR;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Profile.Dtos;

namespace PotatoAccounting.Application.Profile.Queries;

public record GetMyProfileQuery : IRequest<ProfileDto>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public GetMyProfileQueryHandler(IIdentityService identity, ICurrentUserService currentUser)
    {
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("لا يوجد سياق مستخدم");
        return _identity.GetMyProfileAsync(userId);
    }
}
