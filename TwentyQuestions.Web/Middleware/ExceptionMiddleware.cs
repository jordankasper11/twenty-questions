using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwentyQuestions.Web.Middleware
{
    public static class ExceptionMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }

        #endregion Methods
    }

    public class ExceptionMiddleware
    {
        #region Members

        private readonly IWebHostEnvironment _environment;
        private readonly RequestDelegate _next;

        #endregion Members

        #region Constructors

        public ExceptionMiddleware(IWebHostEnvironment environment, RequestDelegate next)
        {
            _environment = environment;
            _next = next;
        }

        #endregion Constructors

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (_environment.IsDevelopment())
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new StringBuilder();

                response.AppendLine(exception.Message);
                response.AppendLine(exception.StackTrace);

                await context.Response.WriteAsync(response.ToString());
            }
        }

        #endregion Methods
    }
}
