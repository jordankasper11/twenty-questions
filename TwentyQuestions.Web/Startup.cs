using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwentyQuestions.Web.Configuration;
using TwentyQuestions.Web.Extensions;
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

            this.Configuration.Bind(configurationSettings);

            configurationSettings.Validate();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddSingleton<ConfigurationSettings>(configurationSettings);            
            services.AddTransient<SqlConnection>(serviceProvider => new SqlConnection(configurationSettings.Database.ConnectionString));            
            services.AddSignalR();
            services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");
            services.AddCustomAuthentication(configurationSettings.Authentication.SecurityKey);
            services.AddCustomDependencies(configurationSettings);
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
