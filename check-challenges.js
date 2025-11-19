const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function checkChallenges() {
    const client = new Client({
        connectionString: pgConnectionString,
        ssl: {
            rejectUnauthorized: false
        }
    });

    try {
        await client.connect();
        console.log('✓ Connected to database\n');

        // Check existing default challenges
        const result = await client.query(`
            SELECT
                "Id",
                "Title",
                "Type",
                "TargetValue",
                "IsDefault"
            FROM "Challenges"
            WHERE "IsDefault" = true
            ORDER BY "Type", "TargetValue";
        `);

        console.log(`Found ${result.rows.length} default challenges:\n`);

        let currentType = '';
        result.rows.forEach(row => {
            if (row.Type !== currentType) {
                currentType = row.Type;
                console.log(`\n${currentType}:`);
            }
            console.log(`  - ${row.Title} (Target: ${row.TargetValue})`);
        });

        // Count total users
        const userCount = await client.query('SELECT COUNT(*) FROM "Users"');
        console.log(`\n\nTotal users in database: ${userCount.rows[0].count}`);

        // Count challenge progress records
        const progressCount = await client.query('SELECT COUNT(*) FROM "ChallengeProgresses"');
        console.log(`Total challenge progress records: ${progressCount.rows[0].count}\n`);

    } catch (error) {
        console.error('Error:', error.message);
    } finally {
        await client.end();
    }
}

checkChallenges();
