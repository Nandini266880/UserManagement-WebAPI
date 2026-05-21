using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Users.API.Helpers
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                if (description.GroupName.Equals("v1"))
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = "User Management API",
                        Version = description.ApiVersion.ToString(),
                        Description = ".NET 9 Web API with JWT auth"
                    });
                }
                else if (description.GroupName.Equals("v2"))
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = "User Management API",
                        Version = description.ApiVersion.ToString(),
                        Description = ".NET 9 Web API - User Friendly with Pagination and Sorting"
                    });
                }
            }


        }
    }
}
