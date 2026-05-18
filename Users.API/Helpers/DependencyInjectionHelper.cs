using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Users.API.Helpers;
using Users.Application.Models;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.JwtServices;
using Users.Infrastructure.Repository;
using Users.Infrastructure.Services;

namespace Users.Api.Helpers
{
    public static class DependencyInjectionHelper
    {
        public static IServiceCollection AddCustomDI(this IServiceCollection services, IConfiguration configuration)
        {
            #region DB Connection
            var conString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conString));
            #endregion

            #region Dependency Injections

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();

            #endregion

            #region JWT Authentication

            var jwtSettings = configuration.GetSection("JwtConfig");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = jwtSettings["Audience"],
                    ValidIssuer = jwtSettings["Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                };
            });
            services.AddAuthorization();
            #endregion

            #region Override [ApiController]'s built in 400 response factory

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.Errors?.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors
                            .Select(e => new FieldError
                            {
                                Field = kvp.Key,
                                Message = string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage,
                            })
                        ).ToList();

                    var problemDetail = new ProblemDetail
                    {
                        Title = "Validation errors",
                        Status = StatusCodes.Status400BadRequest,
                        Instance = context.HttpContext.Request.Path,
                        Errors = errors
                    };

                    return new BadRequestObjectResult(problemDetail);
                };
            });

            #endregion

            #region Global Exception Handling

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            #endregion

            return services;
        }
    }
}
