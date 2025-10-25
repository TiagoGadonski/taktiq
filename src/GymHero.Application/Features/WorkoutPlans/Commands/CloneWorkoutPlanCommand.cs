using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record CloneWorkoutPlanCommand(
    Guid OriginalPlanId, // O ID do plano que queremos clonar
    Guid NewOwnerId      // O ID do utilizador que está a clicar no botão (vem do token)
) : IRequest<WorkoutPlanResponse>; // Retorna o novo plano clonado