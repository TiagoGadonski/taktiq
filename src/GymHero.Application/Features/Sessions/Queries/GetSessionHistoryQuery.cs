using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Sessions.Queries;

public record GetSessionHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<PaginatedResponse<WorkoutSessionDto>>;
