using MediatR;

namespace GymHero.Application.Features.Posts.Commands;

public record DeletePostCommand(Guid PostId, Guid AuthorId) : IRequest;
