using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using GymHero.Application.Features.Auth.Commands;
using GymHero.Application.Features.Auth.Queries;
using GymHero.Application.Common.Interfaces;
using GymHero.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// Generates a cryptographically secure 6-digit code
    /// </summary>
    private static string GenerateSecureResetToken()
    {
        // Use cryptographically secure random number generator
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        // Endpoint de Cadastro (SignUp)
        group.MapPost("/signup", async (
            [FromBody] RegisterRequest request,
            ISender sender,
            ILogger<Program> logger) =>
        {
            try
            {
                var workoutLocation = (WorkoutLocation)request.PreferredWorkoutLocation;
                var command = new RegisterCommand(request.Name, request.Email, request.Password, workoutLocation);
                var result = await sender.Send(command);

                logger.LogInformation("New user registered: {Email}", request.Email);
                return Results.Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {
                logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during user registration");
                return Results.Problem("Erro ao criar conta. Tente novamente.");
            }
        })
        .WithName("SignUp")
        .WithSummary("Register a new user")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Endpoint de Login with rate limiting
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            ISender sender,
            ILogger<Program> logger) =>
        {
            try
            {
                var query = new LoginQuery(request.Email, request.Password);
                var result = await sender.Send(query);

                logger.LogInformation("Successful login for user: {Email}", request.Email);
                return Results.Ok(result);
            }
            catch (AuthenticationException ex)
            {
                logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return Results.Problem("Erro ao fazer login. Tente novamente.");
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
                location = dbUser.Location,
                preferredWorkoutLocation = (int)dbUser.PreferredWorkoutLocation
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
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Buscar usuário pelo email
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                // Por segurança, sempre retornamos sucesso mesmo se o email não existir
                // Isso previne que atacantes descubram quais emails estão cadastrados
                if (user == null)
                {
                    logger.LogWarning("Password reset requested for non-existent email");
                    return Results.Ok(new { message = "Se o email existir, você receberá um código de recuperação." });
                }

                // Gerar token seguro de 6 dígitos usando criptografia
                var resetToken = GenerateSecureResetToken();

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

                logger.LogInformation("Password reset token sent to user {Email}", user.Email);
                return Results.Ok(new { message = "Se o email existir, você receberá um código de recuperação." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing forgot password request");
                return Results.Ok(new { message = "Se o email existir, você receberá um código de recuperação." });
            }
        })
        .WithName("ForgotPassword")
        .WithSummary("Request a password reset token")
        .Produces(StatusCodes.Status200OK);

        // Endpoint para redefinir senha usando o token
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
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
                    logger.LogWarning("Invalid or expired password reset token attempted");
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

                logger.LogInformation("Password successfully reset for user {UserId}", resetToken.UserId);
                return Results.Ok(new { message = "Senha redefinida com sucesso!" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error resetting password");
                return Results.Problem("Erro ao redefinir senha. Tente novamente.");
            }
        })
        .WithName("ResetPassword")
        .WithSummary("Reset password using a valid token")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Endpoint para ativar conta através de convite de personal trainer
        group.MapPost("/activate", async (
            [FromBody] ActivateAccountRequest request,
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            CancellationToken cancellationToken) =>
        {
            // Buscar convite válido
            var invitation = await context.StudentInvitations
                .Include(i => i.Trainer)
                .Include(i => i.WorkoutPlan)
                .FirstOrDefaultAsync(i =>
                    i.ActivationToken == request.Token &&
                    i.Status == "Pending" &&
                    i.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (invitation == null)
            {
                return Results.Json(
                    new { message = "Convite inválido ou expirado." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Verificar se o email já está em uso
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == invitation.StudentEmail, cancellationToken);

            if (existingUser != null)
            {
                return Results.Json(
                    new { message = "Este email já está em uso." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Criar novo usuário
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = invitation.StudentEmail,
                PasswordHash = passwordHasher.Hash(request.Password),
                Role = "Aluno",
                PersonalTrainerId = invitation.TrainerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PreferredWorkoutLocation = (WorkoutLocation)request.PreferredWorkoutLocation
            };

            await context.Users.AddAsync(newUser, cancellationToken);

            // Atualizar convite como ativado
            invitation.Status = "Activated";
            invitation.ActivatedAt = DateTime.UtcNow;
            invitation.CreatedUserId = newUser.Id;

            // Se houver plano de treino associado, copiar para o novo usuário
            if (invitation.WorkoutPlanId.HasValue && invitation.WorkoutPlan != null)
            {
                var workoutPlanCopy = new WorkoutPlan
                {
                    Id = Guid.NewGuid(),
                    Name = invitation.WorkoutPlan.Name,
                    Goal = invitation.WorkoutPlan.Goal,
                    Duration = invitation.WorkoutPlan.Duration,
                    OwnerId = newUser.Id,
                    IsActive = true
                };

                await context.WorkoutPlans.AddAsync(workoutPlanCopy, cancellationToken);

                // Copy workouts if needed
                var workouts = await context.Workouts
                    .Include(w => w.Exercises)
                    .Where(w => w.PlanId == invitation.WorkoutPlanId)
                    .ToListAsync(cancellationToken);

                foreach (var workout in workouts)
                {
                    var workoutCopy = new Workout
                    {
                        Id = Guid.NewGuid(),
                        Name = workout.Name,
                        DayOfWeek = workout.DayOfWeek,
                        PlanId = workoutPlanCopy.Id
                    };

                    await context.Workouts.AddAsync(workoutCopy, cancellationToken);

                    // Copy exercises
                    foreach (var exercise in workout.Exercises)
                    {
                        var exerciseCopy = new WorkoutExercise
                        {
                            Id = Guid.NewGuid(),
                            WorkoutId = workoutCopy.Id,
                            ExerciseId = exercise.ExerciseId,
                            TargetSets = exercise.TargetSets,
                            TargetReps = exercise.TargetReps,
                            RestSeconds = exercise.RestSeconds,
                            Notes = exercise.Notes
                        };

                        await context.WorkoutExercises.AddAsync(exerciseCopy, cancellationToken);
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            // Gerar token JWT para login automático
            var token = jwtTokenGenerator.GenerateToken(newUser);

            return Results.Ok(new AuthResponse(
                newUser.Id,
                newUser.Name,
                newUser.Email,
                token,
                newUser.Role,
                newUser.ProfilePictureUrl
            ));
        })
        .WithName("ActivateAccount")
        .WithSummary("Activate account through personal trainer invitation")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}