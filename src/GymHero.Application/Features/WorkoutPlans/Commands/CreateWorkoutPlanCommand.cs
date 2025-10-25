using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

// O comando precisa do nome e do objetivo, mas também do ID do usuário dono.
// A API será responsável por extrair o OwnerId do token e passá-lo para cá.
public record CreateWorkoutPlanCommand(
    string Name, 
    string? Goal, 
    Guid OwnerId) : IRequest<WorkoutPlanResponse>;