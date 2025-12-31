using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public class CreateStudentGroupCommandHandler : IRequestHandler<CreateStudentGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStudentGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStudentGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new StudentGroup
        {
            TrainerId = request.TrainerId,
            Name = request.Name,
            Description = request.Description,
            Tags = request.Tags
        };

        _context.StudentGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        return group.Id;
    }
}
