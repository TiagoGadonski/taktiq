using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class TestimonialEndpoints
{
    public static void MapTestimonialEndpoints(this IEndpointRouteBuilder app)
    {
        // Student endpoints - require authentication
        var studentGroup = app.MapGroup("/api/testimonials")
            .WithTags("Testimonials - Student")
            .RequireAuthorization();

        // Create a new testimonial
        studentGroup.MapPost("", async (
            [FromBody] CreateTestimonialRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var studentId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if student already has a testimonial for this trainer
            var existing = await context.Testimonials
                .AnyAsync(t => t.TrainerId == request.TrainerId && t.StudentId == studentId, ct);

            if (existing)
                return Results.BadRequest(new { message = "Você já deixou um depoimento para este personal trainer" });

            var testimonial = new Testimonial
            {
                Id = Guid.NewGuid(),
                TrainerId = request.TrainerId,
                StudentId = studentId,
                Content = request.Content,
                Rating = request.Rating,
                BeforePhotoUrl = request.BeforePhotoUrl,
                AfterPhotoUrl = request.AfterPhotoUrl,
                TransformationDetails = request.TransformationDetails,
                TrainingDuration = request.TrainingDuration,
                SubmittedAt = DateTime.UtcNow,
                IsApproved = false, // Requires trainer approval
                CreatedAt = DateTime.UtcNow
            };

            context.Testimonials.Add(testimonial);
            await context.SaveChangesAsync(ct);

            return Results.Created($"/api/testimonials/{testimonial.Id}", new { id = testimonial.Id });
        })
        .WithName("CreateTestimonial")
        .WithSummary("Creates a new testimonial for a trainer");

        // Get all testimonials written by the authenticated student
        studentGroup.MapGet("/my", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var studentId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var testimonials = await context.Testimonials
                .Include(t => t.Trainer)
                .Include(t => t.Student)
                .Where(t => t.StudentId == studentId)
                .OrderByDescending(t => t.SubmittedAt)
                .Select(t => new TestimonialResponse(
                    t.Id,
                    t.TrainerId,
                    t.Trainer.Name,
                    t.StudentId,
                    t.Student.Name,
                    t.Student.ProfilePictureUrl,
                    t.Content,
                    t.Rating,
                    t.SubmittedAt,
                    t.IsApproved,
                    t.BeforePhotoUrl,
                    t.AfterPhotoUrl,
                    t.TransformationDetails,
                    t.TrainingDuration
                ))
                .ToListAsync(ct);

            return Results.Ok(testimonials);
        })
        .WithName("GetMyTestimonials")
        .WithSummary("Gets all testimonials written by the authenticated student");

        // Update a testimonial (only if not approved yet)
        studentGroup.MapPut("/{testimonialId:guid}", async (
            Guid testimonialId,
            [FromBody] UpdateTestimonialRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var studentId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var testimonial = await context.Testimonials
                .FirstOrDefaultAsync(t => t.Id == testimonialId && t.StudentId == studentId, ct);

            if (testimonial == null)
                return Results.NotFound(new { message = "Depoimento não encontrado" });

            if (testimonial.IsApproved)
                return Results.BadRequest(new { message = "Não é possível editar um depoimento já aprovado" });

            testimonial.Content = request.Content;
            testimonial.Rating = request.Rating;
            testimonial.BeforePhotoUrl = request.BeforePhotoUrl;
            testimonial.AfterPhotoUrl = request.AfterPhotoUrl;
            testimonial.TransformationDetails = request.TransformationDetails;
            testimonial.TrainingDuration = request.TrainingDuration;

            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateTestimonial")
        .WithSummary("Updates an existing testimonial");

        // Delete a testimonial
        studentGroup.MapDelete("/{testimonialId:guid}", async (
            Guid testimonialId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var studentId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var testimonial = await context.Testimonials
                .FirstOrDefaultAsync(t => t.Id == testimonialId && t.StudentId == studentId, ct);

            if (testimonial == null)
                return Results.NotFound(new { message = "Depoimento não encontrado" });

            context.Testimonials.Remove(testimonial);
            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteTestimonial")
        .WithSummary("Deletes a testimonial");

        // Personal Trainer endpoints - manage received testimonials
        var personalGroup = app.MapGroup("/api/personal/testimonials")
            .WithTags("Personal Trainer - Testimonials")
            .RequireAuthorization("RequirePersonalRole");

        // Get all testimonials for the authenticated trainer
        personalGroup.MapGet("", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] bool? approvedOnly,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = context.Testimonials
                .Include(t => t.Trainer)
                .Include(t => t.Student)
                .Where(t => t.TrainerId == trainerId);

            if (approvedOnly.HasValue && approvedOnly.Value)
                query = query.Where(t => t.IsApproved);

            var testimonials = await query
                .OrderByDescending(t => t.SubmittedAt)
                .Select(t => new TestimonialResponse(
                    t.Id,
                    t.TrainerId,
                    t.Trainer.Name,
                    t.StudentId,
                    t.Student.Name,
                    t.Student.ProfilePictureUrl,
                    t.Content,
                    t.Rating,
                    t.SubmittedAt,
                    t.IsApproved,
                    t.BeforePhotoUrl,
                    t.AfterPhotoUrl,
                    t.TransformationDetails,
                    t.TrainingDuration
                ))
                .ToListAsync(ct);

            return Results.Ok(testimonials);
        })
        .WithName("GetMyTestimonials_PT")
        .WithSummary("Gets all testimonials received by the authenticated trainer");

        // Approve a testimonial
        personalGroup.MapPost("/{testimonialId:guid}/approve", async (
            Guid testimonialId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var testimonial = await context.Testimonials
                .FirstOrDefaultAsync(t => t.Id == testimonialId && t.TrainerId == trainerId, ct);

            if (testimonial == null)
                return Results.NotFound(new { message = "Depoimento não encontrado" });

            testimonial.IsApproved = true;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Depoimento aprovado com sucesso" });
        })
        .WithName("ApproveTestimonial")
        .WithSummary("Approves a testimonial for public display");

        // Unapprove a testimonial
        personalGroup.MapPost("/{testimonialId:guid}/unapprove", async (
            Guid testimonialId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var testimonial = await context.Testimonials
                .FirstOrDefaultAsync(t => t.Id == testimonialId && t.TrainerId == trainerId, ct);

            if (testimonial == null)
                return Results.NotFound(new { message = "Depoimento não encontrado" });

            testimonial.IsApproved = false;
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Aprovação removida" });
        })
        .WithName("UnapproveTestimonial")
        .WithSummary("Unapproves a testimonial");

        // Public endpoints - anyone can view approved testimonials
        var publicGroup = app.MapGroup("/api/trainers/{trainerId:guid}/testimonials")
            .WithTags("Testimonials - Public")
            .AllowAnonymous();

        // Get all approved testimonials for a specific trainer
        publicGroup.MapGet("", async (
            Guid trainerId,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var testimonials = await context.Testimonials
                .Include(t => t.Trainer)
                .Include(t => t.Student)
                .Where(t => t.TrainerId == trainerId && t.IsApproved)
                .OrderByDescending(t => t.SubmittedAt)
                .Select(t => new TestimonialResponse(
                    t.Id,
                    t.TrainerId,
                    t.Trainer.Name,
                    t.StudentId,
                    t.Student.Name,
                    t.Student.ProfilePictureUrl,
                    t.Content,
                    t.Rating,
                    t.SubmittedAt,
                    t.IsApproved,
                    t.BeforePhotoUrl,
                    t.AfterPhotoUrl,
                    t.TransformationDetails,
                    t.TrainingDuration
                ))
                .ToListAsync(ct);

            return Results.Ok(testimonials);
        })
        .WithName("GetTrainerTestimonials")
        .WithSummary("Gets all approved testimonials for a specific trainer");
    }
}
