using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Users.Application.Models
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
