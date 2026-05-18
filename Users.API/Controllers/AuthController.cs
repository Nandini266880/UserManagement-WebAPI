using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;
using Users.Application.Models;

namespace Users.Api.Controllers
{

    // POST api/v1/auth/login

    [ApiController]
    [Route("/api/v1/auth")]
    public class AuthController : ControllerBase
    {
        #region Private Members
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IValidator<LoginRequestModel> _loginValidator;
        #endregion

        #region Constructor
        public AuthController(IUserService userService, ILogger<AuthController> logger, ITokenService tokenService, 
            IValidator<LoginRequestModel> loginValidator)
        {
            _userService = userService;
            _logger = logger;
            _tokenService = tokenService;
            _loginValidator = loginValidator;
        }
        #endregion

        /// <summary>
        /// Authenticates a user and returns a signed JWT token.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>JWT token and expiry on success.</returns>
        /// <response code="200">Login successful — JWT returned.</response>
        /// <response code="400">Request format invalid (validation failed).</response>
        /// <response code="401">Credentials do not match.</response>
        /// <response code="404">User not found.</response>

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel request)
        {
            var validation = await _loginValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                _logger.LogError("Login Validation: Couldn't login User {Email} and {Validation}", request.Email, validation.Errors.ToString());
                throw new ValidationException(validation.Errors);
            }

            var user = await _userService.GetUserByFuncExpression(u => u.Email.Equals(request.Email));
            if(user is null)
            {
                _logger.LogWarning("Login: An unknown user attempt for login.");
                return NotFound("User not found. Kindly check your credentials.");
            }

            var isPasswordValid = !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (isPasswordValid)
            {
                _logger.LogError("Login: User {Email} attempt login with invalid password", request.Email);
                return Unauthorized("Invalid credentials");
            }

            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            var accessToken = await _tokenService.GetJWTAccessToken(user);

            return Ok(new LoginResponseModel
            {
                Id = user.Id.ToString(),
                FullName = user.FullName,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
            });
        }
    }
}
