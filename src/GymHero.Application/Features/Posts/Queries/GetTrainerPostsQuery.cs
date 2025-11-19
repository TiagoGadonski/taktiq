using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Posts.Queries;

/// <summary>
/// Gets published posts from a specific trainer (for public viewing or student feed)
/// </summary>
public record GetTrainerPostsQuery(Guid TrainerId) : IRequest<List<PostSummaryResponse>>;
