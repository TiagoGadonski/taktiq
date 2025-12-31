using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.StudentGroups.Queries;

public class GetStudentGroupsQueryHandler : IRequestHandler<GetStudentGroupsQuery, IEnumerable<StudentGroupResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentGroupsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentGroupResponse>> Handle(GetStudentGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _context.StudentGroups
            .Where(g => g.TrainerId == request.TrainerId)
            .Include(g => g.Members)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new StudentGroupResponse(
                g.Id,
                g.Name,
                g.Description,
                g.Tags,
                g.Members.Count,
                g.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return groups;
    }
}
