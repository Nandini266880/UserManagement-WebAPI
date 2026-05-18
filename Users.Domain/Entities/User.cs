using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        [EmailAddress, Required, MaxLength(50)]
        public string Email { get; set; }

        [Required, MaxLength(500)]
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.Visitor;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
