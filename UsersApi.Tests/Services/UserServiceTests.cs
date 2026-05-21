using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using Users.API.Exceptions;
using Users.Application.Exceptions;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Services;
using Users.Domain.Entities;

namespace UsersApi.Tests.Services
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly ILogger<UserService> _logger;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserServiceTests()
        {
            _logger = new NullLogger<UserService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _sut = new UserService(_userRepositoryMock.Object, _logger);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnUser_WhenEmailIsNotTaken()
        {
            var request = new CreateUserRequestModel { FullName = "test user4", Email = "user4@gmail.com", Password = "user@4" };

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                .ReturnsAsync((User?)null);

            var result = await _sut.CreateUserAsync(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(request.FullName, result.FullName);
        }

        [Fact]
        public async Task CreateUser_ShouldThrowConflictException_WhenEmailIsAlreadyTaken()
        {
            var request = new CreateUserRequestModel { FullName = "test user5", Email = "user5@gmail.com", Password = "user@5" };
            var existing = new User { Id = Guid.NewGuid(), FullName = "test user5", Email = "user5@gmail.com" };

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                .ReturnsAsync(existing);

            await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateUserAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateUser_ShouldReturnUser_WhenDataIsValid()
        {
            var request = new CreateUserRequestModel { FullName = "test user6", Email = "user6@gmail.com", Password = "user@6" };
            var existing = new User { Id = Guid.NewGuid(), FullName = "test user6", Email = "user6@gmail.com" };

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(u => u.CreateUserAsync(existing, CancellationToken.None));

            var result = await _sut.CreateUserAsync(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<UserResponseModel>(result);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers()
        {
            var data = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "test user4", Email = "user4@gmail.com" },
                new() { Id = Guid.NewGuid(), FullName = "test user5", Email = "user5@gmail.com" },
            };

            _userRepositoryMock.Setup(u => u.GetAllUsersAsync(CancellationToken.None))
                .ReturnsAsync(data);

            var result = await _sut.GetAllUsersAsync(CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Theory]
        [InlineData("09DBFE4B-40EC-4C9B-B0DC-DECC21FCDDC6")]
        [InlineData("87DBF287-FD66-4EA7-862C-5E27CB35C322")]
        public async Task GetUserById_ShouldThrowNotFoundEXception_WhenUserDoesNotExist(Guid userId)
        {
            var currentData = new List<User>
            {
                new() { Id = Guid.NewGuid(), FullName = "test user2", Email = "user2@gmail.com" },
                new() { Id = Guid.NewGuid(), FullName = "test user3", Email = "user3@gmail.com" },
            };

            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync(currentData.FirstOrDefault(u => u.Id == userId));

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserByIdAsync(userId, CancellationToken.None));
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            Guid userId = Guid.NewGuid();
            var currentData = new List<User>
            {
                new() { Id = userId, FullName = "test user4", Email = "user4@gmail.com" },
                new() { Id = new Guid("715A3852-723B-4B9F-952D-515879D929E2"), FullName = "test user5", Email = "user5@gmail.com" },
            };

            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync(currentData.FirstOrDefault(u => u.Id == userId));

            var result = await _sut.GetUserByIdAsync(userId, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task UpdateUser_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequestModel { FullName = "nandini", Email = "nandini@gmail.com", Password = "nandini@123" };

            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateUserAsync(userId, request, CancellationToken.None));
        }


        [Fact]
        public async Task UpdateUser_ShouldThrowConflictException_WhenRequestEmailIsTaken()
        {
            var userId = Guid.NewGuid();

            var request = new UpdateUserRequestModel { FullName = "test user5", Email = "user63@gmail.com", Password = "user@5" };
            var existingUser = new User { Id = userId, FullName = "test user5", Email = "user5@gmail.com" };
            var anotherUser = new User { Id=Guid.NewGuid(), FullName = "test user63", Email = "user63@gmail.com" };

            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                    .ReturnsAsync(anotherUser);

            await Assert.ThrowsAsync<ConflictException>(() => _sut.UpdateUserAsync(userId, request, CancellationToken.None));
           
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, FullName = "Old Name", Email = "old@gmail.com"};

            var request = new UpdateUserRequestModel
            {
                FullName = "New Name",
                Email = "new@gmail.com",
                Password = ""
            };

            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(userId, CancellationToken.None))
               .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(u => u.UpdateUserAsync(
                    It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingUser); 


            var result = await _sut.UpdateUserAsync(userId, request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(result.FullName, request.FullName);
        }

        [Theory]
        [InlineData("09DBFE4B-40EC-4C9B-B0DC-DECC21FCDDC6")]
        [InlineData("87DBF287-FD66-4EA7-862C-5E27CB35C322")]
        public async Task DeleteUser_ShouldThrowNotFoundException_WhenUserDoesNotExist(Guid userId)
        {
            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteUserAsync(userId, CancellationToken.None));

            _userRepositoryMock.Verify(x => x.DeleteUserAsync(userId, CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldDeleteUser_WhenUserExists()
        {
            Guid userId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "test user4", Email = "user4@gmail.com" };

            _userRepositoryMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(u => u.DeleteUserAsync(userId, CancellationToken.None));

            await _sut.DeleteUserAsync(userId, CancellationToken.None);
            _userRepositoryMock.Verify(x => x.DeleteUserAsync(userId, CancellationToken.None), Times.Once());
        }

    }
}
