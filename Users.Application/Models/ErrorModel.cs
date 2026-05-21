namespace Users.Application.Models
{
    /// <summary>Represents a validation error for a specific field.</summary>
    public sealed class FieldError
    {
        /// <summary>Name of the field that failed validation.</summary>
        public string Field { get; init; } = string.Empty;

        /// <summary>Human-readable description of the validation failure.</summary>
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>Standardized error response structure for API problem details.</summary>
    public sealed class ProblemDetail
    {
        /// <summary>Short summary of the problem (e.g., "Validation Failed").</summary>
        public string? Title { get; set; }

        /// <summary>HTTP status code associated with the problem.</summary>
        public int? Status { get; set; }

        /// <summary>Relative URI identifying the specific request that caused the problem.</summary>
        public string? Instance { get; set; }

        /// <summary>List of field-level validation errors. Null if no field errors.</summary>
        public List<FieldError>? Errors { get; set; }
    }
}
