using MediatR;
namespace GymHero.Application.Features.WorkoutPlans.Commands;

// O comando precisa de todas as informações:
// - O ID do plano (da URL)
// - O ID do dono (do token)
// - Os dados do exercício a ser adicionado (do corpo da requisição)
public record AddExerciseToPlanCommand(
    Guid WorkoutPlanId,
    Guid OwnerId,
    Guid ExerciseId,
    int Order,
    int TargetSets,
    int TargetReps,
    double TargetLoad) : IRequest<Guid>; // Retorna o ID do WorkoutExercise criado