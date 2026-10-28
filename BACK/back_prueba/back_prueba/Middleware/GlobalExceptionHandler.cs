using back_prueba.Dtos;
using back_prueba.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace back_prueba.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            ErrorResponse error;

            if (exception is AppException appException)
            {
                error = new ErrorResponse(appException.StatusCode, appException.Message, appException.Errors);
            }
            else
            {
                logger.LogError(exception, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
                error = new ErrorResponse(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado. Intente de nuevo más tarde.");
            }

            context.Response.StatusCode = error.Status;
            await context.Response.WriteAsJsonAsync(error, ct);
            return true;
        }
    }
}
