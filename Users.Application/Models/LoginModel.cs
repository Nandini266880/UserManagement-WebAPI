using System.ComponentModel.DataAnnotations;

namespace Users.Application.Models
{
    /// <summary>Credentials required to authenticate a user.</summary>
    public class LoginRequestModel
    {
        /// <summary>Registered email address of the user. Required.</summary>
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Account password. Required.</summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }


    /// <summary>Response returned upon successful authentication.</summary>
    public class LoginResponseModel
    {
        /// <summary>Unique identifier of the authenticated user.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Full name of the authenticated user.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Role of the authenticated user.</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>JWT access token for authorizing subsequent requests.</summary>
        public string AccessToken { get; set; } = string.Empty;
    }
}
