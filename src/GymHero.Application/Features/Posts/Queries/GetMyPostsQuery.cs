using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Posts.Queries;

public record GetMyPostsQuery(Guid AuthorId) : IRequest<List<PostResponse>>;
