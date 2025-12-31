using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Queries;

public class GetStudentGroupDetailQueryHandler : IRequestHandler<GetStudentGroupDetailQuery, StudentGroupDetailResponse>
{
    private readonly IApplicationDbContext _context;

    public GetStudentGroupDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentGroupDetailResponse> Handle(GetStudentGroupDetailQuery request, CancellationToken cancellationToken)
    {
        var group = await _context.StudentGroups
            .Where(g => g.Id == request.GroupId)
            .Include(g => g.Members)
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Grupo não encontrado");
        }

        // Verify ownership
        if (group.TrainerId != request.TrainerId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para visualizar este grupo");
        }

        var members = group.Members.Select(m => new GroupMemberSummary(
            m.Student.Id,
            m.Student.Name,
            m.Student.Email,
            m.Student.ProfilePictureUrl,
            m.AddedAt
        )).ToList();

        var response = new StudentGroupDetailResponse(
            group.Id,
            group.Name,
            group.Description,
            group.Tags,
            group.Members.Count,
            group.CreatedAt,
            members
        );

        return response;
    }
}
