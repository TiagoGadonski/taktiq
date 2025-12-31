using GymHero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Common.Interfaces;

// Este é o nosso contrato com a camada de infraestrutura.
// Qualquer banco de dados que implemente esta interface pode ser usado.
public interface IApplicationDbContext
{
    // Expomos coleções das nossas entidades para que os casos de uso possam consultá-las.
    DbSet<User> Users { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<WorkoutPlan> WorkoutPlans { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutExercise> WorkoutExercises { get; }
    DbSet<WorkoutSession> WorkoutSessions { get; }
    DbSet<WorkoutSet> WorkoutSets { get; }
    DbSet<ProgressMetric> ProgressMetrics { get; }
    DbSet<Challenge> Challenges { get; }
    DbSet<ChallengeProgress> ChallengeProgresses { get; }
    DbSet<Badge> Badges { get; }
    DbSet<BadgeDefinition> BadgeDefinitions { get; }
    DbSet<Friendship> Friendships { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<UserActivityLog> UserActivityLogs { get; }
    DbSet<StudentInvitation> StudentInvitations { get; }
    DbSet<Post> Posts { get; }
    DbSet<PostView> PostViews { get; }
    DbSet<Certification> Certifications { get; }
    DbSet<Testimonial> Testimonials { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<UserAnnouncementRead> UserAnnouncementReads { get; }
    DbSet<Media> Medias { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<WithdrawalRequest> WithdrawalRequests { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<WorkoutPlanComment> WorkoutPlanComments { get; }
    DbSet<PersonalTrainerRequest> PersonalTrainerRequests { get; }

    // Um método para salvar as mudanças de forma assíncrona.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}