using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Users.API.Helpers;
using Users.Application.Interfaces;
using Users.Application.Models;
using Users.Application.Services;
using Users.Infrastructure.Data;
using Users.Infrastructure.JwtServices;
using Users.Infrastructure.Repository;
using Users.Infrastructure.Services;

namespace Users.Api.Helpers
{
    public static class DependencyInjectionHelper
    {
        /// <summary>
        /// Customized Dependency Injection helper for program.cs
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomDI(this IServiceCollection services, IConfiguration configuration)
        {
            #region DB Connection
            var conString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conString));
            #endregion

            #region Dependency Injections

            services.AddScoped<CustomFluentValidationResultFactory>();
            services.AddScoped<ValidateGuidFilter>();
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

            #region API Version

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            #endregion

            #region Swagger Configurations 

            services.AddSwaggerGen(options =>
            {
                // Adding Xml Comments 
                var presentationXml = Path.Combine(AppContext.BaseDirectory, "Users.API.xml");
                if(File.Exists(presentationXml))
                    options.IncludeXmlComments(presentationXml);

                var applicationXml = Path.Combine(AppContext.BaseDirectory, "Users.Application.xml");
                if(File.Exists(applicationXml))
                    options.IncludeXmlComments(applicationXml);

                // JWT configuration 
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your-token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

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
                        );

                    var problemDetail = ValidationErrorHelper.BuildProblemDetail(errors, context.HttpContext.Request.Path);

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
