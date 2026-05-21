using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Users.API.Helpers;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Domain.Common;

namespace Users.API.Controllers
{

    // GET api/v1/users
    // GET api/v2/users
    // POST api/v1/users
    // GET api/v1/users/{id}
    // PUT api/v1/users/{id}
    // DELETE api/v1/users/{id}

    /// <summary>
    /// Handles CRUD operations for user accounts.
    /// All business logic lives in the respective command handlers.
    /// </summary>
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("/api/v{version:apiVersion}/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Private Members

        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UsersController> _logger;
        private readonly IValidator<CreateUserRequestModel> _createUserValidator;
        private readonly IValidator<UpdateUserRequestModel> _updateUserValidator;

        #endregion

        #region Constructor
        public UsersController(IUserService userService, ICurrentUserService currentUserService, ILogger<UsersController> logger, IValidator<CreateUserRequestModel> createUserValidator, 
            IValidator<UpdateUserRequestModel> updateUserValidator)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _logger = logger;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
        }

        #endregion

        #region WEB APIs


        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">User creation request model.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Created user details.</returns>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Validation failed.</response>
        [HttpPost]
        public async Task<ActionResult<UserResponseModel>> CreateUser([FromBody] CreateUserRequestModel request, CancellationToken cancellationToken)
        {   
            var response = await _userService.CreateUserAsync(request, cancellationToken);
           
            return CreatedAtAction( nameof(GetUserById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Retrieves a list of users (summary view).
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of user summaries.</returns>
        /// <response code="200">Users returned successfully.</response>
        //[Authorize(Roles = "Admin")]
        [MapToApiVersion("1.0")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSummaryModel>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Get paged + sorted users (V2).
        /// </summary>
        /// <param name="userQuery">Pagination and sort options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Paged users returned.</response>
        //[Authorize(Roles = "Admin")]
        [MapToApiVersion("2.0")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserSummaryModel>>> GetAllUsers([FromQuery] UserQuery userQuery,
            CancellationToken cancellationToken)
        {
            var users = await _userService.GetPagedUsersAsync(userQuery, cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>User profile for the specified id.</returns>
        /// <response code="200">User profile returned.</response>
        [HttpGet("{id}")]
        [ServiceFilter(typeof(ValidateGuidFilter))]
        public async Task<ActionResult<UserResponseModel>> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="request">Update request containing changed fields.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated user details.</returns>
        /// <response code="200">User updated successfully.</response>
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateGuidFilter))]
        public async Task<ActionResult<UserResponseModel>> UpdateUser(Guid id, [FromBody] UpdateUserRequestModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update: Request for User {Id}", id);
            var updated = await _userService.UpdateUserAsync(id, request, cancellationToken);

            return Ok(updated);
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>No content on successful deletion.</returns>
        /// <response code="204">User deleted successfully.</response>
        //[Authorize(Roles = "Admin")]
        [ServiceFilter(typeof(ValidateGuidFilter))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.GetEmail();
            _logger.LogInformation("Delete: Request for User {Id} by Admin {Email}", id, currentUser);

            await _userService.DeleteUserAsync(id, cancellationToken);
            return NoContent();
        }
        #endregion

    }
}
