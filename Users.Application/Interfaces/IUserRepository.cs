using System.Linq.Expressions;
using Users.Application.Models;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);

    }
}
