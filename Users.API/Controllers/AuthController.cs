using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Utility;

namespace Users.API.Controllers
{

    // POST api/{version}/auth/login

    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("/api/v{version:apiVersion}/[controller]")]
    [ApiController]
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
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>JWT token and expiry on success.</returns>
        /// <response code="200">Login successful - JWT returned.</response>
        /// <response code="400">Request format invalid (validation failed).</response>
        /// <response code="401">Credentials do not match.</response>
        /// <response code="404">User not found.</response>

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel request, CancellationToken cancellationToken)
        {

            var user = await _userService.GetUserByFuncExpression(u => u.Email.Equals(request.Email), cancellationToken);
            if(user is null)
            {
                _logger.LogWarning("Login: An unknown user attempt for login.");
                return NotFound("User not found. Kindly check your credentials.");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                var maskedEmail = LogMaskHelper.MaskEmail(request.Email);

                _logger.LogError("Login: Attempt login using {Email} with invalid password", maskedEmail);
                return Unauthorized("Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully");
            var accessToken = await _tokenService.GetJWTAccessToken(user, cancellationToken);

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
