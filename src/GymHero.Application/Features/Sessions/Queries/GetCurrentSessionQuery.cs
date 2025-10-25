using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Sessions.Queries;

public record GetCurrentSessionQuery(Guid UserId) : IRequest<WorkoutSessionDto?>;
