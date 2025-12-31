using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.WorkoutPlans.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class AssignPlanToGroupCommandHandler : IRequestHandler<AssignPlanToGroupCommand, IEnumerable<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public AssignPlanToGroupCommandHandler(
        IApplicationDbContext context,
        ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<IEnumerable<Guid>> Handle(AssignPlanToGroupCommand request, CancellationToken cancellationToken)
    {
        // Verify group exists and belongs to this trainer
        var group = await _context.StudentGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Grupo não encontrado");
        }

        if (group.TrainerId != request.TrainerId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para usar este grupo");
        }

        if (!group.Members.Any())
        {
            throw new InvalidOperationException("O grupo não possui membros");
        }

        // Get student IDs from group members
        var studentIds = group.Members.Select(m => m.StudentId).ToList();

        // Delegate to the existing AssignPlanToMultipleStudentsCommand
        var command = new AssignPlanToMultipleStudentsCommand(
            request.TrainerId,
            studentIds,
            request.PlanName,
            request.Goal,
            request.TemplatePlanId,
            request.ExpirationDate
        );

        var planIds = await _sender.Send(command, cancellationToken);

        return planIds;
    }
}
