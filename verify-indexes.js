const { Client } = require('pg');

const connectionString = "Host=taktiq-db.postgres.database.azure.com;Port=5432;Database=postgres;Username=tasktiqadmin;Password=W3rt4juk;SslMode=Require;Trust Server Certificate=true";

// Convert .NET connection string to pg format
const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function verifyIndexes() {
    const client = new Client({
        connectionString: pgConnectionString,
        ssl: {
            rejectUnauthorized: false
        }
    });

    try {
        await client.connect();
        console.log('✓ Connected to database\n');

        // Query to get all indexes for our tables
        const result = await client.query(`
            SELECT
                schemaname,
                tablename,
                indexname,
                indexdef
            FROM pg_indexes
            WHERE tablename IN ('Friendships', 'Notifications', 'WorkoutPlans', 'ChallengeProgresses', 'ProgressMetrics', 'WorkoutSessions', 'WorkoutSets', 'Challenges')
            ORDER BY tablename, indexname;
        `);

        console.log('Database Indexes Created:');
        console.log('========================\n');

        let currentTable = '';
        result.rows.forEach(row => {
            if (row.tablename !== currentTable) {
                currentTable = row.tablename;
                console.log(`\n${currentTable}:`);
            }
            console.log(`  - ${row.indexname}`);
        });

        console.log('\n\nTotal indexes found:', result.rows.length);

        // Check specifically for our new indexes
        const newIndexes = [
            'IX_Friendships_RequesterAddressee',
            'IX_Friendships_RequesterId',
            'IX_Friendships_AddresseeId',
            'IX_Friendships_AddresseeStatus',
            'IX_Notifications_UserId',
            'IX_Notifications_UserReadCreated',
            'IX_WorkoutPlans_OwnerActive',
            'IX_ChallengeProgress_ParticipantId',
            'IX_ChallengeProgress_ChallengeId',
            'IX_ProgressMetrics_OwnerId',
            'IX_ProgressMetrics_OwnerDate'
        ];

        console.log('\n\nPerformance Optimization Indexes:');
        console.log('==================================');

        const foundIndexNames = result.rows.map(r => r.indexname);
        newIndexes.forEach(indexName => {
            const found = foundIndexNames.includes(indexName);
            console.log(`  ${found ? '✓' : '✗'} ${indexName}`);
        });

    } catch (error) {
        console.error('Error:', error.message);
    } finally {
        await client.end();
    }
}

verifyIndexes();
