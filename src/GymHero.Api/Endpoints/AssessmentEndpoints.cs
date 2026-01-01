using System.Security.Claims;
using GymHero.Application.Features.Assessments.Commands;
using GymHero.Application.Features.Assessments.Queries;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class AssessmentEndpoints
{
    public static void MapAssessmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assessments")
            .WithTags("Assessments")
            .RequireAuthorization();  // Todos os endpoints requerem autenticação

        // POST /api/assessments - Criar nova avaliação (só PT)
        group.MapPost("/", async (
            [FromBody] CreateAssessmentRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            // Verificar se é Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var command = new CreateAssessmentCommand(trainerId, request);
                var assessmentId = await sender.Send(command);
                return Results.Created($"/api/assessments/{assessmentId}", new { id = assessmentId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: 403
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Erro ao criar avaliação",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("CreateAssessment");

        // GET /api/assessments/student/{studentId} - Listar avaliações do aluno (só PT)
        group.MapGet("/student/{studentId:guid}", async (
            Guid studentId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            // Verificar se é Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var query = new GetStudentAssessmentsQuery(trainerId, studentId);
                var assessments = await sender.Send(query);
                return Results.Ok(assessments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: 403
                );
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetStudentAssessments");

        // GET /api/assessments/{id} - Obter detalhes de uma avaliação (só PT)
        group.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            // Verificar se é Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var query = new GetAssessmentDetailQuery(id, trainerId);
                var assessment = await sender.Send(query);

                if (assessment == null)
                    return Results.NotFound(new { message = "Avaliação não encontrada" });

                return Results.Ok(assessment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: 403
                );
            }
        })
        .WithName("GetAssessmentDetail");

        // PUT /api/assessments/{id} - Atualizar avaliação (só PT)
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAssessmentRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            // Verificar se é Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var command = new UpdateAssessmentCommand(id, trainerId, request);
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: 403
                );
            }
        })
        .WithName("UpdateAssessment");

        // DELETE /api/assessments/{id} - Deletar avaliação (só PT)
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            // Verificar se é Personal Trainer
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            if (userRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var command = new DeleteAssessmentCommand(id, trainerId);
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: 403
                );
            }
        })
        .WithName("DeleteAssessment");
    }
}
