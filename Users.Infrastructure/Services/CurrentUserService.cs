using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Reads the current user's email claim from the HTTP context.
        /// </summary>
        /// <returns>User email if present; otherwise an empty string.</returns>
        public string GetEmail()
        {
            return _httpContextAccessor?
                        .HttpContext?
                        .User?
                        .FindFirst(ClaimTypes.Email)?
                        .Value ?? string.Empty;
        }
    }
}
