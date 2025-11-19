using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Posts.Queries;

public record GetPostByIdQuery(Guid PostId) : IRequest<PostResponse>;
