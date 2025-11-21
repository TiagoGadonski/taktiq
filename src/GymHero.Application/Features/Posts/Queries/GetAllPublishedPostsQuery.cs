using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Posts.Queries;

/// <summary>
/// Gets all published posts from all trainers (for public feed)
/// </summary>
public record GetAllPublishedPostsQuery(int Page, int PageSize) : IRequest<List<PostSummaryResponse>>;
