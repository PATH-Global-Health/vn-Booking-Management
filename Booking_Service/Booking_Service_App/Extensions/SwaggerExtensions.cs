using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Linq;

namespace Booking_Service_App.Extensions
{
    public static class SwaggerExtensions
    {
        public static void ConfigSwagger(this IServiceCollection services)
        {
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Booking Services";
                document.Version = "2.0";
                document.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                document.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
        }
    }
}
