using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class RespondToPTRequestCommandHandler : IRequestHandler<RespondToPTRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RespondToPTRequestCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(RespondToPTRequestCommand request, CancellationToken cancellationToken)
    {
        // Find the request
        var ptRequest = await _context.PersonalTrainerRequests
            .Include(r => r.Trainer)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (ptRequest == null)
        {
            throw new NotFoundException("Request not found");
        }

        // Verify the request belongs to the student
        if (ptRequest.StudentId != request.StudentId)
        {
            throw new UnauthorizedAccessException("You don't have permission to respond to this request");
        }

        // Verify request is still pending
        if (ptRequest.Status != "Pending")
        {
            throw new InvalidOperationException("This request has already been responded to");
        }

        // Update request status
        ptRequest.Status = request.Accepted ? "Accepted" : "Rejected";
        ptRequest.RespondedAt = DateTime.UtcNow;

        // If accepted, assign the PT to the student
        if (request.Accepted)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

            if (student != null)
            {
                student.PersonalTrainerId = ptRequest.TrainerId;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Send notification to PT
        if (request.Accepted)
        {
            await _notificationService.CreatePTRequestAcceptedNotificationAsync(
                ptRequest.TrainerId,
                request.StudentId,
                ptRequest.Student.Name,
                cancellationToken);
        }
        else
        {
            await _notificationService.CreatePTRequestRejectedNotificationAsync(
                ptRequest.TrainerId,
                request.StudentId,
                ptRequest.Student.Name,
                cancellationToken);
        }
    }
}
