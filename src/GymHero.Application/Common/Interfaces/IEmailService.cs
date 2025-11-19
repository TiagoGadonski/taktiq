namespace GymHero.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendWelcomeEmailAsync(string email, string userName);
    Task SendStudentInvitationEmailAsync(string email, string trainerName, string activationToken, string workoutPlanName);
}
