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

        // Endpoint para alterar senha
        group.MapPost("/change-password", async (
            [FromBody] ChangePasswordRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            try
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var command = new ChangePasswordCommand(
                    userId,
                    request.CurrentPassword,
                    request.NewPassword);

                await sender.Send(command);
                return Results.Ok(new { message = "Senha alterada com sucesso" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status400BadRequest);
            }
        })
        .RequireAuthorization()
        .WithName("ChangePassword")
        .WithSummary("Change user password")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // Endpoint para solicitar recuperação de senha
        group.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            IApplicationDbContext context,
            IEmailService emailService,
            CancellationToken cancellationToken) =>
        {
            // Buscar usuário pelo email
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            // Por segurança, sempre retornamos sucesso mesmo se o email não existir
            // Isso previne que atacantes descubram quais emails estão cadastrados
            if (user == null)
            {
                return Results.Ok(new { message = "Se o email existir, você receberá um código de recuperação." });
            }

            // Gerar token de 6 dígitos
            var resetToken = new Random().Next(100000, 999999).ToString();

            // Criar registro de token no banco
            var passwordResetToken = new Domain.Entities.PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = resetToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
                IsUsed = false
            };

            await context.PasswordResetTokens.AddAsync(passwordResetToken, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Enviar email com o token
            await emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return Results.Ok(new { message = "Se o email existir, você receberá um código de recuperação." });
        })
        .WithName("ForgotPassword")
        .WithSummary("Request a password reset token")
        .Produces(StatusCodes.Status200OK);

        // Endpoint para redefinir senha usando o token
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            CancellationToken cancellationToken) =>
        {
            // Buscar token válido
            var resetToken = await context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == request.Token &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (resetToken == null)
            {
                return Results.Json(
                    new { message = "Código de recuperação inválido ou expirado." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Atualizar senha do usuário
            resetToken.User.PasswordHash = passwordHasher.Hash(request.NewPassword);

            // Marcar token como usado
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Senha redefinida com sucesso!" });
        })
        .WithName("ResetPassword")
        .WithSummary("Reset password using a valid token")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}