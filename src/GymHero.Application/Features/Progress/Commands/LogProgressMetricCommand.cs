using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Progress.Commands;

// O comando precisa dos dados da métrica e do ID do dono (do token)
public record LogProgressMetricCommand(
    Guid OwnerId,
    string Type,
    double Value,
    string Unit,
    DateTime Date) : IRequest<ProgressMetricResponse>;