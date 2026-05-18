using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;
using Users.Api.Helpers;
using Users.Application.Validations;

var builder = WebApplication.CreateBuilder(args);

#region Configure Serilog

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddCustomDI(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
//builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
//builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();

#region Swagger with JWT support

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "1.0.0",
        Description = ".NET 9 Web API with JWT auth"
    });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
