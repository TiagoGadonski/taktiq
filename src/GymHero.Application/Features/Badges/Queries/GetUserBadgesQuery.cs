using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Badges.Queries;

public record GetUserBadgesQuery(Guid UserId) : IRequest<IEnumerable<BadgeResponse>>;