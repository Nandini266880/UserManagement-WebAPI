using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using Users.API.Exceptions;
using Users.Application.Exceptions;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Utility;
using Users.Domain.Common;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRespository, ILogger<UserService> logger)
        {
            _userRepository = userRespository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user from the provided request model.
        /// </summary>
        /// <param name="request">Create user request containing FullName, Email and Password.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="UserResponseModel"/> representing the created user.</returns>
        /// <exception cref="ConflictException">Thrown when a user with the same email already exists.</exception>
        public async Task<UserResponseModel> CreateUserAsync(CreateUserRequestModel request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetUserByFuncExpression(u => u.Email.Equals(request.Email), cancellationToken);
            if (existingUser != null)
            {
                var maskedEmail = LogMaskHelper.MaskEmail(request.Email);
                _logger.LogWarning("Email {Email} is already registered", maskedEmail);

                throw new ConflictException($"Email {maskedEmail} is already registered");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = UserRole.Visitor,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsActive = true
            };

            await _userRepository.CreateUserAsync(user, cancellationToken);
            _logger.LogInformation("Created a new User {Id}", user.Id);

            return new UserResponseModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                CreatedAt = string.Format("{0:f}", user.CreatedAt),
                UpdatedAt = null
            };
        }

        /// <summary>
        /// Retrieves all users in a summary projection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>An enumerable of <see cref="UserSummaryModel"/>.</returns>
        public async Task<IEnumerable<UserSummaryModel>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllUsersAsync(cancellationToken);
            return users.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves all users with pagination and sorting (V2)
        /// </summary>
        /// <param name="userQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Paged users returned.</response>
        public async Task<PagedResult<UserSummaryModel>> GetPagedUsersAsync(UserQuery userQuery, 
            CancellationToken cancellationToken = default)
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(userQuery, cancellationToken);

            var userSummary = users.Select(MapToDto);

            var finalUserList = new PagedResult<UserSummaryModel>(userSummary, totalCount, userQuery.PageNumber, userQuery.PageSize);

            return finalUserList;
        }

        /// <summary>
        /// Retrieves a user entity that matches the provided filter expression.
        /// </summary>
        /// <param name="filter">An expression used to find the user.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The matched <see cref="User"/> or <c>null</c> if none found.</returns>
        public async Task<User?> GetUserByFuncExpression(Expression<Func<User, bool>> filter, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByFuncExpression(filter, cancellationToken);
        }

        /// <summary>
        /// Retrieves a user by identifier and maps it to a response model.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="UserResponseModel"/> for the requested user.</returns>
        /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
        public async Task<UserResponseModel?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (user is null)
                throw new NotFoundException($"User {userId} not found.");

            var mappedUser = new UserResponseModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                CreatedAt = string.Format("{0:f}", user.CreatedAt),
                UpdatedAt = string.Format("{0:f}", user.UpdatedAt)
            };

            _logger.LogInformation("Get: User {Id} data requested", userId);
            return mappedUser;
        }

        /// <summary>
        /// Updates an existing user with values from the request model.
        /// </summary>
        /// <param name="userId">Identifier of the user to update.</param>
        /// <param name="request">Update request containing fields to change (FullName, optional Email, optional Password).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated <see cref="UserResponseModel"/>.</returns>
        /// <exception cref="NotFoundException">Thrown when the target user does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the requested email is already taken by another user.</exception>
        public async Task<UserResponseModel> UpdateUserAsync(Guid userId, UpdateUserRequestModel request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if(existingUser is null)
                throw new NotFoundException($"User {userId} not found.");

            if (!string.IsNullOrWhiteSpace(request.Email) && existingUser.Email != request.Email)
            {
                var emailTaken = await _userRepository.GetUserByFuncExpression(u =>
                    u.Email.Equals(request.Email) && u.Id != userId, cancellationToken);

                if (emailTaken != null)
                    throw new ConflictException("Email is already registered!");

                existingUser.Email = request.Email;
            }

            existingUser.FullName = request.FullName;
            existingUser.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            var result = await _userRepository.UpdateUserAsync(existingUser, cancellationToken);
            _logger.LogInformation("Updated a User {Id}", result.Id);

            var mappedUser = new UserResponseModel
            {
                Id = result.Id,
                FullName = result.FullName,
                Role = result.Role.ToString(),
                CreatedAt = string.Format("{0:f}", result.CreatedAt),
                UpdatedAt = string.Format("{0:f}", result.UpdatedAt)
            };

            return mappedUser;
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="userId">Identifier of the user to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByFuncExpression(u => u.Id == userId, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException($"User {userId} not found.");
            }

            await _userRepository.DeleteUserAsync(userId, cancellationToken);
            _logger.LogInformation("Deleted a User {Id} is ", userId);
        }


        #region Private Helper Methods

        private static UserSummaryModel MapToDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,   
        };

        #endregion
    }
}
