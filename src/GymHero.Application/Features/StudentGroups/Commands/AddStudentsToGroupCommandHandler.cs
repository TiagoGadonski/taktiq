using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class AddStudentsToGroupCommandHandler : IRequestHandler<AddStudentsToGroupCommand>
{
    private readonly IApplicationDbContext _context;

    public AddStudentsToGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AddStudentsToGroupCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("Você não tem permissão para modificar este grupo");
        }

        // Verify all students belong to this trainer
        var students = await _context.Users
            .Where(u => request.StudentIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        if (students.Count != request.StudentIds.Count())
        {
            throw new NotFoundException("Um ou mais alunos não foram encontrados");
        }

        var invalidStudents = students.Where(s => s.PersonalTrainerId != request.TrainerId).ToList();
        if (invalidStudents.Any())
        {
            throw new UnauthorizedAccessException("Um ou mais alunos não pertencem a você");
        }

        // Get existing member IDs to avoid duplicates
        var existingMemberIds = group.Members.Select(m => m.StudentId).ToHashSet();

        // Add new members (skip students already in the group)
        foreach (var studentId in request.StudentIds.Where(id => !existingMemberIds.Contains(id)))
        {
            var member = new StudentGroupMember
            {
                GroupId = request.GroupId,
                StudentId = studentId,
                AddedAt = DateTime.UtcNow
            };

            _context.StudentGroupMembers.Add(member);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
