using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Domain.Entities;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Users.ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
                    UserQuery userQuery, CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.Users.AsQueryable();

            var sortCol = UserQuery.AllowedSortColumns.Contains(userQuery.SortBy)
                ? userQuery.SortBy : "Full Name";

            dbQuery = sortCol switch
            {
                "Email" => userQuery.IsDescending ? 
                        dbQuery.OrderByDescending(x => x.Email) : dbQuery.OrderBy(x => x.Email),

                "CreatedAt" => userQuery.IsDescending ? 
                        dbQuery.OrderByDescending(x => x.CreatedAt) : dbQuery.OrderBy(x => x.CreatedAt),

                _ => userQuery.IsDescending ?
                        dbQuery.OrderByDescending(x => x.FullName) : dbQuery.OrderBy(x => x.FullName)
            };

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var users = await dbQuery
                           .Skip((userQuery.PageNumber - 1) * userQuery.PageSize)
                           .Take(userQuery.PageSize)
                           .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        public async Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter, CancellationToken cancellationToken)
        {
            IQueryable<User> query = _dbContext.Users;
            return await query.FirstOrDefaultAsync(filter, cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            return user ?? null;
        }

        public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(userId, cancellationToken);
            if (user is null) return;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
