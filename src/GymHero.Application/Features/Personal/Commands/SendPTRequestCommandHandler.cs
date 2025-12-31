using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class SendPTRequestCommandHandler : IRequestHandler<SendPTRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public SendPTRequestCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Guid> Handle(SendPTRequestCommand request, CancellationToken cancellationToken)
    {
        // Validate student exists
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found");
        }

        // Validate student doesn't already have a PT
        if (student.PersonalTrainerId.HasValue)
        {
            throw new InvalidOperationException("Student already has a Personal Trainer");
        }

        // Check if there's already a pending request
        var existingRequest = await _context.PersonalTrainerRequests
            .FirstOrDefaultAsync(r =>
                r.TrainerId == request.TrainerId &&
                r.StudentId == request.StudentId &&
                r.Status == "Pending",
                cancellationToken);

        if (existingRequest != null)
        {
            throw new InvalidOperationException("There is already a pending request for this student");
        }

        // Create the request
        var ptRequest = new PersonalTrainerRequest
        {
            TrainerId = request.TrainerId,
            StudentId = request.StudentId,
            Message = request.Message,
            Status = "Pending"
        };

        _context.PersonalTrainerRequests.Add(ptRequest);
        await _context.SaveChangesAsync(cancellationToken);

        // Get trainer info for notification
        var trainer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.TrainerId, cancellationToken);

        if (trainer != null)
        {
            // Send notification to student
            await _notificationService.CreatePTRequestNotificationAsync(
                request.StudentId,
                request.TrainerId,
                trainer.Name,
                request.Message,
                cancellationToken);
        }

        return ptRequest.Id;
    }
}
