using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymHero.Api.Endpoints;

public static class WhatsAppEndpoints
{
    public static void MapWhatsAppEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/whatsapp")
            .WithTags("WhatsApp")
            .RequireAuthorization();

        // Send text message
        group.MapPost("/send-text", SendTextMessage)
            .WithName("SendWhatsAppText");

        // Send media message
        group.MapPost("/send-media", SendMediaMessage)
            .WithName("SendWhatsAppMedia");

        // Send workout plan notification
        group.MapPost("/send-workout-plan", SendWorkoutPlanNotification)
            .WithName("SendWorkoutPlanNotification");

        // Send workout reminder
        group.MapPost("/send-workout-reminder", SendWorkoutReminder)
            .WithName("SendWorkoutReminder");

        // Send progress update
        group.MapPost("/send-progress-update", SendProgressUpdate)
            .WithName("SendProgressUpdate");

        // Send assessment results
        group.MapPost("/send-assessment-results", SendAssessmentResults)
            .WithName("SendAssessmentResults");

        // Check WhatsApp service status
        group.MapGet("/status", GetWhatsAppStatus)
            .WithName("GetWhatsAppStatus");
    }

    private static async Task<IResult> SendTextMessage(
        [FromBody] SendTextMessageRequest request,
        IWhatsAppService whatsAppService,
        IApplicationDbContext context,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Verify the trainer has access to this phone number (basic validation)
        var result = await whatsAppService.SendTextMessageAsync(
            request.ToPhoneNumber,
            request.Message,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> SendMediaMessage(
        [FromBody] SendMediaMessageRequest request,
        IWhatsAppService whatsAppService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await whatsAppService.SendMediaMessageAsync(
            request.ToPhoneNumber,
            request.MediaUrl,
            request.Caption,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> SendWorkoutPlanNotification(
        [FromBody] SendWorkoutPlanNotificationRequest request,
        IWhatsAppService whatsAppService,
        IApplicationDbContext context,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get student details
        var student = await context.Users
            .Where(u => u.Id == request.StudentId && u.PersonalTrainerId == trainerId)
            .Select(u => new { u.Name, u.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return Results.NotFound(new { message = "Student not found or you don't have permission" });
        }

        if (string.IsNullOrEmpty(student.PhoneNumber))
        {
            return Results.BadRequest(new { message = "Student does not have a phone number" });
        }

        var result = await whatsAppService.SendWorkoutPlanAsync(
            student.PhoneNumber,
            student.Name,
            request.WorkoutPlanName,
            request.WorkoutPlanUrl,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> SendWorkoutReminder(
        [FromBody] SendWorkoutReminderRequest request,
        IWhatsAppService whatsAppService,
        IApplicationDbContext context,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get student details
        var student = await context.Users
            .Where(u => u.Id == request.StudentId && u.PersonalTrainerId == trainerId)
            .Select(u => new { u.Name, u.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return Results.NotFound(new { message = "Student not found or you don't have permission" });
        }

        if (string.IsNullOrEmpty(student.PhoneNumber))
        {
            return Results.BadRequest(new { message = "Student does not have a phone number" });
        }

        var result = await whatsAppService.SendWorkoutReminderAsync(
            student.PhoneNumber,
            student.Name,
            request.WorkoutName,
            request.ScheduledTime,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> SendProgressUpdate(
        [FromBody] SendProgressUpdateRequest request,
        IWhatsAppService whatsAppService,
        IApplicationDbContext context,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get student details
        var student = await context.Users
            .Where(u => u.Id == request.StudentId && u.PersonalTrainerId == trainerId)
            .Select(u => new { u.Name, u.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return Results.NotFound(new { message = "Student not found or you don't have permission" });
        }

        if (string.IsNullOrEmpty(student.PhoneNumber))
        {
            return Results.BadRequest(new { message = "Student does not have a phone number" });
        }

        var result = await whatsAppService.SendProgressUpdateAsync(
            student.PhoneNumber,
            student.Name,
            request.ProgressSummary,
            request.ProgressPhotoUrl,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> SendAssessmentResults(
        [FromBody] SendAssessmentResultsRequest request,
        IWhatsAppService whatsAppService,
        IApplicationDbContext context,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get student details
        var student = await context.Users
            .Where(u => u.Id == request.StudentId && u.PersonalTrainerId == trainerId)
            .Select(u => new { u.Name, u.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return Results.NotFound(new { message = "Student not found or you don't have permission" });
        }

        if (string.IsNullOrEmpty(student.PhoneNumber))
        {
            return Results.BadRequest(new { message = "Student does not have a phone number" });
        }

        var result = await whatsAppService.SendAssessmentResultsAsync(
            student.PhoneNumber,
            student.Name,
            request.AssessmentSummary,
            cancellationToken);

        var response = new WhatsAppMessageResponse(
            result.Success,
            result.MessageId,
            result.ErrorMessage,
            result.SentAt);

        return result.Success ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> GetWhatsAppStatus(
        IWhatsAppService whatsAppService,
        CancellationToken cancellationToken)
    {
        // Check if WhatsApp service is enabled
        var isEnabled = await whatsAppService.IsWhatsAppEnabledAsync("dummy", cancellationToken);

        return Results.Ok(new
        {
            enabled = isEnabled,
            provider = "Twilio",
            status = isEnabled ? "active" : "disabled"
        });
    }
}
