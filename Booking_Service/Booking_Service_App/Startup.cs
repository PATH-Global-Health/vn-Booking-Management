using AutoMapper;
using Booking_Service_App.Extensions;
using Data.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.MappingProfile;

namespace Booking_Service_App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.ConfigMongoDb(Configuration["AppDatabaseSettings:ConnectionString"], Configuration["AppDatabaseSettings:DatabaseName"]);
            services.AddBusinessServices();
            services.ConfigJwt(Configuration["JwtSecretKey"], Configuration["JwtIssuerOptions:Issuer"], Configuration["JwtIssuerOptions:Audience"]);
            services.ConfigCors();
            services.ConfigSwagger();
            services.AddAutoMapper(typeof(MapperProfile));
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            dbContext.CreateCollectionsIfNotExists();

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseOpenApi();

            app.UseSwaggerUi3();
        }
    }
}
