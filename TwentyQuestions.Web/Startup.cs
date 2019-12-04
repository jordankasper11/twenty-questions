using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DbUp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configurationSettings = new ConfigurationSettings();

            configurationSettings.Database.ConnectionString = this.Configuration["Database_ConnectionString"] ?? "Server=localhost;Database=20Q;Trusted_Connection=True;";
            configurationSettings.Authentication.SecurityKey = this.Configuration["Authentication_SecurityKey"] ?? "20QDevSecurityKey";
            configurationSettings.Paths.Avatars = this.Configuration["Paths_Avatars"] ?? @"C:\Projects\TwentyQuestions\TwentyQuestions.Web\Storage\avatars";

            //throw new Exception(configurationSettings.Authentication.SecurityKey);

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

            services.AddMemoryCache();
            services.AddSingleton<ConfigurationSettings>(configurationSettings);
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddTransient<SqlConnection>(serviceProvider => new SqlConnection(configurationSettings.Database.ConnectionString));
            services.AddScoped<IRepositoryContext, RepositoryContext>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new AuthenticationRepository(sqlConnection, repositoryContext, configurationSettings.Authentication.SecurityKey);
            });
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IUserRepository, UserRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new UserRepository(sqlConnection, repositoryContext, configurationSettings.Paths.Avatars);
            });
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger, ConfigurationSettings configurationSettings)
        {
            applicationLifetime.ApplicationStarted.Register(() => UpdateDatabase(configurationSettings.Database.ConnectionString, logger));

            if (environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.ConfigureExceptionMiddleware();
            app.UseCors(c => c
                //.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(host => true)
            );
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

            // Redirect non-file 404 requests to Angular routing
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = "text/html";

                    await context.Response.SendFileAsync(Path.Combine(environment.WebRootPath, "index.html"));
                }
                else
                    await next();
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
