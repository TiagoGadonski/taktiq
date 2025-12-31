using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Queries;

public class GetMyPTRequestsQueryHandler : IRequestHandler<GetMyPTRequestsQuery, IEnumerable<PTRequestResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetMyPTRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PTRequestResponse>> Handle(GetMyPTRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _context.PersonalTrainerRequests
            .Where(r => r.StudentId == request.StudentId && r.Status == "Pending")
            .Include(r => r.Trainer)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PTRequestResponse(
                r.Id,
                r.TrainerId,
                r.Trainer.Name,
                r.Trainer.ProfilePictureUrl,
                r.Message,
                r.Status,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return requests;
    }
}
