const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function applyFix() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });

  try {
    await client.connect();
    console.log('✓ Connected to database\n');

    // Check if migration already applied
    const migrationCheck = await client.query(`
      SELECT "MigrationId" FROM "__EFMigrationsHistory"
      WHERE "MigrationId" = '20251114114209_FixUserActivityLogsForeignKey'
    `);

    if (migrationCheck.rows.length > 0) {
      console.log('✓ Migration already applied. Skipping.');
      return;
    }

    console.log('Applying fix for UserActivityLogs foreign key...\n');

    // Start transaction
    await client.query('BEGIN');

    try {
      // Drop the existing foreign key constraint
      console.log('1. Dropping existing FK constraint...');
      await client.query(`
        ALTER TABLE "UserActivityLogs"
        DROP CONSTRAINT IF EXISTS "FK_UserActivityLogs_Users_UserId"
      `);
      console.log('   ✓ FK constraint dropped');

      // Recreate the foreign key with ON DELETE SET NULL behavior
      console.log('2. Creating new FK constraint with ON DELETE SET NULL...');
      await client.query(`
        ALTER TABLE "UserActivityLogs"
        ADD CONSTRAINT "FK_UserActivityLogs_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id")
        ON DELETE SET NULL
      `);
      console.log('   ✓ FK constraint created with ON DELETE SET NULL');

      // Add migration record
      console.log('3. Recording migration...');
      await client.query(`
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251114114209_FixUserActivityLogsForeignKey', '9.0.0')
      `);
      console.log('   ✓ Migration recorded');

      // Commit transaction
      await client.query('COMMIT');
      console.log('\n✓ Fix applied successfully!\n');

      // Verify the fix
      console.log('Verifying fix...');
      const constraintInfo = await client.query(`
        SELECT
          tc.constraint_name,
          tc.table_name,
          kcu.column_name,
          ccu.table_name AS foreign_table_name,
          ccu.column_name AS foreign_column_name,
          rc.delete_rule
        FROM information_schema.table_constraints AS tc
        JOIN information_schema.key_column_usage AS kcu
          ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage AS ccu
          ON ccu.constraint_name = tc.constraint_name
        JOIN information_schema.referential_constraints AS rc
          ON rc.constraint_name = tc.constraint_name
        WHERE tc.constraint_name = 'FK_UserActivityLogs_Users_UserId'
      `);

      if (constraintInfo.rows.length > 0) {
        const info = constraintInfo.rows[0];
        console.log('✓ Constraint verified:');
        console.log(`  - Name: ${info.constraint_name}`);
        console.log(`  - Table: ${info.table_name}`);
        console.log(`  - Column: ${info.column_name}`);
        console.log(`  - References: ${info.foreign_table_name}(${info.foreign_column_name})`);
        console.log(`  - ON DELETE: ${info.delete_rule.toUpperCase()}`);
      }

    } catch (error) {
      await client.query('ROLLBACK');
      throw error;
    }

  } catch (error) {
    console.error('Error:', error.message);
    throw error;
  } finally {
    await client.end();
  }
}

applyFix();
