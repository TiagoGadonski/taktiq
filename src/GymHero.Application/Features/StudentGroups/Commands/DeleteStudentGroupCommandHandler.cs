using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class DeleteStudentGroupCommandHandler : IRequestHandler<DeleteStudentGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteStudentGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.StudentGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Grupo não encontrado");
        }

        // Verify ownership
        if (group.TrainerId != request.TrainerId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para deletar este grupo");
        }

        // Remove all members first (cascade should handle this, but being explicit)
        _context.StudentGroupMembers.RemoveRange(group.Members);

        // Remove the group
        _context.StudentGroups.Remove(group);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
