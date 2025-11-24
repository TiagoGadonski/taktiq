using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs; // Garanta que este using está correto
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        // --- Endpoint GET /me ATUALIZADO ---
        group.MapGet("/", async (ClaimsPrincipal user, IApplicationDbContext context, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (currentUser is null) return Results.NotFound();
            
            // Mapeamos a entidade User completa para o DTO de resposta
            var response = new UserProfileResponse(
                currentUser.Id,
                currentUser.Name,
                currentUser.Email,
                currentUser.Role,
                currentUser.DateOfBirth,
                currentUser.Location,
                currentUser.Bio,
                currentUser.Height,
                currentUser.Weight,
                currentUser.ProfilePictureUrl,
                currentUser.GymName,
                currentUser.PhoneNumber,
                currentUser.Injuries,
                currentUser.HealthConditions,
                currentUser.ExerciseGoal,
                currentUser.TrainingSplit,
                (int)currentUser.PreferredWorkoutLocation,
                currentUser.CreatedAt,
                // Personal Trainer Profile Fields
                currentUser.ProfileSlug,
                currentUser.CoverPhotoUrl,
                currentUser.Specialization,
                currentUser.Education,
                currentUser.Experience,
                currentUser.PricingInfo,
                currentUser.Philosophy,
                currentUser.Methodology,
                currentUser.VideoIntroUrl,
                currentUser.YearsExperience,
                currentUser.ClientsCount,
                currentUser.SuccessStoriesCount,
                currentUser.IsPublicProfile,
                currentUser.InstagramUrl,
                currentUser.FacebookUrl,
                currentUser.WebsiteUrl
            );
            return Results.Ok(response);
        });
        
        // --- Endpoint PUT /me ATUALIZADO ---
        group.MapPut("/", async (UpdateProfileRequest request, ClaimsPrincipal user, IApplicationDbContext context, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userToUpdate = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (userToUpdate is null) return Results.NotFound();

            // Atualizamos todos os campos com os dados recebidos
            userToUpdate.Name = request.Name;
            userToUpdate.Email = request.Email;
            userToUpdate.DateOfBirth = request.DateOfBirth.HasValue
        ? DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Utc)
        : null;
            userToUpdate.Location = request.Location;
            userToUpdate.Bio = request.Bio;
            userToUpdate.Height = request.Height;
            userToUpdate.Weight = request.Weight;
            userToUpdate.GymName = request.GymName;
            userToUpdate.PhoneNumber = request.PhoneNumber;
            userToUpdate.Injuries = request.Injuries;
            userToUpdate.HealthConditions = request.HealthConditions;
            userToUpdate.ExerciseGoal = request.ExerciseGoal;
            userToUpdate.TrainingSplit = request.TrainingSplit;

            // Update workout location preference if provided
            if (request.PreferredWorkoutLocation.HasValue)
            {
                userToUpdate.PreferredWorkoutLocation = (GymHero.Domain.Enums.WorkoutLocation)request.PreferredWorkoutLocation.Value;
            }

            await context.SaveChangesAsync(ct);
            return Results.Ok();
        });

        // Endpoint para atualizar a foto de perfil
        group.MapPost("/profile-picture", async (
            IFormFile file,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IBlobStorageService blobStorage,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userToUpdate = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (userToUpdate is null) return Results.NotFound();

            // SECURITY: Multi-layer validation
            // 1. File size validation
            if (file.Length > 5 * 1024 * 1024) // 5MB
            {
                return Results.BadRequest(new { message = "A imagem deve ter no máximo 5MB" });
            }

            // 2. Extension validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return Results.BadRequest(new { message = "Apenas imagens são permitidas (jpg, jpeg, png, gif)" });
            }

            // 3. MIME type validation
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var mimeType = file.ContentType?.ToLowerInvariant();

            if (string.IsNullOrEmpty(mimeType) || !allowedMimeTypes.Contains(mimeType))
            {
                return Results.BadRequest(new { message = "Tipo de arquivo inválido. Use apenas imagens." });
            }

            // 4. File signature (magic numbers) validation
            using var stream = file.OpenReadStream();
            var header = new byte[8];
            await stream.ReadAsync(header, 0, 8, ct);
            stream.Position = 0;

            var isValidImage = false;
            // Check JPEG signature (FF D8 FF)
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                isValidImage = true;
            // Check PNG signature (89 50 4E 47 0D 0A 1A 0A)
            else if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                isValidImage = true;
            // Check GIF signature (47 49 46 38)
            else if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                isValidImage = true;

            if (!isValidImage)
            {
                return Results.BadRequest(new { message = "Arquivo não é uma imagem válida" });
            }

            // Generate unique file name
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";

            // Upload to Azure Blob Storage
            var blobUrl = await blobStorage.UploadAsync(
                stream,
                fileName,
                file.ContentType ?? "application/octet-stream",
                "profile-pictures",
                ct);

            // Delete old profile picture from blob storage if exists
            if (!string.IsNullOrEmpty(userToUpdate.ProfilePictureUrl) &&
                userToUpdate.ProfilePictureUrl.Contains("blob.core.windows.net"))
            {
                try
                {
                    await blobStorage.DeleteAsync(
                        userToUpdate.ProfilePictureUrl,
                        "profile-pictures",
                        ct);
                }
                catch
                {
                    // Ignore errors when deleting old blob (might not exist)
                }
            }

            // Update the photo URL in the database
            userToUpdate.ProfilePictureUrl = blobUrl;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { profilePictureUrl = userToUpdate.ProfilePictureUrl });
        }).DisableAntiforgery();

        // Endpoint existente para buscar as medalhas (não precisa de alteração)
        group.MapGet("/badges", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new Application.Features.Badges.Queries.GetUserBadgesQuery(userId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });
    }
}