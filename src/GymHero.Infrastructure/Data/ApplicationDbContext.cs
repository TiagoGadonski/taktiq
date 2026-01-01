using System.Reflection;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Implementação da interface IApplicationDbContext
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<ProgressMetric> ProgressMetrics => Set<ProgressMetric>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeProgress> ChallengeProgresses => Set<ChallengeProgress>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<BadgeDefinition> BadgeDefinitions => Set<BadgeDefinition>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<StudentInvitation> StudentInvitations => Set<StudentInvitation>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostView> PostViews => Set<PostView>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<UserAnnouncementRead> UserAnnouncementReads => Set<UserAnnouncementRead>();
    public DbSet<Media> Medias => Set<Media>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<WorkoutPlanComment> WorkoutPlanComments => Set<WorkoutPlanComment>();
    public DbSet<PersonalTrainerRequest> PersonalTrainerRequests => Set<PersonalTrainerRequest>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<StudentGroupMember> StudentGroupMembers => Set<StudentGroupMember>();
    public DbSet<StudentAssessment> StudentAssessments => Set<StudentAssessment>();
    public DbSet<WorkoutSessionFeedback> WorkoutSessionFeedbacks => Set<WorkoutSessionFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidades que estiverem no mesmo assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Exemplo de configuração manual (Fluent API)
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email).IsUnique(); // Garante que o email seja único
        });

        modelBuilder.Entity<User>()
            // Um Personal Trainer (PersonalTrainer) tem muitos Alunos (Clients)
            .HasMany(personal => personal.Clients) 
            // Cada Aluno tem um (ou nenhum) Personal Trainer
            .WithOne(client => client.PersonalTrainer) 
            // A chave estrangeira que liga os dois é PersonalTrainerId
            .HasForeignKey(client => client.PersonalTrainerId)
            // Define o comportamento de exclusão. Se um Personal for apagado,
            // o PersonalTrainerId dos seus alunos ficará nulo (eles ficam sem personal).
            .OnDelete(DeleteBehavior.SetNull); 

        modelBuilder.Entity<Friendship>(entity =>
    {
        // Primary key
        entity.HasKey(f => f.Id);

        // Configura a relação do lado do "Requester" (quem envia)
        entity.HasOne(f => f.Requester)
              .WithMany(u => u.SentFriendRequests)
              .HasForeignKey(f => f.RequesterId)
              .OnDelete(DeleteBehavior.Restrict); // Impede que um utilizador seja apagado se tiver amizades

        // Configura a relação do lado do "Addressee" (quem recebe)
        entity.HasOne(f => f.Addressee)
              .WithMany(u => u.ReceivedFriendRequests)
              .HasForeignKey(f => f.AddresseeId)
              .OnDelete(DeleteBehavior.Restrict);

        // Composite index for finding friendships between two users (prevents duplicates)
        entity.HasIndex(f => new { f.RequesterId, f.AddresseeId })
              .IsUnique()
              .HasDatabaseName("IX_Friendships_RequesterAddressee");

        // Index for finding all friendships involving a user (for friend list queries)
        entity.HasIndex(f => f.RequesterId)
              .HasDatabaseName("IX_Friendships_RequesterId");

        entity.HasIndex(f => f.AddresseeId)
              .HasDatabaseName("IX_Friendships_AddresseeId");

        // Index for finding pending requests by status
        entity.HasIndex(f => new { f.AddresseeId, f.Status })
              .HasDatabaseName("IX_Friendships_AddresseeStatus");
    });

        // Performance indexes for frequently queried columns
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            // Index for user-specific session queries
            entity.HasIndex(s => s.OwnerId)
                .HasDatabaseName("IX_WorkoutSessions_OwnerId");

            // Composite index for common query pattern (user sessions by completion date)
            entity.HasIndex(s => new { s.OwnerId, s.CompletedAt })
                .HasDatabaseName("IX_WorkoutSessions_OwnerId_CompletedAt");
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            // Index for personal records and exercise-specific queries
            entity.HasIndex(s => s.ExerciseId)
                .HasDatabaseName("IX_WorkoutSets_ExerciseId");

            // Index for session-specific set queries
            entity.HasIndex(s => s.WorkoutSessionId)
                .HasDatabaseName("IX_WorkoutSets_WorkoutSessionId");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            // Index for creator-specific challenge queries
            entity.HasIndex(c => c.CreatorId)
                .HasDatabaseName("IX_Challenges_CreatorId");
        });

        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            // Index for user-specific workout plan queries
            entity.HasIndex(wp => wp.OwnerId)
                .HasDatabaseName("IX_WorkoutPlans_OwnerId");

            // Index for finding active plans
            entity.HasIndex(wp => new { wp.OwnerId, wp.IsActive })
                .HasDatabaseName("IX_WorkoutPlans_OwnerActive");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            // Index for user-specific notification queries (most common query)
            entity.HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notifications_UserId");

            // Composite index for unread notifications query
            entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_UserReadCreated");
        });

        modelBuilder.Entity<ChallengeProgress>(entity =>
        {
            // Index for user-specific challenge progress queries
            entity.HasIndex(cp => cp.ParticipantId)
                .HasDatabaseName("IX_ChallengeProgress_ParticipantId");

            // Index for challenge-specific progress queries
            entity.HasIndex(cp => cp.ChallengeId)
                .HasDatabaseName("IX_ChallengeProgress_ChallengeId");
        });

        modelBuilder.Entity<ProgressMetric>(entity =>
        {
            // Index for user-specific metrics queries
            entity.HasIndex(pm => pm.OwnerId)
                .HasDatabaseName("IX_ProgressMetrics_OwnerId");

            // Composite index for time-based metric queries
            entity.HasIndex(pm => new { pm.OwnerId, pm.Date })
                .HasDatabaseName("IX_ProgressMetrics_OwnerDate");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            // Configure relationship with User
            // Use SetNull to preserve activity logs for audit purposes even after user deletion
            entity.HasOne(log => log.User)
                  .WithMany()
                  .HasForeignKey(log => log.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Media>(entity =>
        {
            // Index for user-specific media queries
            entity.HasIndex(m => m.UploadedBy)
                .HasDatabaseName("IX_Media_UploadedBy");

            // Index for media type queries
            entity.HasIndex(m => m.MediaType)
                .HasDatabaseName("IX_Media_MediaType");

            // Index for entity-specific media queries
            entity.HasIndex(m => new { m.EntityId, m.UsageContext })
                .HasDatabaseName("IX_Media_EntityUsage");

            // Index for finding non-deleted media
            entity.HasIndex(m => m.IsDeleted)
                .HasDatabaseName("IX_Media_IsDeleted");

            // Configure relationship with User (uploader)
            entity.HasOne(m => m.Uploader)
                  .WithMany()
                  .HasForeignKey(m => m.UploadedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            // Index for buyer's transaction history
            entity.HasIndex(t => t.BuyerId)
                .HasDatabaseName("IX_Transactions_BuyerId");

            // Index for seller's transaction history
            entity.HasIndex(t => t.SellerId)
                .HasDatabaseName("IX_Transactions_SellerId");

            // Index for workout plan transactions
            entity.HasIndex(t => t.WorkoutPlanId)
                .HasDatabaseName("IX_Transactions_WorkoutPlanId");

            // Index for transaction status queries
            entity.HasIndex(t => t.Status)
                .HasDatabaseName("IX_Transactions_Status");

            // Index for Stripe payment intent lookup
            entity.HasIndex(t => t.StripePaymentIntentId)
                .HasDatabaseName("IX_Transactions_StripePaymentIntentId");

            // Configure relationship with buyer
            entity.HasOne(t => t.Buyer)
                  .WithMany()
                  .HasForeignKey(t => t.BuyerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with seller
            entity.HasOne(t => t.Seller)
                  .WithMany()
                  .HasForeignKey(t => t.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with workout plan
            entity.HasOne(t => t.WorkoutPlan)
                  .WithMany()
                  .HasForeignKey(t => t.WorkoutPlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Precision for decimal amount
            entity.Property(t => t.Amount)
                  .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            // Index for author-specific post queries
            entity.HasIndex(p => p.AuthorId)
                .HasDatabaseName("IX_Posts_AuthorId");

            // Index for published posts queries
            entity.HasIndex(p => new { p.IsPublished, p.PublishedAt })
                .HasDatabaseName("IX_Posts_PublishedDate");

            // Configure relationship with author
            entity.HasOne(p => p.Author)
                  .WithMany()
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostView>(entity =>
        {
            // Index for post-specific view queries (most common)
            entity.HasIndex(pv => pv.PostId)
                .HasDatabaseName("IX_PostViews_PostId");

            // Index for viewer-specific view queries
            entity.HasIndex(pv => pv.ViewerId)
                .HasDatabaseName("IX_PostViews_ViewerId");

            // Composite index for unique viewer counting
            entity.HasIndex(pv => new { pv.PostId, pv.ViewerId })
                .HasDatabaseName("IX_PostViews_PostViewer");

            // Index for time-based analytics queries
            entity.HasIndex(pv => new { pv.PostId, pv.ViewedAt })
                .HasDatabaseName("IX_PostViews_PostDate");

            // Configure relationship with post
            entity.HasOne(pv => pv.Post)
                  .WithMany(p => p.Views)
                  .HasForeignKey(pv => pv.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with viewer (optional)
            entity.HasOne(pv => pv.Viewer)
                  .WithMany()
                  .HasForeignKey(pv => pv.ViewerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            // Index for trainer-specific certification queries
            entity.HasIndex(c => c.TrainerId)
                .HasDatabaseName("IX_Certifications_TrainerId");

            // Index for finding active certifications
            entity.HasIndex(c => new { c.TrainerId, c.ExpiryDate })
                .HasDatabaseName("IX_Certifications_TrainerExpiry");

            // Configure relationship with trainer
            entity.HasOne(c => c.Trainer)
                  .WithMany(u => u.Certifications)
                  .HasForeignKey(c => c.TrainerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            // Index for trainer-specific testimonial queries
            entity.HasIndex(t => t.TrainerId)
                .HasDatabaseName("IX_Testimonials_TrainerId");

            // Index for finding approved testimonials
            entity.HasIndex(t => new { t.TrainerId, t.IsApproved })
                .HasDatabaseName("IX_Testimonials_TrainerApproved");

            // Index for student's testimonials
            entity.HasIndex(t => t.StudentId)
                .HasDatabaseName("IX_Testimonials_StudentId");

            // Configure relationship with trainer
            entity.HasOne(t => t.Trainer)
                  .WithMany(u => u.Testimonials)
                  .HasForeignKey(t => t.TrainerId)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete testimonials when trainer is deleted

            // Configure relationship with student
            entity.HasOne(t => t.Student)
                  .WithMany()
                  .HasForeignKey(t => t.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Constraint: Rating must be between 1 and 5
            entity.Property(t => t.Rating)
                  .HasAnnotation("CheckConstraint", "CK_Testimonials_Rating CHECK (Rating >= 1 AND Rating <= 5)");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            // Index for finding active announcements
            entity.HasIndex(a => new { a.IsActive, a.PublishedAt })
                .HasDatabaseName("IX_Announcements_ActivePublished");

            // Index for finding announcements by type
            entity.HasIndex(a => a.Type)
                .HasDatabaseName("IX_Announcements_Type");

            // Index for popup announcements
            entity.HasIndex(a => new { a.ShowAsPopup, a.IsActive, a.PublishedAt })
                .HasDatabaseName("IX_Announcements_PopupActive");
        });

        modelBuilder.Entity<UserAnnouncementRead>(entity =>
        {
            // Index for user-specific read queries
            entity.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_UserAnnouncementReads_UserId");

            // Index for announcement-specific read queries
            entity.HasIndex(r => r.AnnouncementId)
                .HasDatabaseName("IX_UserAnnouncementReads_AnnouncementId");

            // Composite unique index to prevent duplicate reads
            entity.HasIndex(r => new { r.UserId, r.AnnouncementId })
                .IsUnique()
                .HasDatabaseName("IX_UserAnnouncementReads_UserAnnouncement");

            // Configure relationship with user
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with announcement
            entity.HasOne(r => r.Announcement)
                  .WithMany(a => a.Reads)
                  .HasForeignKey(r => r.AnnouncementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            // Index for finding conversations by participant
            entity.HasIndex(c => c.Participant1Id)
                .HasDatabaseName("IX_Conversations_Participant1Id");

            entity.HasIndex(c => c.Participant2Id)
                .HasDatabaseName("IX_Conversations_Participant2Id");

            // Composite index for finding a conversation between two users
            entity.HasIndex(c => new { c.Participant1Id, c.Participant2Id })
                .IsUnique()
                .HasDatabaseName("IX_Conversations_Participants");

            // Index for sorting conversations by last message
            entity.HasIndex(c => new { c.Participant1Id, c.LastMessageAt })
                .HasDatabaseName("IX_Conversations_Participant1LastMessage");

            entity.HasIndex(c => new { c.Participant2Id, c.LastMessageAt })
                .HasDatabaseName("IX_Conversations_Participant2LastMessage");

            // Configure relationships with participants
            entity.HasOne(c => c.Participant1)
                .WithMany()
                .HasForeignKey(c => c.Participant1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Participant2)
                .WithMany()
                .HasForeignKey(c => c.Participant2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            // Index for finding messages by conversation (most common query)
            entity.HasIndex(m => m.ConversationId)
                .HasDatabaseName("IX_Messages_ConversationId");

            // Index for finding unread messages
            entity.HasIndex(m => new { m.ConversationId, m.ReadAt })
                .HasDatabaseName("IX_Messages_ConversationRead");

            // Index for message search by sender
            entity.HasIndex(m => m.SenderId)
                .HasDatabaseName("IX_Messages_SenderId");

            // Index for chronological ordering
            entity.HasIndex(m => new { m.ConversationId, m.SentAt })
                .HasDatabaseName("IX_Messages_ConversationSent");

            // Configure relationship with conversation
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with sender
            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}