using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using Users.Api.Controllers;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Validations;
using Users.Domain.Entities;

namespace UsersApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _sut;
        private readonly LoginRequestValidator _validator;
        private readonly ILogger<AuthController> _logger;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserService> _userServiceMock;

        public AuthControllerTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _userServiceMock = new Mock<IUserService>();
            _logger = new NullLogger<AuthController>();
            _validator = new LoginRequestValidator();
            _sut = new AuthController(_userServiceMock.Object, _logger, _tokenServiceMock.Object, _validator);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("@.com", "")]
        [InlineData("test123@gmail.com", "12345")]
        [InlineData("string", "123456")]
        public async Task Login_ShouldThrowValidationException_WhenDataIsInvalid(string email, string password)
        {
            var request = new LoginRequestModel { Email = email, Password = password };
            var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _sut.Login(request));

            Assert.NotEmpty(exception.Errors);
        }

        [Fact]
        public async Task Login_ShouldReturn404_WhenUserDoesNotExist()
        {
            var request = new LoginRequestModel { Email = "testuser123@gmail.com", Password = "345678" };

            _userServiceMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            var result = await _sut.Login(request);

            var badObject = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, badObject.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturn200_WithCorrectCredentials()
        {
            var request = new LoginRequestModel { Email = "user123@gmail.com", Password = "user@123" };

            var user = new User
            {
                Id = new Guid("661FB70B-19D5-4E38-B484-1376F233B9A2"),
                FullName = "test user23",
                Email = "user123@gmail.com",
                PasswordHash = "$2a$11$M8MNUBKQEdd9tqiYo4f8ku2L2yjjOEDzuRYJ8iCrPQ5CD5yPg5avO"
            };

            _userServiceMock.Setup(u => u.GetUserByFuncExpression(It.IsAny<Expression<Func<User, bool>>>()))
                        .ReturnsAsync(user);

            var result = await _sut.Login(request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, ok.StatusCode);
            Assert.IsType<LoginResponseModel>(ok.Value);
        }
    }
}
