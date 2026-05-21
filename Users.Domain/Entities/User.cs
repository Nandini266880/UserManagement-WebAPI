using System.ComponentModel.DataAnnotations;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    /// <summary>Represents a registered user in the system.</summary>
    public class User
    {
        /// <summary>Unique identifier of the user.</summary>
        public Guid Id { get; set; }

        /// <summary>Full name of the user.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Unique email address used for authentication.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Bcrypt-hashed password. Never store plain text.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Role assigned to the user. Defaults to Visitor.</summary>
        public UserRole Role { get; set; } = UserRole.Visitor;

        /// <summary>Indicates whether the user account is active.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>UTC timestamp when the user was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last update. Null if never updated.</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
