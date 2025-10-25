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
                .Where(ws => ws.WorkoutPlan.OwnerId == userId && ws.CompletedAt != null)
                .OrderByDescending(ws => ws.CompletedAt)
                .Take(5)
                .Select(ws => new WorkoutSummary(
                    ws.Id,
                    ws.WorkoutPlan.Name,
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
    }
}

public record AdminChangePasswordRequest(string NewPassword);