using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Friends.Queries;

public record SearchUsersQuery(string SearchTerm, Guid CurrentUserId) : IRequest<IEnumerable<UserSearchResponse>>;