using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

// O comando para atualizar precisa de 3 informações:
// 1. O ID do plano a ser atualizado.
// 2. Os novos dados (Nome, Objetivo).
// 3. O ID do dono, para garantir que um usuário só possa editar seus próprios planos.
public record UpdateWorkoutPlanCommand(
    Guid Id, 
    string Name, 
    string? Goal, 
    Guid OwnerId) : IRequest; // IRequest sem tipo de retorno significa que ele retorna 'void'