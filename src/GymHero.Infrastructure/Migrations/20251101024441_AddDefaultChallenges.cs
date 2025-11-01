using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert default challenges for all users
            // Use the first admin user's ID as the creator
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    admin_user_id UUID;
                BEGIN
                    -- Get the ID of the first admin user
                    SELECT ""Id"" INTO admin_user_id
                    FROM ""Users""
                    WHERE ""Role"" = 'Admin'
                    LIMIT 1;

                    -- Only insert challenges if an admin user exists
                    IF admin_user_id IS NOT NULL THEN
                        INSERT INTO ""Challenges"" (""Id"", ""CreatorId"", ""Title"", ""Type"", ""TargetValue"", ""StartDate"", ""EndDate"", ""Status"", ""TargetType"", ""IsDefault"", ""IconName"", ""CreatedAt"")
                        VALUES
                        -- Beginner Challenges (Short-term, motivational)
                        ('11111111-1111-1111-1111-111111111111', admin_user_id, 'Primeira Semana de Treinos', 'WorkoutCount', 3, NOW(), NOW() + INTERVAL '30 days', 'Active', 0, true, 'flame', NOW()),
                        ('22222222-2222-2222-2222-222222222222', admin_user_id, '10 Treinos Completados', 'WorkoutCount', 10, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'trophy', NOW()),
                        ('33333333-3333-3333-3333-333333333333', admin_user_id, 'Guerreiro de 50 Séries', 'SetCount', 50, NOW(), NOW() + INTERVAL '60 days', 'Active', 0, true, 'star', NOW()),

                        -- Intermediate Challenges (Medium-term)
                        ('44444444-4444-4444-4444-444444444444', admin_user_id, 'Mês de Consistência', 'WorkoutCount', 12, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'zap', NOW()),
                        ('55555555-5555-5555-5555-555555555555', admin_user_id, '100 Séries Completas', 'SetCount', 100, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'medal', NOW()),
                        ('66666666-6666-6666-6666-666666666666', admin_user_id, 'Mestre dos Pesos', 'TotalWeight', 5000, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'dumbbell', NOW()),

                        -- Advanced Challenges (Long-term, aspirational)
                        ('77777777-7777-7777-7777-777777777777', admin_user_id, 'Centenário', 'WorkoutCount', 100, NOW(), NOW() + INTERVAL '365 days', 'Active', 0, true, 'crown', NOW()),
                        ('88888888-8888-8888-8888-888888888888', admin_user_id, 'Lenda das 500 Séries', 'SetCount', 500, NOW(), NOW() + INTERVAL '180 days', 'Active', 0, true, 'award', NOW()),
                        ('99999999-9999-9999-9999-999999999999', admin_user_id, 'Hércules', 'TotalWeight', 50000, NOW(), NOW() + INTERVAL '365 days', 'Active', 0, true, 'shield', NOW());
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove default challenges
            migrationBuilder.Sql(@"
                DELETE FROM ""Challenges""
                WHERE ""Id"" IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444',
                    '55555555-5555-5555-5555-555555555555',
                    '66666666-6666-6666-6666-666666666666',
                    '77777777-7777-7777-7777-777777777777',
                    '88888888-8888-8888-8888-888888888888',
                    '99999999-9999-9999-9999-999999999999'
                );
            ");
        }
    }
}
