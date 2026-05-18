using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;
using Users.Application.Models;

namespace Users.Api.Controllers
{

    // GET api/v1/users
    // POST api/v1/users
    // GET api/v1/users/{id}
    // PUT api/v1/users/{id}
    // DELETE api/v1/users/{id}

    /// <summary>
    /// Handles CRUD operations for user accounts.
    /// All business logic lives in the respective command handlers.
    /// </summary>
    [ApiController]
    [Route("/api/v1/users")]
    public class UserController : ControllerBase
    {
        #region Private Members

        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserController> _logger;
        private readonly IValidator<CreateUserRequestModel> _createUserValidator;
        private readonly IValidator<UpdateUserRequestModel> _updateUserValidator;

        #endregion

        #region Constructor
        public UserController(IUserService userService, ICurrentUserService currentUserService, ILogger<UserController> logger, IValidator<CreateUserRequestModel> createUserValidator, 
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
        /// <returns>Created user details.</returns>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Validation failed.</response>
        [HttpPost]
        public async Task<ActionResult<UserResponseModel>> CreateUser([FromBody] CreateUserRequestModel request)
        {   
            _logger.LogInformation("Create: A new User {Email}", request.Email);
            var validation = await _createUserValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                _logger.LogError("Create Validation: Error creating user {Email} and {Validation}"
                    , request.Email, validation.Errors.ToString());
                throw new ValidationException(validation.Errors);
            }

            var response = await _userService.CreateUserAsync(request);
            return CreatedAtAction( nameof(GetUserById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Retrieves a list of users (summary view).
        /// </summary>
        /// <returns>List of user summaries.</returns>
        /// <response code="200">Users returned successfully.</response>
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSummaryModel>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>User profile for the specified id.</returns>
        /// <response code="200">User profile returned.</response>
        /// <response code="400">Invalid id supplied (Guid.Empty).</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseModel>> GetUserById(Guid id)
        {
            if(id == Guid.Empty)
            {
                _logger.LogWarning("GetUserById: User {Id} is Guid.Empty. Result in BadRequest(400)", id);
                return BadRequest("User Id cannot be Guid.Empty");
            }
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="request">Update request containing changed fields.</param>
        /// <returns>Updated user details.</returns>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid id supplied or validation failed.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseModel>> UpdateUser(Guid id, [FromBody] UpdateUserRequestModel request)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Update: User {Id} is Guid.Empty. Result in BadRequest(400)", id);
                return BadRequest("User Id cannot be Guid.Empty");
            }

            var validation = await _updateUserValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                _logger.LogError("Update Validation: Error updating user {Id} and {Validation}", id, validation.Errors.ToString());
                throw new ValidationException(validation.Errors);
            }

            _logger.LogInformation("Update: Request for User {Id}", id);
            var updated = await _userService.UpdateUserAsync(id, request);

            return Ok(updated);
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        /// <response code="204">User deleted successfully.</response>
        /// <response code="400">Invalid id supplied (Guid.Empty).</response>
        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Delete: User {Id} is Guid.Empty. Result in BadRequest(400)", id);
                return BadRequest("User Id cannot be Guid.Empty");
            }

            var currentUser = _currentUserService.GetEmail();
            _logger.LogInformation("Delete: Request for User {Id} by Admin {Email}", id, currentUser);

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        #endregion
    }
}
