using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class CertificationEndpoints
{
    public static void MapCertificationEndpoints(this IEndpointRouteBuilder app)
    {
        // Personal Trainer endpoints - require PT authorization
        var personalGroup = app.MapGroup("/api/personal/certifications")
            .WithTags("Personal Trainer - Certifications")
            .RequireAuthorization("RequirePersonalRole");

        // Create a new certification
        personalGroup.MapPost("", async (
            [FromBody] CreateCertificationRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certification = new Certification
            {
                Id = Guid.NewGuid(),
                TrainerId = trainerId,
                CertificationName = request.CertificationName,
                IssuingOrganization = request.IssuingOrganization,
                DateObtained = request.DateObtained,
                ExpiryDate = request.ExpiryDate,
                ImageUrl = request.ImageUrl,
                CredentialId = request.CredentialId,
                CreatedAt = DateTime.UtcNow
            };

            context.Certifications.Add(certification);
            await context.SaveChangesAsync(ct);

            return Results.Created($"/api/personal/certifications/{certification.Id}", new { id = certification.Id });
        })
        .WithName("CreateCertification")
        .WithSummary("Creates a new certification for the trainer");

        // Get all certifications for the authenticated trainer
        personalGroup.MapGet("", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certifications = await context.Certifications
                .Where(c => c.TrainerId == trainerId)
                .OrderByDescending(c => c.DateObtained ?? c.CreatedAt)
                .Select(c => new CertificationResponse(
                    c.Id,
                    c.TrainerId,
                    c.CertificationName,
                    c.IssuingOrganization,
                    c.DateObtained,
                    c.ExpiryDate,
                    c.ImageUrl,
                    c.CredentialId,
                    c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow,
                    c.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(certifications);
        })
        .WithName("GetMyCertifications")
        .WithSummary("Gets all certifications for the authenticated trainer");

        // Update a certification
        personalGroup.MapPut("/{certificationId:guid}", async (
            Guid certificationId,
            [FromBody] UpdateCertificationRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certification = await context.Certifications
                .FirstOrDefaultAsync(c => c.Id == certificationId && c.TrainerId == trainerId, ct);

            if (certification == null)
                return Results.NotFound(new { message = "Certificação não encontrada" });

            certification.CertificationName = request.CertificationName;
            certification.IssuingOrganization = request.IssuingOrganization;
            certification.DateObtained = request.DateObtained;
            certification.ExpiryDate = request.ExpiryDate;
            certification.ImageUrl = request.ImageUrl;
            certification.CredentialId = request.CredentialId;

            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateCertification")
        .WithSummary("Updates an existing certification");

        // Delete a certification
        personalGroup.MapDelete("/{certificationId:guid}", async (
            Guid certificationId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certification = await context.Certifications
                .FirstOrDefaultAsync(c => c.Id == certificationId && c.TrainerId == trainerId, ct);

            if (certification == null)
                return Results.NotFound(new { message = "Certificação não encontrada" });

            context.Certifications.Remove(certification);
            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteCertification")
        .WithSummary("Deletes a certification");

        // Public endpoints - anyone can view trainer certifications
        var publicGroup = app.MapGroup("/api/trainers/{trainerId:guid}/certifications")
            .WithTags("Certifications - Public")
            .AllowAnonymous();

        // Get all certifications for a specific trainer (public)
        publicGroup.MapGet("", async (
            Guid trainerId,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var certifications = await context.Certifications
                .Where(c => c.TrainerId == trainerId)
                .OrderByDescending(c => c.DateObtained ?? c.CreatedAt)
                .Select(c => new CertificationResponse(
                    c.Id,
                    c.TrainerId,
                    c.CertificationName,
                    c.IssuingOrganization,
                    c.DateObtained,
                    c.ExpiryDate,
                    c.ImageUrl,
                    c.CredentialId,
                    c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow,
                    c.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(certifications);
        })
        .WithName("GetTrainerCertifications")
        .WithSummary("Gets all certifications for a specific trainer");
    }
}
