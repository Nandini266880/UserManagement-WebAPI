using System.Linq.Expressions;
using Users.Application.Models;
using Users.Domain.Common;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseModel> CreateUserAsync(CreateUserRequestModel request, CancellationToken cancellationToken);
        Task<IEnumerable<UserSummaryModel>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<PagedResult<UserSummaryModel>> GetPagedUsersAsync(UserQuery userQuery, 
            CancellationToken cancellationToken = default);
        Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter, CancellationToken cancellationToken);
        Task<UserResponseModel?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserResponseModel> UpdateUserAsync(Guid userId, UpdateUserRequestModel request, CancellationToken cancellationToken);
        Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    }
}
