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
            migrationBuilder.Sql(@"
                INSERT INTO ""Challenges"" (""Id"", ""Title"", ""Description"", ""Type"", ""TargetValue"", ""CreatedBy"", ""CreatedAt"", ""StartDate"", ""EndDate"", ""IsDefault"", ""TargetType"")
                VALUES
                -- Beginner Challenges (Short-term, motivational)
                ('11111111-1111-1111-1111-111111111111', 'Primeira Semana de Treinos', 'Complete 3 treinos em uma semana', 0, 3, NULL, NOW(), NOW(), NOW() + INTERVAL '30 days', true, 0),
                ('22222222-2222-2222-2222-222222222222', '10 Treinos Completados', 'Complete 10 treinos completos', 0, 10, NULL, NOW(), NOW(), NOW() + INTERVAL '90 days', true, 0),
                ('33333333-3333-3333-3333-333333333333', 'Guerreiro de 50 Séries', 'Complete 50 séries de exercícios', 1, 50, NULL, NOW(), NOW(), NOW() + INTERVAL '60 days', true, 0),

                -- Intermediate Challenges (Medium-term)
                ('44444444-4444-4444-4444-444444444444', 'Mês de Consistência', 'Complete 12 treinos em 30 dias', 0, 12, NULL, NOW(), NOW(), NOW() + INTERVAL '90 days', true, 0),
                ('55555555-5555-5555-5555-555555555555', '100 Séries Completas', 'Complete 100 séries de exercícios', 1, 100, NULL, NOW(), NOW(), NOW() + INTERVAL '90 days', true, 0),
                ('66666666-6666-6666-6666-666666666666', 'Mestre dos Pesos', 'Levante um total de 5000kg em todos os exercícios', 2, 5000, NULL, NOW(), NOW(), NOW() + INTERVAL '90 days', true, 0),

                -- Advanced Challenges (Long-term, aspirational)
                ('77777777-7777-7777-7777-777777777777', 'Centenário', 'Complete 100 treinos completos', 0, 100, NULL, NOW(), NOW(), NOW() + INTERVAL '365 days', true, 0),
                ('88888888-8888-8888-8888-888888888888', 'Lenda das 500 Séries', 'Complete 500 séries de exercícios', 1, 500, NULL, NOW(), NOW(), NOW() + INTERVAL '180 days', true, 0),
                ('99999999-9999-9999-9999-999999999999', 'Hércules', 'Levante um total de 50000kg em todos os exercícios', 2, 50000, NULL, NOW(), NOW(), NOW() + INTERVAL '365 days', true, 0);
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
