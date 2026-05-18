using System.ComponentModel.DataAnnotations;

namespace Users.Application.Models
{
    public class CreateUserRequestModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserRequestModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class UserResponseModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
    }

    public class UserSummaryModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
