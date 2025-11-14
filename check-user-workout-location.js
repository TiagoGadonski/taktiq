const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function checkUserWorkoutLocation() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });

  try {
    await client.connect();
    console.log('✓ Connected to database\n');

    // Get all users and their workout location preferences
    const users = await client.query(`
      SELECT
        "Id",
        "Name",
        "Email",
        "PreferredWorkoutLocation",
        CASE
          WHEN "PreferredWorkoutLocation" = 0 THEN 'Gym'
          WHEN "PreferredWorkoutLocation" = 1 THEN 'Home'
          WHEN "PreferredWorkoutLocation" = 2 THEN 'Both'
          ELSE 'Unknown'
        END as location_name,
        "IsActive",
        "LastLoginAt"
      FROM "Users"
      WHERE "IsActive" = true
      ORDER BY "LastLoginAt" DESC NULLS LAST
      LIMIT 10
    `);

    console.log('Recent active users and their workout location preferences:\n');
    console.log('═══════════════════════════════════════════════════════════════');
    users.rows.forEach((user, index) => {
      console.log(`${index + 1}. ${user.Name} (${user.Email})`);
      console.log(`   Preferred Location: ${user.location_name} (${user.PreferredWorkoutLocation})`);
      console.log(`   Last Login: ${user.LastLoginAt ? new Date(user.LastLoginAt).toLocaleString() : 'Never'}`);
      console.log('───────────────────────────────────────────────────────────────');
    });

    // Check if there are any users with Home preference
    const homeUsers = await client.query(`
      SELECT COUNT(*) as count
      FROM "Users"
      WHERE "PreferredWorkoutLocation" = 1 AND "IsActive" = true
    `);

    console.log(`\nTotal active users with Home preference: ${homeUsers.rows[0].count}`);

    // Get distribution
    const distribution = await client.query(`
      SELECT
        "PreferredWorkoutLocation",
        CASE
          WHEN "PreferredWorkoutLocation" = 0 THEN 'Gym'
          WHEN "PreferredWorkoutLocation" = 1 THEN 'Home'
          WHEN "PreferredWorkoutLocation" = 2 THEN 'Both'
        END as location_name,
        COUNT(*) as count
      FROM "Users"
      WHERE "IsActive" = true
      GROUP BY "PreferredWorkoutLocation"
      ORDER BY "PreferredWorkoutLocation"
    `);

    console.log('\nWorkout location distribution:');
    distribution.rows.forEach(row => {
      console.log(`  ${row.location_name}: ${row.count} users`);
    });

  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await client.end();
  }
}

checkUserWorkoutLocation();
