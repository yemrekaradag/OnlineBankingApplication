using OnlineBankingApplication.Exceptions;
using System.Net;

namespace OnlineBankingApplication.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;

            switch (exception)
            {
                case NotFoundException _:
                    status = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                case BadRequestException _:
                    status = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                default:
                    status = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(new
            {
                error = message
            }.ToString());
        }
    }
}
