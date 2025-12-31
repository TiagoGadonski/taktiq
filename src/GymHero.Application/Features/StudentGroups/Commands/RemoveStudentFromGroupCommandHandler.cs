using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class RemoveStudentFromGroupCommandHandler : IRequestHandler<RemoveStudentFromGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveStudentFromGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveStudentFromGroupCommand request, CancellationToken cancellationToken)
    {
        // Verify group exists and belongs to this trainer
        var group = await _context.StudentGroups
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Grupo não encontrado");
        }

        if (group.TrainerId != request.TrainerId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para modificar este grupo");
        }

        // Find and remove the member
        var member = await _context.StudentGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == request.GroupId && m.StudentId == request.StudentId, cancellationToken);

        if (member == null)
        {
            throw new NotFoundException("Aluno não encontrado neste grupo");
        }

        _context.StudentGroupMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
