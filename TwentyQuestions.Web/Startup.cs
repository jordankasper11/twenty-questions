using DbUp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwentyQuestions.Data.Caching;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Caching;
using TwentyQuestions.Web.Configuration;
using TwentyQuestions.Web.Middleware;
using TwentyQuestions.Web.SignalR;

namespace TwentyQuestions.Web
{
    public class Startup
    {
        private IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configurationSettings = new ConfigurationSettings();

            configurationSettings.Database.ConnectionString = this.Configuration["Database_ConnectionString"] ?? "Server=localhost;Database=20Q;Trusted_Connection=True;";
            configurationSettings.Authentication.SecurityKey = this.Configuration["Authentication_SecurityKey"] ?? "20QDevSecurityKey";
            configurationSettings.Paths.Avatars = this.Configuration["Paths_Avatars"] ?? @"C:\Projects\TwentyQuestions\TwentyQuestions.Web\Storage\avatars";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationSettings.Authentication.SecurityKey))
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

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddSingleton<ConfigurationSettings>(configurationSettings);
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddTransient<SqlConnection>(serviceProvider => new SqlConnection(configurationSettings.Database.ConnectionString));
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
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserRepository, UserRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new UserRepository(sqlConnection, repositoryContext, configurationSettings.Paths.Avatars);
            });
            services.AddSignalR();
            services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger, ConfigurationSettings configurationSettings)
        {
            applicationLifetime.ApplicationStarted.Register(() => UpdateDatabase(configurationSettings.Database.ConnectionString, logger));

            if (environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.ConfigureExceptionMiddleware();

            if (environment.IsDevelopment())
            {
                app.UseCors(c => c
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(host => true)
                );
            }
            else
                app.UseCors();

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(configurationSettings.Paths.Avatars),
                RequestPath = new PathString("/avatars")
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapHub<NotificationHub>("/hubs/notifications"));
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseStatusCodePagesWithReExecute("/");

            var contentTypeProvider = new FileExtensionContentTypeProvider();

            contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = contentTypeProvider
            });

            app.UseSpa(builder =>
            {
                if (environment.IsDevelopment())
                    builder.UseProxyToSpaDevelopmentServer($"http://localhost:4200/");
                else
                    builder.Options.SourcePath = "wwwroot";
            });
        }

        private void UpdateDatabase(string connectionString, ILogger<Startup> logger)
        {
            logger.LogInformation("Checking database status");

            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), scriptName => scriptName.StartsWith("TwentyQuestions.Web.Scripts."))
                .WithTransaction()
                .JournalToSqlTable("dbo", "DbUp")
                .LogToConsole()
                .Build();

            if (upgradeEngine.IsUpgradeRequired())
            {
                logger.LogInformation("Database changes detected");

                var upgradeResult = upgradeEngine.PerformUpgrade();

                if (upgradeResult.Successful)
                {
                    logger.LogInformation("Database updated successfully");

                    foreach (var script in upgradeResult.Scripts)
                        logger.LogInformation($"Executed script {script.Name}");
                }
                else
                    logger.LogError(upgradeResult.Error, "Error updating database");
            }
            else
                logger.LogInformation("No database changes detected");
        }
    }
}
