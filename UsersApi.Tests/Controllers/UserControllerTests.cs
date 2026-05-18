using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Users.Api.Controllers;
using Users.API.Exceptions;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Validations;

namespace UsersApi.Tests.Controllers
{
    /// <summary>
    /// These test cases are for controller by mocking UserService
    /// Moq          → creates fake implementations of interfaces
    /// [Fact]       → single test case, no parameters
    /// [Theory]     → same test logic with multiple different inputs
    /// [InlineData] → provides the input values for [Theory]
    /// </summary>
    public class UserControllerTests
    {
        private readonly UserController _sut;
        private readonly ILogger<UserController> _logger;
        private readonly CreateUserRequestValidator _createUserValidator;
        private readonly UpdateUserRequestValidator _updateUserValidator;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _logger = new NullLogger<UserController>();
            _createUserValidator = new CreateUserRequestValidator();
            _updateUserValidator = new UpdateUserRequestValidator();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _sut = new UserController(_userServiceMock.Object, _currentUserServiceMock.Object,
                _logger, _createUserValidator, _updateUserValidator);
        }

        // GET api/v1/users

        [Fact]
        public async Task GetAllUsers_ShouldReturn200_WhenUsersExist()
        {
            var users = new List<UserSummaryModel>
            {
                new() { Id=Guid.NewGuid(), FullName="test user 1", Email="testuser1@gmail.com"},
                new() { Id=Guid.NewGuid(), FullName="test user 2", Email="testuser2@gmail.com"},
            };

            _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _sut.GetAllUsers();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);
            var returned = Assert.IsAssignableFrom<IEnumerable<UserSummaryModel>>(ok.Value);
            Assert.Equal(2, returned.Count());
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturn200WithEmptyList_WhenNoUsersExist()
        {
            var users = new List<UserSummaryModel>();

            _userServiceMock.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            var result = await _sut.GetAllUsers();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);

            var returned = Assert.IsAssignableFrom<IEnumerable<UserSummaryModel>>(ok.Value);
            Assert.Empty(returned);
        }

        // api/v1/users/{id}

        [Theory]
        [InlineData("test user 1", "Visitor")]
        [InlineData("test user 2", "Visitor")]
        [InlineData("test user 3", "Admin")]
        public async Task GetUserById_ShouldReturnCorrectData_ForDifferentUsers(string name, string role)
        {
            var userId = Guid.NewGuid();
            var user = new UserResponseModel { Id=userId, FullName=name, Role=role, CreatedAt="", UpdatedAt=null };

            _userServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _sut.GetUserById(userId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<UserResponseModel>(ok.Value);
            Assert.Equal(userId, returned.Id);
            Assert.Equal(name, returned.FullName);
            Assert.Equal(role, returned.Role);
        }

        [Fact]
        public async Task GetUserById_ShouldPropogateNotFoundException_WhenUserDoNotExist()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserByIdAsync(userId))
                .ThrowsAsync(new NotFoundException($"User: {userId}"));

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.GetUserById(userId));
        }

        // POST api/v1/users

        [Theory]
        [InlineData("", "", "")]
        [InlineData("test user 4", "@.com", "")]
        [InlineData("test user 4", "string", "password")]

        public async Task CreateUser_ShouldThrowValidationException_ForInvalidData(string name, string email, string password)
        {
            var creatingUser = new CreateUserRequestModel { FullName = name, Email = email, Password = password };

            var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _sut.CreateUser(creatingUser)
                );

            Assert.NotEmpty(exception.Errors);

        }

        [Fact]
        public async Task CreateUser_ShouldReturn201_ForValidData()
        {
            var request = new CreateUserRequestModel { FullName = "test user", Email = "testuser4@gmail.com", Password = "Password@123" };
            
            var response = new UserResponseModel { 
                Id=Guid.NewGuid(), FullName = "test user", 
                CreatedAt = new DateTime().ToString(),
                UpdatedAt = null,
            };

            _userServiceMock.Setup(u => u.CreateUserAsync(request))
                .ReturnsAsync(response);

            var result = await _sut.CreateUser(request);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, created.StatusCode);

            var returned = Assert.IsAssignableFrom<UserResponseModel>(created.Value);
            Assert.Equal(request.FullName, returned.FullName);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturn200_WhenDataIsValid()
        {
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequestModel { FullName = "New Name", Email = "new@gmail.com", Password = "Password@123" };
            var response = new UserResponseModel { Id = userId, FullName = "New Name"};

            _userServiceMock.Setup(s => s.UpdateUserAsync(userId, request))
                .ReturnsAsync(response);

            var result = await _sut.UpdateUser(userId, request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);
            _userServiceMock.Verify(s => s.UpdateUserAsync(userId, request), Times.Once);
        }

        [Theory]
        [InlineData("", "new@gmail.com", "Password@123")] // empty name → 400
        [InlineData("New Name", "notanemail", "Password@123")] // bad email format → 400
        [InlineData("New Name", "@.com", "Password@123")] // bad email format → 400
        [InlineData("New Name", null, "pass")] // too short → 400

        public async Task UpdateUser_ShouldThrowValidationException_WhenDataIsInvalid(
            string name, string? email, string? password)
        {
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequestModel { FullName = name, Email = email, Password = password };

            await Assert.ThrowsAsync<ValidationException>(() => 
                _sut.UpdateUser(userId, request));

            var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _sut.UpdateUser(userId, request)
                );

            Assert.NotEmpty(exception.Errors);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task DeleteUser_ShouldReturn400_WhenDataIsInvalid(Guid userId)
        {
            var result = await _sut.DeleteUser(userId);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
            _userServiceMock.Verify(s => s.DeleteUserAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
