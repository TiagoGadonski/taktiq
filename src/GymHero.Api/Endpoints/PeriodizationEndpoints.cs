using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymHero.Api.Endpoints;

public static class PeriodizationEndpoints
{
    public static void MapPeriodizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/periodization")
            .WithTags("Periodization")
            .RequireAuthorization();

        // Generate a periodized workout plan
        group.MapPost("/generate", GeneratePeriodizedPlan)
            .WithName("GeneratePeriodizedPlan");

        // Get predefined templates
        group.MapGet("/templates", GetPeriodizationTemplates)
            .WithName("GetPeriodizationTemplates");

        // Create custom template
        group.MapPost("/templates", CreatePeriodizationTemplate)
            .WithName("CreatePeriodizationTemplate");

        // Calculate progressions preview
        group.MapPost("/preview-progressions", PreviewProgressions)
            .WithName("PreviewProgressions");
    }

    private static async Task<IResult> GeneratePeriodizedPlan(
        [FromBody] GeneratePeriodizedPlanRequest request,
        IPeriodizationService periodizationService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var response = await periodizationService.GeneratePeriodizedPlanAsync(
                request,
                trainerId,
                cancellationToken);

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetPeriodizationTemplates(
        [FromQuery] PeriodizationModel? model,
        IPeriodizationService periodizationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var templates = await periodizationService.GetPeriodizationTemplatesAsync(
                model,
                cancellationToken);

            return Results.Ok(templates);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> CreatePeriodizationTemplate(
        [FromBody] CreatePeriodizationTemplateRequest request,
        IPeriodizationService periodizationService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var template = await periodizationService.CreateTemplateAsync(
                request,
                trainerId,
                cancellationToken);

            return Results.Created($"/api/periodization/templates/{template.Id}", template);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static IResult PreviewProgressions(
        [FromBody] PreviewProgressionsRequest request,
        IPeriodizationService periodizationService)
    {
        try
        {
            var progressions = periodizationService.CalculateProgressions(
                request.DurationWeeks,
                request.Model,
                request.StartingPhase,
                request.IncludeDeloadWeeks);

            return Results.Ok(new
            {
                durationWeeks = request.DurationWeeks,
                model = request.Model,
                progressions = progressions
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public record PreviewProgressionsRequest(
        int DurationWeeks,
        PeriodizationModel Model,
        TrainingPhase StartingPhase,
        bool IncludeDeloadWeeks
    );
}
