using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Challenges.Queries;

// Query para buscar todos os desafios de um utilizador específico.
// Espera-se que retorne uma coleção de ChallengeResponse.
public record GetUserChallengesQuery(Guid OwnerId) : IRequest<IEnumerable<ChallengeResponse>>;