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
                currentUser.CreatedAt
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

            await context.SaveChangesAsync(ct);
            return Results.Ok();
        });

        // Endpoint para atualizar a foto de perfil
        group.MapPost("/profile-picture", async (IFormFile file, ClaimsPrincipal user, IApplicationDbContext context, CancellationToken ct) =>
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

            // Criar diretório de uploads se não existir
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);

            // Gerar nome único para o arquivo
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Salvar o arquivo
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, ct);
            }

            // Deletar a foto antiga se existir
            if (!string.IsNullOrEmpty(userToUpdate.ProfilePictureUrl))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userToUpdate.ProfilePictureUrl.TrimStart('/'));
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            // Atualizar o URL da foto no banco
            userToUpdate.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
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