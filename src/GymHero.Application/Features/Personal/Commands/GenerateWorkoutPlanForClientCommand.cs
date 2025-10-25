using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Personal.Commands;

public record GenerateWorkoutPlanForClientCommand(
    Guid PersonalId,
    Guid ClientId,
    string Goal,
    string Level,
    int DaysPerWeek) : IRequest<WorkoutPlanResponse>; // Retorna o plano gerado