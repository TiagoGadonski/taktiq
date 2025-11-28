namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for creating a workout plan comment
/// </summary>
public record CreateWorkoutPlanCommentRequest(
    Guid WorkoutPlanId,
    string Content,
    Guid? ParentCommentId
);

/// <summary>
/// Response DTO for a workout plan comment
/// </summary>
public record WorkoutPlanCommentResponse(
    Guid Id,
    Guid WorkoutPlanId,
    Guid UserId,
    string UserName,
    string Content,
    Guid? ParentCommentId,
    DateTime CreatedAt,
    List<WorkoutPlanCommentResponse> Replies
);

/// <summary>
/// Request DTO for updating a workout plan comment
/// </summary>
public record UpdateWorkoutPlanCommentRequest(
    string Content
);
