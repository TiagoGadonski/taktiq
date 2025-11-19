using MediatR;

namespace GymHero.Application.Features.Posts.Commands;

public record UpdatePostCommand(
    Guid PostId,
    Guid AuthorId,
    string Title,
    string Content,
    string? ImageUrl,
    bool IsPublished
) : IRequest;
