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
        // 1. Registar o erro nos nossos logs
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Preparar uma resposta de erro padronizada
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred on the server."
        };

        // 3. Enviar a resposta de erro para o utilizador
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Retornamos 'true' para indicar que o erro foi tratado.
        return true;
    }
}