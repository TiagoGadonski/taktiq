using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.StudentGroups.Queries;

public record GetStudentGroupsQuery(Guid TrainerId) : IRequest<IEnumerable<StudentGroupResponse>>;
