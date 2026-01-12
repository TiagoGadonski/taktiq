using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class ProgressPhotoEndpoints
{
    public static void MapProgressPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress-photos")
            .WithTags("Progress Photos")
            .RequireAuthorization();

        // POST /api/progress-photos - Create a progress photo
        group.MapPost("/", async (
            [FromBody] CreateProgressPhotoRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            // Verify the media exists and belongs to the uploader
            var media = await context.Medias
                .FirstOrDefaultAsync(m => m.Id == request.MediaId && !m.IsDeleted, cancellationToken);

            if (media == null)
            {
                return Results.NotFound(new { message = "Mídia não encontrada" });
            }

            // Verify authorization
            // Trainer can add photos for their students, student can add for themselves
            if (userRole == "PersonalTrainer")
            {
                // Verify student belongs to this trainer
                var student = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.StudentId && u.PersonalTrainerId == userId, cancellationToken);

                if (student == null)
                {
                    return Results.Problem(
                        title: "Acesso negado",
                        detail: "Este aluno não pertence a você",
                        statusCode: 403
                    );
                }
            }
            else
            {
                // Student can only add photos for themselves
                if (request.StudentId != userId)
                {
                    return Results.Problem(
                        title: "Acesso negado",
                        detail: "Você só pode adicionar fotos para si mesmo",
                        statusCode: 403
                    );
                }
            }

            var progressPhoto = new ProgressPhoto
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                MediaId = request.MediaId,
                UploadedBy = userId,
                PhotoType = request.PhotoType,
                BodyAngle = request.BodyAngle,
                PhotoDate = request.PhotoDate,
                WeightKg = request.WeightKg,
                BodyFatPercentage = request.BodyFatPercentage,
                MuscleMassKg = request.MuscleMassKg,
                ChestCm = request.ChestCm,
                WaistCm = request.WaistCm,
                HipsCm = request.HipsCm,
                LeftArmCm = request.LeftArmCm,
                RightArmCm = request.RightArmCm,
                LeftThighCm = request.LeftThighCm,
                RightThighCm = request.RightThighCm,
                LeftCalfCm = request.LeftCalfCm,
                RightCalfCm = request.RightCalfCm,
                TrainerNotes = request.TrainerNotes,
                StudentNotes = request.StudentNotes,
                Caption = request.Caption,
                IsVisibleToStudent = request.IsVisibleToStudent,
                IsPublic = request.IsPublic,
                StudentAssessmentId = request.StudentAssessmentId,
                CreatedAt = DateTime.UtcNow
            };

            await context.ProgressPhotos.AddAsync(progressPhoto, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/progress-photos/{progressPhoto.Id}", new { id = progressPhoto.Id });
        })
        .WithName("CreateProgressPhoto")
        .WithSummary("Create a new progress photo");

        // GET /api/progress-photos/student/{studentId} - Get all photos for a student
        group.MapGet("/student/{studentId:guid}", async (
            Guid studentId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] ProgressPhotoType? photoType,
            [FromQuery] BodyAngle? bodyAngle,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            // Verify authorization
            if (userRole == "PersonalTrainer")
            {
                // Verify student belongs to this trainer
                var studentBelongsToTrainer = await context.Users
                    .AnyAsync(u => u.Id == studentId && u.PersonalTrainerId == userId, cancellationToken);

                if (!studentBelongsToTrainer)
                {
                    return Results.Problem(
                        title: "Acesso negado",
                        detail: "Este aluno não pertence a você",
                        statusCode: 403
                    );
                }
            }
            else
            {
                // Student can only view their own photos
                if (studentId != userId)
                {
                    return Results.Forbid();
                }
            }

            var query = context.ProgressPhotos
                .Include(p => p.Media)
                .Include(p => p.Student)
                .Include(p => p.Uploader)
                .Where(p => p.StudentId == studentId && !p.IsDeleted);

            // Apply filters
            if (photoType.HasValue)
            {
                query = query.Where(p => p.PhotoType == photoType.Value);
            }

            if (bodyAngle.HasValue)
            {
                query = query.Where(p => p.BodyAngle == bodyAngle.Value);
            }

            // If student is viewing, only show visible photos
            if (userRole != "PersonalTrainer")
            {
                query = query.Where(p => p.IsVisibleToStudent);
            }

            var photos = await query
                .OrderBy(p => p.PhotoDate)
                .Select(p => new ProgressPhotoResponse(
                    p.Id,
                    p.StudentId,
                    p.Student.Name,
                    p.MediaId,
                    p.Media.FileUrl,
                    p.Media.ThumbnailUrl,
                    p.UploadedBy,
                    p.Uploader.Name,
                    p.PhotoType,
                    p.BodyAngle,
                    p.PhotoDate,
                    p.WeightKg,
                    p.BodyFatPercentage,
                    p.MuscleMassKg,
                    p.ChestCm,
                    p.WaistCm,
                    p.HipsCm,
                    p.LeftArmCm,
                    p.RightArmCm,
                    p.LeftThighCm,
                    p.RightThighCm,
                    p.LeftCalfCm,
                    p.RightCalfCm,
                    p.TrainerNotes,
                    p.StudentNotes,
                    p.Caption,
                    p.IsVisibleToStudent,
                    p.IsPublic,
                    p.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(photos);
        })
        .WithName("GetStudentProgressPhotos")
        .WithSummary("Get all progress photos for a student");

        // GET /api/progress-photos/{id} - Get specific photo
        group.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            var photo = await context.ProgressPhotos
                .Include(p => p.Media)
                .Include(p => p.Student)
                .Include(p => p.Uploader)
                .Where(p => p.Id == id && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (photo == null)
            {
                return Results.NotFound(new { message = "Foto não encontrada" });
            }

            // Verify authorization
            if (userRole == "PersonalTrainer")
            {
                // Verify student belongs to this trainer
                if (photo.Student.PersonalTrainerId != userId)
                {
                    return Results.Forbid();
                }
            }
            else
            {
                // Student can only view their own photos that are visible
                if (photo.StudentId != userId || !photo.IsVisibleToStudent)
                {
                    return Results.Forbid();
                }
            }

            var response = new ProgressPhotoResponse(
                photo.Id,
                photo.StudentId,
                photo.Student.Name,
                photo.MediaId,
                photo.Media.FileUrl,
                photo.Media.ThumbnailUrl,
                photo.UploadedBy,
                photo.Uploader.Name,
                photo.PhotoType,
                photo.BodyAngle,
                photo.PhotoDate,
                photo.WeightKg,
                photo.BodyFatPercentage,
                photo.MuscleMassKg,
                photo.ChestCm,
                photo.WaistCm,
                photo.HipsCm,
                photo.LeftArmCm,
                photo.RightArmCm,
                photo.LeftThighCm,
                photo.RightThighCm,
                photo.LeftCalfCm,
                photo.RightCalfCm,
                photo.TrainerNotes,
                photo.StudentNotes,
                photo.Caption,
                photo.IsVisibleToStudent,
                photo.IsPublic,
                photo.CreatedAt
            );

            return Results.Ok(response);
        })
        .WithName("GetProgressPhoto")
        .WithSummary("Get a specific progress photo");

        // PUT /api/progress-photos/{id} - Update a progress photo
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateProgressPhotoRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            var photo = await context.ProgressPhotos
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

            if (photo == null)
            {
                return Results.NotFound(new { message = "Foto não encontrada" });
            }

            // Verify authorization
            if (userRole == "PersonalTrainer")
            {
                if (photo.Student.PersonalTrainerId != userId)
                {
                    return Results.Forbid();
                }
            }
            else
            {
                // Student can only update their own photos
                if (photo.StudentId != userId)
                {
                    return Results.Forbid();
                }
            }

            // Update fields
            photo.PhotoType = request.PhotoType;
            photo.BodyAngle = request.BodyAngle;
            photo.PhotoDate = request.PhotoDate;
            photo.WeightKg = request.WeightKg;
            photo.BodyFatPercentage = request.BodyFatPercentage;
            photo.MuscleMassKg = request.MuscleMassKg;
            photo.ChestCm = request.ChestCm;
            photo.WaistCm = request.WaistCm;
            photo.HipsCm = request.HipsCm;
            photo.LeftArmCm = request.LeftArmCm;
            photo.RightArmCm = request.RightArmCm;
            photo.LeftThighCm = request.LeftThighCm;
            photo.RightThighCm = request.RightThighCm;
            photo.LeftCalfCm = request.LeftCalfCm;
            photo.RightCalfCm = request.RightCalfCm;

            // Only trainer can update trainer notes
            if (userRole == "PersonalTrainer")
            {
                photo.TrainerNotes = request.TrainerNotes;
                photo.IsVisibleToStudent = request.IsVisibleToStudent;
                photo.IsPublic = request.IsPublic;
            }

            photo.StudentNotes = request.StudentNotes;
            photo.Caption = request.Caption;
            photo.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("UpdateProgressPhoto")
        .WithSummary("Update a progress photo");

        // DELETE /api/progress-photos/{id} - Delete a progress photo
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            var photo = await context.ProgressPhotos
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

            if (photo == null)
            {
                return Results.NotFound(new { message = "Foto não encontrada" });
            }

            // Verify authorization
            if (userRole == "PersonalTrainer")
            {
                if (photo.Student.PersonalTrainerId != userId)
                {
                    return Results.Forbid();
                }
            }
            else
            {
                // Student can only delete their own photos
                if (photo.StudentId != userId)
                {
                    return Results.Forbid();
                }
            }

            // Soft delete
            photo.IsDeleted = true;
            photo.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteProgressPhoto")
        .WithSummary("Delete a progress photo");

        // GET /api/progress-photos/compare - Compare two photos
        group.MapGet("/compare", async (
            [FromQuery] Guid beforePhotoId,
            [FromQuery] Guid afterPhotoId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = user.FindFirstValue(ClaimTypes.Role);

            var beforePhoto = await context.ProgressPhotos
                .Include(p => p.Media)
                .Include(p => p.Student)
                .Include(p => p.Uploader)
                .FirstOrDefaultAsync(p => p.Id == beforePhotoId && !p.IsDeleted, cancellationToken);

            var afterPhoto = await context.ProgressPhotos
                .Include(p => p.Media)
                .Include(p => p.Student)
                .Include(p => p.Uploader)
                .FirstOrDefaultAsync(p => p.Id == afterPhotoId && !p.IsDeleted, cancellationToken);

            if (beforePhoto == null || afterPhoto == null)
            {
                return Results.NotFound(new { message = "Uma ou ambas as fotos não foram encontradas" });
            }

            // Verify both photos belong to the same student
            if (beforePhoto.StudentId != afterPhoto.StudentId)
            {
                return Results.BadRequest(new { message = "As fotos devem ser do mesmo aluno" });
            }

            // Verify authorization
            if (userRole == "PersonalTrainer")
            {
                if (beforePhoto.Student.PersonalTrainerId != userId)
                {
                    return Results.Forbid();
                }
            }
            else
            {
                if (beforePhoto.StudentId != userId)
                {
                    return Results.Forbid();
                }
            }

            // Calculate metrics
            var daysBetween = (afterPhoto.PhotoDate - beforePhoto.PhotoDate).Days;
            double? weightChanged = null;
            double? weightChangePercentage = null;
            double? bodyFatChange = null;
            double? muscleMassChange = null;
            double? chestChange = null;
            double? waistChange = null;
            double? hipsChange = null;

            if (beforePhoto.WeightKg.HasValue && afterPhoto.WeightKg.HasValue)
            {
                weightChanged = afterPhoto.WeightKg.Value - beforePhoto.WeightKg.Value;
                weightChangePercentage = (weightChanged.Value / beforePhoto.WeightKg.Value) * 100;
            }

            if (beforePhoto.BodyFatPercentage.HasValue && afterPhoto.BodyFatPercentage.HasValue)
            {
                bodyFatChange = afterPhoto.BodyFatPercentage.Value - beforePhoto.BodyFatPercentage.Value;
            }

            if (beforePhoto.MuscleMassKg.HasValue && afterPhoto.MuscleMassKg.HasValue)
            {
                muscleMassChange = afterPhoto.MuscleMassKg.Value - beforePhoto.MuscleMassKg.Value;
            }

            if (beforePhoto.ChestCm.HasValue && afterPhoto.ChestCm.HasValue)
            {
                chestChange = afterPhoto.ChestCm.Value - beforePhoto.ChestCm.Value;
            }

            if (beforePhoto.WaistCm.HasValue && afterPhoto.WaistCm.HasValue)
            {
                waistChange = afterPhoto.WaistCm.Value - beforePhoto.WaistCm.Value;
            }

            if (beforePhoto.HipsCm.HasValue && afterPhoto.HipsCm.HasValue)
            {
                hipsChange = afterPhoto.HipsCm.Value - beforePhoto.HipsCm.Value;
            }

            var metrics = new ComparisonMetrics(
                daysBetween,
                weightChanged,
                weightChangePercentage,
                bodyFatChange,
                muscleMassChange,
                chestChange,
                waistChange,
                hipsChange
            );

            var beforeResponse = MapToResponse(beforePhoto);
            var afterResponse = MapToResponse(afterPhoto);

            var comparison = new PhotoComparisonResponse(
                beforeResponse,
                afterResponse,
                metrics
            );

            return Results.Ok(comparison);
        })
        .WithName("CompareProgressPhotos")
        .WithSummary("Compare two progress photos and show metrics");
    }

    private static ProgressPhotoResponse MapToResponse(ProgressPhoto photo)
    {
        return new ProgressPhotoResponse(
            photo.Id,
            photo.StudentId,
            photo.Student.Name,
            photo.MediaId,
            photo.Media.FileUrl,
            photo.Media.ThumbnailUrl,
            photo.UploadedBy,
            photo.Uploader.Name,
            photo.PhotoType,
            photo.BodyAngle,
            photo.PhotoDate,
            photo.WeightKg,
            photo.BodyFatPercentage,
            photo.MuscleMassKg,
            photo.ChestCm,
            photo.WaistCm,
            photo.HipsCm,
            photo.LeftArmCm,
            photo.RightArmCm,
            photo.LeftThighCm,
            photo.RightThighCm,
            photo.LeftCalfCm,
            photo.RightCalfCm,
            photo.TrainerNotes,
            photo.StudentNotes,
            photo.Caption,
            photo.IsVisibleToStudent,
            photo.IsPublic,
            photo.CreatedAt
        );
    }
}
