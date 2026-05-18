using System.Linq.Expressions;
using Users.Application.Models;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseModel> CreateUserAsync(CreateUserRequestModel request);
        Task<IEnumerable<UserSummaryModel>> GetAllUsersAsync();
        Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter);
        Task<UserResponseModel?> GetUserByIdAsync(Guid userId);
        Task<UserResponseModel> UpdateUserAsync(Guid userId, UpdateUserRequestModel request);
        Task DeleteUserAsync(Guid userId);
    }
}
