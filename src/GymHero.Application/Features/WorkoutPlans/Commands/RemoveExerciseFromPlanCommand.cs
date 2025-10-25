using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

// O comando precisa de 3 IDs para ser seguro e preciso:
// 1. O ID do plano de treino (para garantir o escopo).
// 2. O ID do 'WorkoutExercise' (o registro da ligação que queremos apagar).
// 3. O ID do dono (para garantir a permissão).
public record RemoveExerciseFromPlanCommand(
    Guid WorkoutPlanId,
    Guid WorkoutExerciseId,
    Guid OwnerId) : IRequest;