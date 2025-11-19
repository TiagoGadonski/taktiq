const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

// New challenges to add (matching the C# code)
const newChallenges = [
    // Onboarding and Setup Challenges
    { title: "TaktIQ Iniciante", type: "Setup", targetValue: 1, iconName: "user-check" },
    { title: "Planejador Pro", type: "Planos", targetValue: 1, iconName: "clipboard-list" },
    { title: "Meu Arsenal", type: "Exercícios", targetValue: 1, iconName: "dumbbell" },

    // Consistency Challenges
    { title: "Primeira Semana", type: "Treinos", targetValue: 3, iconName: "calendar-check" },
    { title: "Maratonista", type: "Treinos", targetValue: 10, iconName: "footprints" },
    { title: "Fim de Semana Ativo", type: "Treinos", targetValue: 1, iconName: "sun" },

    // Progress and Strength Challenges
    { title: "Força Bruta", type: "PR", targetValue: 1, iconName: "zap" },
    { title: "Monstro de Volume", type: "Volume", targetValue: 1000, iconName: "weight" },
    { title: "Superador", type: "PR", targetValue: 3, iconName: "star" },

    // Social Challenges
    { title: "Conexão", type: "Social", targetValue: 1, iconName: "users" },
    { title: "Incentivador", type: "Social", targetValue: 1, iconName: "share-2" },
    { title: "Círculo de Ferro", type: "Social", targetValue: 5, iconName: "users" },

    // Advanced Workout Consistency
    { title: "Disciplina de Aço", type: "Treinos", targetValue: 25, iconName: "shield" },
    { title: "Centurião", type: "Treinos", targetValue: 50, iconName: "award" },
    { title: "Lenda do Ginásio", type: "Treinos", targetValue: 100, iconName: "crown" },

    // Volume Challenges - Progressive
    { title: "Levantador", type: "Volume", targetValue: 5000, iconName: "trending-up" },
    { title: "Titã de Ferro", type: "Volume", targetValue: 10000, iconName: "activity" },
    { title: "Atlas", type: "Volume", targetValue: 25000, iconName: "mountain" },

    // PR Challenges - More tiers
    { title: "Máquina de Recordes", type: "PR", targetValue: 5, iconName: "target" },
    { title: "Imparável", type: "PR", targetValue: 10, iconName: "zap" },

    // Streak Challenges (consecutive days)
    { title: "Sequência de 7", type: "Streak", targetValue: 7, iconName: "flame" },
    { title: "Mês Perfeito", type: "Streak", targetValue: 30, iconName: "fire" },

    // Time-based Challenges
    { title: "Madrugador", type: "Timing", targetValue: 5, iconName: "sunrise" },
    { title: "Guerreiro Noturno", type: "Timing", targetValue: 5, iconName: "moon" },

    // Exercise Variety
    { title: "Explorador", type: "Exercícios", targetValue: 10, iconName: "compass" },
    { title: "Mestre de Movimentos", type: "Exercícios", targetValue: 25, iconName: "layers" },

    // Workout Plan Challenges
    { title: "Arquiteto do Corpo", type: "Planos", targetValue: 3, iconName: "layout" },
    { title: "Estrategista", type: "Planos", targetValue: 5, iconName: "book-open" }
];

async function seedChallenges() {
    const client = new Client({
        connectionString: pgConnectionString,
        ssl: {
            rejectUnauthorized: false
        }
    });

    try {
        await client.connect();
        console.log('✓ Connected to database\n');

        // First, get all existing challenge titles to avoid duplicates
        const existing = await client.query(`
            SELECT "Title" FROM "Challenges" WHERE "IsDefault" = true
        `);

        const existingTitles = new Set(existing.rows.map(r => r.Title));
        console.log(`Found ${existingTitles.size} existing default challenges\n`);

        // Filter out challenges that already exist
        const challengesToAdd = newChallenges.filter(c => !existingTitles.has(c.title));

        if (challengesToAdd.length === 0) {
            console.log('All challenges already exist in database!');
            return;
        }

        console.log(`Adding ${challengesToAdd.length} new challenges:\n`);

        // Get the CreatorId used by existing system challenges
        const creatorResult = await client.query(`
            SELECT "CreatorId" FROM "Challenges" WHERE "IsDefault" = true LIMIT 1
        `);

        const creatorId = creatorResult.rows.length > 0
            ? creatorResult.rows[0].CreatorId
            : 'f634d666-e130-4f74-9bda-05920afdbf3e'; // Fallback to the ID we found

        console.log(`Using CreatorId: ${creatorId}\n`);

        let added = 0;

        for (const challenge of challengesToAdd) {
            const now = new Date();
            const endDate = new Date();
            endDate.setFullYear(endDate.getFullYear() + 10);

            await client.query(`
                INSERT INTO "Challenges"
                ("Id", "CreatorId", "Title", "Type", "TargetValue", "StartDate", "EndDate",
                 "Status", "TargetType", "IsDefault", "IconName", "CreatedAt")
                VALUES
                (gen_random_uuid(), $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)
            `, [
                creatorId,
                challenge.title,
                challenge.type,
                challenge.targetValue,
                now,
                endDate,
                'Ativo',
                0, // AllUsers
                true,
                challenge.iconName,
                now
            ]);

            console.log(`  ✓ Added: ${challenge.title} (${challenge.type}, Target: ${challenge.targetValue})`);
            added++;
        }

        console.log(`\n✓ Successfully added ${added} new challenges!\n`);

        // Now get total count
        const total = await client.query(`
            SELECT COUNT(*) FROM "Challenges" WHERE "IsDefault" = true
        `);

        console.log(`Total default challenges in database: ${total.rows[0].count}\n`);

    } catch (error) {
        console.error('Error:', error.message);
        console.error(error.stack);
    } finally {
        await client.end();
    }
}

seedChallenges();
