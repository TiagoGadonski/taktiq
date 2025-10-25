using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Progress.Queries;

// Query para buscar o histórico de métricas de um utilizador
public record GetProgressHistoryQuery(Guid OwnerId) : IRequest<IEnumerable<ProgressMetricResponse>>;