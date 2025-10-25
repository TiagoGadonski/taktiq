using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Personal.Queries;

// Query para buscar o progresso de um aluno específico,
// validando a permissão do personal.
public record GetClientProgressQuery(Guid PersonalId, Guid ClientId) : IRequest<ClientProgressDashboardResponse>;