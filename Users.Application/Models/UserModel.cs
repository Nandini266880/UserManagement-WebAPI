using System.ComponentModel.DataAnnotations;

namespace Users.Application.Models
{
    /// <summary>Payload for creating a new user account.</summary>
    public class CreateUserRequestModel
    {
        /// <summary>Full name of the user. Required.</summary>
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>User's unique email address. Required.</summary>
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Password for the user account. Required.</summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }


    /// <summary>Payload for updating an existing user's information.</summary>
    public class UpdateUserRequestModel
    {
        /// <summary>Updated full name of the user. Required.</summary>
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Updated email address. Optional.</summary>
        public string? Email { get; set; }

        /// <summary>Updated password. Optional.</summary>
        public string? Password { get; set; }
    }


    /// <summary>Full user details returned after a successful operation.</summary>
    public class UserResponseModel
    {
        /// <summary>Unique identifier of the user.</summary>
        public Guid Id { get; set; }

        /// <summary>Full name of the user.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Role assigned to the user (e.g., Admin, User).</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>Date and time when the user was created.</summary>
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>Date and time of the last update. Null if never updated.</summary>
        public string? UpdatedAt { get; set; }
    }


    /// <summary>Lightweight user summary for listings or dropdowns.</summary>
    public class UserSummaryModel
    {
        /// <summary>Unique identifier of the user.</summary>
        public Guid Id { get; set; }

        /// <summary>Full name of the user.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Email address of the user.</summary>
        public string Email { get; set; } = string.Empty;
    }
}
