using System.Linq.Expressions;
using Users.Application.Models;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken);
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
                    UserQuery userQuery, CancellationToken cancellationToken = default);
        Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter, CancellationToken cancellationToken);
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken);
        Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

    }
}
