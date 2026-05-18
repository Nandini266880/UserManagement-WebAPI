using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Models
{
    public sealed class FieldError
    {
        public string Field { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }

    public sealed class ProblemDetail
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Instance { get; set; }
        public List<FieldError>? Errors { get; set; }
    }
}
