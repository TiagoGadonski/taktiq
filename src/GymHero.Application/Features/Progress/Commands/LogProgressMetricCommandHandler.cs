using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.Progress.Commands;

public class LogProgressMetricCommandHandler : IRequestHandler<LogProgressMetricCommand, ProgressMetricResponse>
{
    private readonly IApplicationDbContext _context;

    public LogProgressMetricCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProgressMetricResponse> Handle(LogProgressMetricCommand request, CancellationToken cancellationToken)
    {
        var metric = new ProgressMetric
        {
            OwnerId = request.OwnerId,
            Type = request.Type,
            Value = request.Value,
            Unit = request.Unit,
            Date = request.Date
        };

        await _context.ProgressMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProgressMetricResponse(metric.Id, metric.Type, metric.Value, metric.Unit, metric.Date);
    }
}