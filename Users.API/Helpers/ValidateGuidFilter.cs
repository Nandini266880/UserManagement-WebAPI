using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Users.Application.Models;

namespace Users.API.Helpers
{
    public class ValidateGuidFilter : IActionFilter
    {
        private readonly ILogger<ValidateGuidFilter> _logger;
        public ValidateGuidFilter(ILogger<ValidateGuidFilter> logger)
        {
            _logger = logger;
        }

        public void  OnActionExecuting(ActionExecutingContext context)
        {
            var emptyGuids = context.ActionArguments
                                        .Where(arg => arg.Value is Guid guid && guid == Guid.Empty)
                                        .Select(arg => arg.Key)
                                        .ToList();

            if(emptyGuids.Count > 0)
            {
                _logger.LogWarning("ValidateGuidFilter: Empty Guid detected for {Parameters} at {Path}",
                        string.Join(", ", emptyGuids),
                        context.HttpContext.Request.Path
                    );

                var errors = emptyGuids.Select(field => new FieldError
                {
                    Field = field,
                    Message = $"{field} cannot be Guid.Empty"
                }).ToList();

                var problemDetail = ValidationErrorHelper.BuildProblemDetail(errors, context.HttpContext.Request.Path);

                context.Result = new BadRequestObjectResult(problemDetail);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
