using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetJWTAccessToken(User user, CancellationToken cancellationToken);
    }
}
