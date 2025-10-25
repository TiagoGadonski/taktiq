using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Progress.Queries;

public class GetProgressHistoryQueryHandler : IRequestHandler<GetProgressHistoryQuery, IEnumerable<ProgressMetricResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetProgressHistoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ProgressMetricResponse>> Handle(GetProgressHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProgressMetrics
            .AsNoTracking()
            .Where(m => m.OwnerId == request.OwnerId)
            .OrderByDescending(m => m.Date)
            .Select(m => new ProgressMetricResponse(m.Id, m.Type, m.Value, m.Unit, m.Date))
            .ToListAsync(cancellationToken);
    }
}