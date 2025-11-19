const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function assignChallenges() {
    const client = new Client({
        connectionString: pgConnectionString,
        ssl: {
            rejectUnauthorized: false
        }
    });

    try {
        await client.connect();
        console.log('✓ Connected to database\n');

        // Get all default challenges
        const challenges = await client.query(`
            SELECT "Id", "Title", "TargetType"
            FROM "Challenges"
            WHERE "IsDefault" = true
            ORDER BY "Title"
        `);

        console.log(`Found ${challenges.rows.length} default challenges\n`);

        // Get all users
        const users = await client.query(`
            SELECT "Id", "Name", "Role"
            FROM "Users"
            ORDER BY "CreatedAt" DESC
        `);

        console.log(`Found ${users.rows.length} users\n`);

        // Get existing challenge progresses
        const existingProgresses = await client.query(`
            SELECT "ParticipantId", "ChallengeId"
            FROM "ChallengeProgresses"
        `);

        // Create a Set of existing combinations
        const existingSet = new Set(
            existingProgresses.rows.map(p => `${p.ParticipantId}_${p.ChallengeId}`)
        );

        console.log(`Found ${existingProgresses.rows.length} existing progress records\n`);
        console.log('Assigning challenges to users...\n');

        let totalAssigned = 0;
        let usersProcessed = 0;

        for (const user of users.rows) {
            let userAssignments = 0;

            for (const challenge of challenges.rows) {
                // Check if challenge is applicable to this user
                const isApplicable =
                    challenge.TargetType === 0 || // AllUsers
                    (challenge.TargetType === 1 && user.Role === 'PersonalTrainer'); // AllTrainers

                const progressKey = `${user.Id}_${challenge.Id}`;

                // If applicable and doesn't exist yet, create it
                if (isApplicable && !existingSet.has(progressKey)) {
                    await client.query(`
                        INSERT INTO "ChallengeProgresses"
                        ("Id", "ChallengeId", "ParticipantId", "CurrentValue", "LastUpdate", "CreatedAt")
                        VALUES
                        (gen_random_uuid(), $1, $2, 0, $3, $3)
                    `, [challenge.Id, user.Id, new Date()]);

                    userAssignments++;
                    totalAssigned++;
                    existingSet.add(progressKey); // Add to set to avoid duplicates
                }
            }

            if (userAssignments > 0) {
                console.log(`  ✓ ${user.Name}: assigned ${userAssignments} challenges`);
            }
            usersProcessed++;
        }

        console.log(`\n✓ Successfully assigned ${totalAssigned} challenge progress records!`);
        console.log(`✓ Processed ${usersProcessed} users\n`);

        // Final count
        const finalCount = await client.query('SELECT COUNT(*) FROM "ChallengeProgresses"');
        console.log(`Total challenge progress records in database: ${finalCount.rows[0].count}\n`);

    } catch (error) {
        console.error('Error:', error.message);
        console.error(error.stack);
    } finally {
        await client.end();
    }
}

assignChallenges();
