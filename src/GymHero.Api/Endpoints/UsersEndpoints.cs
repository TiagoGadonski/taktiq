using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        // Endpoint público para ver o perfil de um utilizador
        group.MapGet("/{userId:guid}/profile", async (Guid userId, IApplicationDbContext context) =>
        {
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return Results.NotFound();

            // Buscar os últimos 5 workouts completados do usuário
            var recentWorkouts = await context.WorkoutSessions
                .AsNoTracking()
                .Include(ws => ws.WorkoutPlan)
                .Where(ws => ws.OwnerId == userId && ws.CompletedAt != null)
                .OrderByDescending(ws => ws.CompletedAt)
                .Take(5)
                .Select(ws => new WorkoutSummary(
                    ws.Id,
                    ws.WorkoutPlan != null ? ws.WorkoutPlan.Name : "Free Workout",
                    ws.CompletedAt!.Value
                ))
                .ToListAsync();

            // Buscar os desafios completados do usuário (CurrentValue >= TargetValue)
            var completedChallenges = await context.ChallengeProgresses
                .AsNoTracking()
                .Include(cp => cp.Challenge)
                .Where(cp => cp.ParticipantId == userId && cp.CurrentValue >= cp.Challenge.TargetValue)
                .OrderByDescending(cp => cp.LastUpdate)
                .Select(cp => new CompletedChallengeDto(
                    cp.ChallengeId,
                    cp.Challenge.Title,
                    cp.Challenge.Type,
                    cp.Challenge.TargetValue,
                    cp.CurrentValue,
                    cp.LastUpdate
                ))
                .ToListAsync();

            var response = new PublicProfileResponse(
                user.Id,
                user.Name,
                user.Location,
                user.Bio,
                user.Email,
                user.ProfilePictureUrl,
                user.GymName,
                user.PhoneNumber,
                recentWorkouts,
                completedChallenges
            );
            return Results.Ok(response);
        });

        // Endpoint para admin alterar senha de usuário
        group.MapPut("/{userId:guid}/password", async (
            Guid userId,
            AdminChangePasswordRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            // Verificar se o usuário é Admin
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Admin")
            {
                return Results.Forbid();
            }

            // Buscar o usuário a ser alterado
            var targetUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (targetUser is null)
            {
                return Results.NotFound(new { message = "Usuário não encontrado" });
            }

            // Hash da nova senha
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Atualizar a senha
            targetUser.PasswordHash = passwordHash;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Senha alterada com sucesso" });
        }).RequireAuthorization();

        // POST /api/users/profile-picture - Upload profile picture
        group.MapPost("/profile-picture", async (
            IFormFile file,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IWebHostEnvironment env,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Validate file
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "Nenhum arquivo foi enviado." });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return Results.BadRequest(new { message = "O arquivo deve ter no máximo 5MB." });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Results.BadRequest(new { message = "Apenas arquivos JPG, PNG ou GIF são permitidos." });
            }

            // Generate unique filename
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var uploadsPath = Path.Combine(env.WebRootPath, "uploads", "profile-pictures");

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Update user profile picture URL
            var userEntity = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (userEntity == null)
            {
                return Results.NotFound(new { message = "Usuário não encontrado." });
            }

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(userEntity.ProfilePictureUrl))
            {
                var oldFileName = Path.GetFileName(userEntity.ProfilePictureUrl);
                var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            userEntity.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                profilePictureUrl = userEntity.ProfilePictureUrl,
                message = "Foto de perfil atualizada com sucesso."
            });
        })
        .RequireAuthorization()
        .WithName("UploadProfilePicture")
        .WithSummary("Upload user profile picture")
        .DisableAntiforgery(); // Required for file upload

        // PUT /api/users/profile - Update user profile
        group.MapPut("/profile", async (
            UpdatePTProfileRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var userEntity = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (userEntity == null)
            {
                return Results.NotFound(new { message = "Usuário não encontrado." });
            }

            // Update fields
            if (request.Name != null) userEntity.Name = request.Name;
            if (request.PhoneNumber != null) userEntity.PhoneNumber = request.PhoneNumber;
            if (request.Bio != null) userEntity.Bio = request.Bio;
            if (request.Location != null) userEntity.Location = request.Location;
            if (request.Specialization != null) userEntity.Specialization = request.Specialization;

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Perfil atualizado com sucesso." });
        })
        .RequireAuthorization()
        .WithName("UpdateProfile")
        .WithSummary("Update user profile");

        // PUT /api/users/notification-preferences - Update notification preferences
        group.MapPut("/notification-preferences", async (
            UpdateNotificationPreferencesRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // TODO: Store notification preferences in database
            // For now, just return success
            return Results.Ok(new { message = "Preferências de notificação atualizadas." });
        })
        .RequireAuthorization()
        .WithName("UpdateNotificationPreferences")
        .WithSummary("Update user notification preferences");

        // PUT /api/users/privacy-settings - Update privacy settings
        group.MapPut("/privacy-settings", async (
            UpdatePrivacySettingsRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // TODO: Store privacy settings in database
            // For now, just return success
            return Results.Ok(new { message = "Configurações de privacidade atualizadas." });
        })
        .RequireAuthorization()
        .WithName("UpdatePrivacySettings")
        .WithSummary("Update user privacy settings");
    }
}

public record AdminChangePasswordRequest(string NewPassword);

public record UpdatePTProfileRequest(
    string? Name,
    string? PhoneNumber,
    string? Bio,
    string? Location,
    string? Specialization
);

public record UpdateNotificationPreferencesRequest(
    bool EmailWorkoutCompleted,
    bool EmailNewClient,
    bool EmailPaymentReceived,
    bool PushWorkoutCompleted,
    bool PushNewClient,
    bool PushPaymentReceived
);

public record UpdatePrivacySettingsRequest(
    bool ProfilePublic,
    bool ShowEmail,
    bool ShowPhone,
    bool AllowMarketplaceListings
);