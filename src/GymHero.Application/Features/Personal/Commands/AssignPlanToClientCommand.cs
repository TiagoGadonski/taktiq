using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Personal.Commands;

public record AssignPlanToClientCommand(
    Guid PersonalId, // ID do personal (do token)
    Guid ClientId,   // ID do aluno (da URL)
    string PlanName, // Dados do plano (do corpo da requisição)
    string? PlanGoal) : IRequest<WorkoutPlanResponse>; // Retorna o plano criado