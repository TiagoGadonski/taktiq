using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class UpdateStudentGroupCommandHandler : IRequestHandler<UpdateStudentGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateStudentGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.StudentGroups
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Grupo não encontrado");
        }

        // Verify ownership
        if (group.TrainerId != request.TrainerId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar este grupo");
        }

        group.Name = request.Name;
        group.Description = request.Description;
        group.Tags = request.Tags;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
