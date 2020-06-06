using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwentyQuestions.Data.Caching;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Caching;
using TwentyQuestions.Web.Configuration;
using TwentyQuestions.Web.SignalR;

namespace TwentyQuestions.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, string securityKey)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = false,
                       ValidateAudience = false,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey))
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           if (context.HttpContext.Request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                           {
                               var accessToken = context.Request.Query["access_token"];

                               if (!String.IsNullOrWhiteSpace(accessToken))
                                   context.Token = accessToken;
                           }

                           return Task.CompletedTask;
                       }
                   };
               });

            return services;
        }

        public static IServiceCollection AddCustomDependencies(this IServiceCollection services, ConfigurationSettings configurationSettings)
        {
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<IRepositoryContext, RepositoryContext>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var user = httpContextAccessor.HttpContext.User;
                var repositoryContext = new RepositoryContext();

                repositoryContext.UserId = Guid.TryParse(user.FindFirst("userId")?.Value, out Guid userId) ? userId : (Guid?)null;

                return repositoryContext;
            });

            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new AuthenticationRepository(sqlConnection, repositoryContext, configurationSettings.Authentication.SecurityKey);
            });
                        
            services.AddScoped<IUserRepository, UserRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new UserRepository(sqlConnection, repositoryContext, configurationSettings.Paths.Avatars);
            });

            return services;
        }
    }
}
