using Asp.Versioning.ApiExplorer;
using FluentValidation;
using Microsoft.Extensions.Options;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Enums;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.Api.Helpers;
using Users.API.Helpers;
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
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

//Scanning assembly once - adds all validators to models
builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableBuiltInModelValidation = true;
    config.ValidationStrategy = ValidationStrategy.All;
    config.OverrideDefaultResultFactoryWith<CustomFluentValidationResultFactory>();
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Dynamically build Swagger endpoints for every registered API version
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"Users API {description.GroupName.ToUpperInvariant()}"
            );
        }
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
