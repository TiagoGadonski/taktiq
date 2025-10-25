using System.Security.Authentication;
using System.Security.Claims;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Auth.Commands;
using GymHero.Application.Features.Auth.Queries; // Adicionar este using
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        // Endpoint de Cadastro (SignUp)
        group.MapPost("/signup", async (
            [FromBody] RegisterRequest request,
            ISender sender) =>
        {
            try
            {
                var command = new RegisterCommand(request.Name, request.Email, request.Password);
                var result = await sender.Send(command);
                return Results.Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status400BadRequest);
            }
        })
        .WithName("SignUp")
        .WithSummary("Register a new user")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Endpoint de Login with rate limiting
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            ISender sender) =>
        {
            try
            {
                var query = new LoginQuery(request.Email, request.Password);
                var result = await sender.Send(query);
                return Results.Ok(result);
            }
            catch (AuthenticationException ex)
            {
                // Se a autenticação falhar, retornamos um resultado 401 Unauthorized
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .RequireRateLimiting("auth") // Apply rate limiting to prevent brute force
        .WithName("Login")
        .WithSummary("Authenticate a user and get a JWT")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // Endpoint para obter dados do usuário atual
        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Results.Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var dbUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (dbUser == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                id = dbUser.Id,
                name = dbUser.Name,
                email = dbUser.Email,
                role = dbUser.Role,
                profilePictureUrl = dbUser.ProfilePictureUrl,
                bio = dbUser.Bio,
                location = dbUser.Location
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user information including role")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}