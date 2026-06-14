using System.Security.Claims;
using System.Text.Json;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class AssessmentProtocolEndpoints
{
    public static void MapAssessmentProtocolEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assessment-protocols")
            .WithTags("Assessment Protocols")
            .RequireAuthorization();

        // GET /api/assessment-protocols - List all public protocols
        group.MapGet("/", async (
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var protocols = await context.AssessmentProtocols
                .Where(p => p.IsPublic)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .Select(p => new ProtocolListResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ProtocolType,
                    p.Category,
                    p.DurationMinutes,
                    p.IsPublic
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(protocols);
        })
        .WithName("ListProtocols")
        .WithSummary("List all available assessment protocols");

        // GET /api/assessment-protocols/{id} - Get protocol details
        group.MapGet("/{id:guid}", async (
            Guid id,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var protocol = await context.AssessmentProtocols
                .Where(p => p.Id == id && p.IsPublic)
                .Select(p => new ProtocolDetailResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ProtocolType,
                    p.Category,
                    p.Instructions,
                    p.Equipment,
                    p.DurationMinutes,
                    p.MeasurementFields,
                    p.NormativeData,
                    p.CalculationFormula,
                    p.IsPublic,
                    p.CreatedByUserId,
                    p.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (protocol == null)
                return Results.NotFound(new { message = "Protocolo não encontrado" });

            return Results.Ok(protocol);
        })
        .WithName("GetProtocolDetail")
        .WithSummary("Get detailed information about a specific protocol");

        // POST /api/assessment-protocols/results - Record a protocol result
        group.MapPost("/results", async (
            [FromBody] RecordProtocolResultRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            // Verify user is a Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: "Apenas Personal Trainers podem registrar resultados de protocolos",
                    statusCode: 403
                );
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify protocol exists
            var protocol = await context.AssessmentProtocols
                .FindAsync(new object[] { request.ProtocolId }, cancellationToken);

            if (protocol == null)
            {
                return Results.NotFound(new { message = "Protocolo não encontrado" });
            }

            // Verify student exists and belongs to this trainer
            var student = await context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == request.StudentId &&
                    u.PersonalTrainerId == trainerId,
                    cancellationToken);

            if (student == null)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: "Aluno não encontrado ou não pertence a você",
                    statusCode: 403
                );
            }

            // Parse measurements and calculate score if formula exists
            double? calculatedScore = null;
            string? resultUnit = null;
            string? classification = null;

            try
            {
                var measurements = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Measurements);

                // Simple calculation for common protocols
                if (!string.IsNullOrEmpty(protocol.CalculationFormula) && measurements != null)
                {
                    calculatedScore = CalculateScore(protocol, measurements);
                }

                // Determine classification based on normative data
                if (!string.IsNullOrEmpty(protocol.NormativeData) && calculatedScore.HasValue)
                {
                    classification = GetClassification(protocol, calculatedScore.Value, student);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - store the raw measurements
            }

            // Create the result
            var result = new AssessmentResult
            {
                Id = Guid.NewGuid(),
                ProtocolId = request.ProtocolId,
                StudentId = request.StudentId,
                TrainerId = trainerId,
                StudentAssessmentId = request.StudentAssessmentId,
                TestDate = request.TestDate,
                Measurements = request.Measurements,
                CalculatedScore = calculatedScore,
                ResultUnit = resultUnit,
                Classification = classification,
                TrainerNotes = request.TrainerNotes,
                CreatedAt = DateTime.UtcNow
            };

            await context.AssessmentResults.AddAsync(result, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/assessment-protocols/results/{result.Id}", new { id = result.Id });
        })
        .WithName("RecordProtocolResult")
        .WithSummary("Record the result of executing a protocol on a student");

        // GET /api/assessment-protocols/results/student/{studentId} - Get all results for a student
        group.MapGet("/results/student/{studentId:guid}", async (
            Guid studentId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify student belongs to trainer
            var student = await context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == studentId &&
                    u.PersonalTrainerId == trainerId,
                    cancellationToken);

            if (student == null)
            {
                return Results.NotFound(new { message = "Aluno não encontrado" });
            }

            var results = await context.AssessmentResults
                .Include(r => r.Protocol)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.TestDate)
                .Select(r => new ProtocolResultResponse(
                    r.Id,
                    r.ProtocolId,
                    r.Protocol.Name,
                    r.StudentId,
                    student.Name,
                    r.TestDate,
                    r.Measurements,
                    r.CalculatedScore,
                    r.ResultUnit,
                    r.Classification,
                    r.TrainerNotes,
                    r.Recommendations,
                    r.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(results);
        })
        .WithName("GetStudentProtocolResults")
        .WithSummary("Get all protocol results for a specific student");
    }

    private static double? CalculateScore(AssessmentProtocol protocol, Dictionary<string, object> measurements)
    {
        try
        {
            // Extract common measurement values
            double distance = GetMeasurementValue(measurements, "distance");
            double repetitions = GetMeasurementValue(measurements, "repetitions");
            double duration = GetMeasurementValue(measurements, "duration");
            double heartRate = GetMeasurementValue(measurements, "heartRate");
            double age = GetMeasurementValue(measurements, "age");

            // Calculate based on protocol type
            return protocol.ProtocolType switch
            {
                Shared.Enums.AssessmentProtocolType.CooperTest => (distance - 504.9) / 44.73,
                Shared.Enums.AssessmentProtocolType.PushUpTest => repetitions,
                Shared.Enums.AssessmentProtocolType.SitUpTest => repetitions,
                Shared.Enums.AssessmentProtocolType.PlankTest => duration,
                Shared.Enums.AssessmentProtocolType.SitAndReach => distance,
                Shared.Enums.AssessmentProtocolType.RestingHeartRate => heartRate,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static double GetMeasurementValue(Dictionary<string, object> measurements, string key)
    {
        if (measurements.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetDouble();
            }
            if (double.TryParse(value?.ToString(), out var result))
            {
                return result;
            }
        }
        return 0;
    }

    private static string? GetClassification(AssessmentProtocol protocol, double score, User student)
    {
        // Simplified classification - could be enhanced with age/gender specific tables
        // For now, return a basic classification
        return protocol.ProtocolType switch
        {
            Shared.Enums.AssessmentProtocolType.CooperTest when score > 50 => "Excelente",
            Shared.Enums.AssessmentProtocolType.CooperTest when score > 45 => "Bom",
            Shared.Enums.AssessmentProtocolType.CooperTest when score > 40 => "Médio",
            Shared.Enums.AssessmentProtocolType.CooperTest => "Abaixo da Média",
            _ => null
        };
    }
}
