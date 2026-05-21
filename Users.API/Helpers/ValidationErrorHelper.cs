using Users.Application.Models;

namespace Users.API.Helpers
{
    public static class ValidationErrorHelper
    {
        public static ProblemDetail BuildProblemDetail(IEnumerable<FieldError> errors, string instance)
        {
            
            return new ProblemDetail
            {
                Title = "Validation errors",
                Status = StatusCodes.Status400BadRequest,
                Instance = instance,
                Errors = errors.Select(error => new FieldError
                {
                    Field = error.Field,
                    Message = error.Message
                }).ToList()
            };

        }
    }
}
