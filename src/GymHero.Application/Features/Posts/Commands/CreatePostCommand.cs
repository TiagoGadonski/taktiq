using MediatR;

namespace GymHero.Application.Features.Posts.Commands;

public record CreatePostCommand(
    Guid AuthorId,
    string Title,
    string Content,
    string? ImageUrl,
    bool IsPublished
) : IRequest<Guid>;
