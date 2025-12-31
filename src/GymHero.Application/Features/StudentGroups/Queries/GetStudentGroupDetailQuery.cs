using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.StudentGroups.Queries;

public record GetStudentGroupDetailQuery(
    Guid GroupId,
    Guid TrainerId
) : IRequest<StudentGroupDetailResponse>;
