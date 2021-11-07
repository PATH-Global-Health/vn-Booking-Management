using System;
using System.Linq;
using System.Text;
using Data.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Services;
using Services.Core;
using Services.RabbitMQ;

namespace Booking_Service_App.Extensions
{
    public static class StartupExtensions
    {
        public static void ConfigCors(this IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
        }

        public static void ConfigMongoDb(this IServiceCollection services, string connectionString, string databaseName)
        {
            services.AddSingleton<IMongoClient>(s => new MongoClient(connectionString));
            services.AddScoped(s => new ApplicationDbContext(s.GetRequiredService<IMongoClient>(), databaseName));
            
        }

        public static void ConfigJwt(this IServiceCollection services, string key, string issuer, string audience)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(jwtconfig =>
                {
                    jwtconfig.SaveToken = true;
                    jwtconfig.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false,
                        RequireSignedTokens = true,
                        ValidIssuer = issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidAudience = string.IsNullOrEmpty(audience) ? issuer : audience,
                    };

                });
        }

        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IExaminationService, ExaminationService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IProducerMQ, Producer>();
            services.AddScoped<IProducerCheckExternalId, ProducerCheckExternalId>();
            services.AddScoped<IProducerAddReferTicket, ProducerAddReferTicket>();
            services.AddScoped<IProducerSetInterval, ProducerSetInterval>();
            services.AddScoped<IProducerSetStatusProfile, ProducerSetStatusProfile>();
            services.AddScoped<ITestingHistoryService, TestingHistoryService>();
            services.AddScoped<IPrEPService, PrEPService>();
            services.AddScoped<IARTService, ARTService>();
            services.AddScoped<IWorkingSessionService, WorkingSessionService>();

        }

        public static void ConfigValidationProblem(this IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    return new BadRequestObjectResult(new
                    {
                        StatusCode = 400,
                        Message = context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage),
                        Error = "Bad request"
                    });

                };
            });

        }
    }
}
