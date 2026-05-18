using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Users.Application.Interfaces;
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

        public async Task<User> CreateUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter)
        {
            IQueryable<User> query = _dbContext.Users;
            return await query.FirstOrDefaultAsync(filter);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user ?? null;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user is null) return;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
