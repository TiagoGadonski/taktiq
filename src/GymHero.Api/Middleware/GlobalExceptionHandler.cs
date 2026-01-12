using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Middleware;

// Esta classe será responsável por apanhar qualquer exceção que não tenha sido tratada
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Registar o erro nos nossos logs com detalhes completos
        logger.LogError(exception,
            "An unhandled exception occurred: {Message} | Path: {Path} | Method: {Method} | StatusCode: {StatusCode} | StackTrace: {StackTrace}",
            exception.Message,
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.Response.StatusCode,
            exception.StackTrace);

        // 2. Preparar uma resposta de erro padronizada
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred on the server."
        };

        // 3. IMPORTANT: Do NOT clear or overwrite response headers
        // CORS headers have already been set by CORS middleware
        // We only set the status code and write the body
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        // Retornamos 'true' para indicar que o erro foi tratado.
        return true;
    }
}