using MediatR;

namespace GymHero.Application.Features.Notifications.Commands;

public record MarkNotificationAsReadCommand(
    Guid NotificationId,
    Guid UserId
) : IRequest<bool>;
