const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function checkMigrationStatus() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });

  try {
    await client.connect();
    console.log('✓ Connected to Azure database\n');

    // Check which migrations are applied
    const migrations = await client.query(`
      SELECT "MigrationId", "ProductVersion"
      FROM "__EFMigrationsHistory"
      ORDER BY "MigrationId"
    `);

    console.log(`Applied migrations (${migrations.rows.length} total):\n`);
    migrations.rows.forEach((row, index) => {
      console.log(`${index + 1}. ${row.MigrationId}`);
    });

    // Check if our fix migration is applied
    const ourMigration = migrations.rows.find(
      m => m.MigrationId === '20251114114209_FixUserActivityLogsForeignKey'
    );

    console.log('\n─────────────────────────────────────────────');
    if (ourMigration) {
      console.log('✓ FixUserActivityLogsForeignKey migration is APPLIED');
    } else {
      console.log('✗ FixUserActivityLogsForeignKey migration is NOT applied');
    }
    console.log('─────────────────────────────────────────────\n');

  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await client.end();
  }
}

checkMigrationStatus();
