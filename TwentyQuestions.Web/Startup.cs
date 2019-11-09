using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TwentyQuestions.Data.Caching;
using TwentyQuestions.Data.Repositories;
using TwentyQuestions.Web.Caching;
using TwentyQuestions.Web.Configuration;
using TwentyQuestions.Web.Middleware;

namespace TwentyQuestions.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var configurationSettings = this.Configuration.GetSection("ConfigurationSettings")
                .Get<ConfigurationSettings>();

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
                });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddMemoryCache();
            services.AddSingleton<ConfigurationSettings>(configurationSettings);
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddTransient<SqlConnection>(serviceProvider => new SqlConnection(configurationSettings.Database.ConnectionString));
            services.AddScoped<IRepositoryContext, RepositoryContext>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>(serviceProvider =>
            {
                var sqlConnection = serviceProvider.GetService<SqlConnection>();
                var repositoryContext = serviceProvider.GetService<IRepositoryContext>();

                return new AuthenticationRepository(sqlConnection, repositoryContext, configurationSettings.Authentication.SecurityKey);
            });
            services.AddScoped<IGameRepository, GameRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var configurationSettings = app.ApplicationServices.GetService<ConfigurationSettings>();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.ConfigureExceptionMiddleware();
            app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
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

                    await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, "index.html"));
                }
                else
                    await next();
            });
        }
    }
}
