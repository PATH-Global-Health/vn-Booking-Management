using System;
using System.Text;
using Data.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            services.AddScoped<IProducerMQ, Producer>();
            services.AddScoped<IBookingProducer, BookingProducer>();

            services.AddHostedService<BookingUpdateStatusConsumer>();
            services.AddHostedService<BookingUpdateResultConsumer>();
        }
    }
}
