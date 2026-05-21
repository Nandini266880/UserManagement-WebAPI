using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;
using Users.Application.Models;

namespace Users.API.Helpers
{
    public class CustomFluentValidationResultFactory : IFluentValidationAutoValidationResultFactory
    {
        private readonly ILogger<CustomFluentValidationResultFactory> _logger;
        public CustomFluentValidationResultFactory(ILogger<CustomFluentValidationResultFactory> logger)
        {
            _logger = logger;
        }

        public Task<IActionResult?> CreateActionResult(ActionExecutingContext context,
            ValidationProblemDetails validationProblemDetails,
            IDictionary<IValidationContext, ValidationResult> validationResults)
        {
            var errors = validationResults
                            .SelectMany(kvp => kvp.Value.Errors)
                            .Select(error => new FieldError
                            {
                                Field = error.PropertyName,
                                Message = error.ErrorMessage,
                            });

            _logger.LogError($"Validation error encountered from Custom Fluent Validation Factory.");
            var problemDetail = ValidationErrorHelper.BuildProblemDetail(errors
                , context.HttpContext.Request.Path);

            return Task.FromResult<IActionResult?>(
                new BadRequestObjectResult(problemDetail));
        }
    }
}
