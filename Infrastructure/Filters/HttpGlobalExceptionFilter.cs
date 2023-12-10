using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Afstest.API.SeedWork;
using Afstest.API.Infrastructure.ActionResults;

namespace Afstest.API.Infrastructure.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        readonly IWebHostEnvironment _env;
        readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);

            if (context.Exception is AfstestPlatformException exception)
            {
                var statusCode = exception.CustomStatusCode != null ? exception.CustomStatusCode : StatusCodes.Status400BadRequest;
                var problemDetails = new
                {
                    Title = exception.Message,
                    Instance = context.HttpContext.Request.Path,
                    Status = statusCode,
                    Detail = "Please refer to the errors property for additional details.",
                    Errors = exception.Errors.ToArray()
                };

                context.Result = new ObjectResult(problemDetails) { StatusCode = statusCode };
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error occured.Try it again." }
                };

                if (_env.IsDevelopment())
                {
                    json.DeveloperMessage = new
                    {
                        ClassName = context.Exception.GetType().FullName,
                        context.Exception.Message,
                        context.Exception.Data,
                        InnerException = context.Exception.InnerException?.Message,
                        context.Exception.StackTrace,
                        context.Exception.Source,
                        context.Exception.HResult
                    };
                }

                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }

    public record JsonErrorResponse
    {
        public string[] Messages { get; set; } = default!;

        public object DeveloperMessage { get; set; } = new object();
    }
}
