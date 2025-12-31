using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Personal.Queries;

public record GetMyPTRequestsQuery(
    Guid StudentId
) : IRequest<IEnumerable<PTRequestResponse>>;
