using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;
using Users.Application.Exceptions;
using Users.Application.Models;

namespace Users.API.Helpers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger; // add this

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger; 
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception
            , CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled Exception at {Path}", httpContext.Request.Path);

            var problemDetails = new ProblemDetails();
            problemDetails.Instance = httpContext.Request.Path;

            if(exception is ValidationException validationException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                var problemDetail = new ProblemDetail
                {
                    Title = "Validation errors",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = httpContext.Request.Path,

                    Errors = validationException.Errors
                        .Select(e => new FieldError
                        {
                            Field = e.PropertyName,
                            Message = e.ErrorMessage
                        })
                        .ToList()
                };

                await httpContext.Response.WriteAsJsonAsync(problemDetail, cancellationToken);
                return true;
            }
            else if(exception is NotFoundException)
            {
                problemDetails.Title = exception.Message;
                problemDetails.Status = StatusCodes.Status404NotFound;
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            else if(exception is ConflictException)
            {
                problemDetails.Title = exception.Message;
                problemDetails.Status = StatusCodes.Status409Conflict;
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            }
            else
            {
                problemDetails.Title = "An unexpected error occurred!";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = exception.Message;
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
