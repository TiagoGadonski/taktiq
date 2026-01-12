using GymHero.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymHero.Api.Endpoints;

public static class PdfEndpoints
{
    public static void MapPdfEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pdf")
            .WithTags("PDF Reports")
            .RequireAuthorization();

        // Generate student progress report
        group.MapGet("/student-progress/{studentId}", GenerateStudentProgressReport)
            .WithName("GenerateStudentProgressReport");

        // Generate workout plan PDF
        group.MapGet("/workout-plan/{workoutPlanId}", GenerateWorkoutPlanPdf)
            .WithName("GenerateWorkoutPlanPdf");

        // Generate assessment report
        group.MapGet("/assessment/{assessmentId}", GenerateAssessmentReport)
            .WithName("GenerateAssessmentReport");

        // Generate monthly summary
        group.MapGet("/monthly-summary/{year}/{month}", GenerateMonthlySummary)
            .WithName("GenerateMonthlySummary");

        // Generate assessment comparison
        group.MapGet("/assessment-comparison/{assessment1Id}/{assessment2Id}", GenerateAssessmentComparison)
            .WithName("GenerateAssessmentComparison");
    }

    private static async Task<IResult> GenerateStudentProgressReport(
        Guid studentId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        IPdfGenerationService pdfService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var pdfBytes = await pdfService.GenerateStudentProgressReportAsync(
                studentId,
                trainerId,
                startDate,
                endDate,
                cancellationToken);

            return Results.File(pdfBytes, "application/pdf", $"progresso-aluno-{studentId}-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GenerateWorkoutPlanPdf(
        Guid workoutPlanId,
        IPdfGenerationService pdfService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var pdfBytes = await pdfService.GenerateWorkoutPlanPdfAsync(
                workoutPlanId,
                trainerId,
                cancellationToken);

            return Results.File(pdfBytes, "application/pdf", $"plano-treino-{workoutPlanId}-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GenerateAssessmentReport(
        Guid assessmentId,
        IPdfGenerationService pdfService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var pdfBytes = await pdfService.GenerateAssessmentReportPdfAsync(
                assessmentId,
                trainerId,
                cancellationToken);

            return Results.File(pdfBytes, "application/pdf", $"avaliacao-{assessmentId}-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GenerateMonthlySummary(
        int year,
        int month,
        IPdfGenerationService pdfService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (month < 1 || month > 12)
        {
            return Results.BadRequest(new { message = "Month must be between 1 and 12" });
        }

        if (year < 2020 || year > 2100)
        {
            return Results.BadRequest(new { message = "Year must be between 2020 and 2100" });
        }

        try
        {
            var pdfBytes = await pdfService.GenerateMonthlySummaryReportAsync(
                trainerId,
                year,
                month,
                cancellationToken);

            return Results.File(pdfBytes, "application/pdf", $"resumo-mensal-{year}-{month:D2}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GenerateAssessmentComparison(
        Guid assessment1Id,
        Guid assessment2Id,
        IPdfGenerationService pdfService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var pdfBytes = await pdfService.GenerateAssessmentComparisonPdfAsync(
                assessment1Id,
                assessment2Id,
                trainerId,
                cancellationToken);

            return Results.File(pdfBytes, "application/pdf", $"comparacao-avaliacoes-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
