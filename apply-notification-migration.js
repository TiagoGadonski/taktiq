const { Client } = require('pg');

async function applyMigration() {
  const client = new Client({
    host: 'taktiq-db.postgres.database.azure.com',
    database: 'taktiq-production',
    user: 'taktiqadmin',
    password: 'W8Wk9M#kLpQv2$nR',
    port: 5432,
    ssl: { rejectUnauthorized: false }
  });

  try {
    await client.connect();
    console.log('Connected to database');

    // First check if migration already exists
    const checkMigration = await client.query(`
      SELECT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20251108193340_AddNotificationSystem'
      );
    `);

    if (checkMigration.rows[0].exists) {
      console.log('Migration already applied!');
      return;
    }

    console.log('Applying notification system migration...');

    // Apply the migration
    await client.query(`
      CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Type" text NOT NULL,
        "Title" text NOT NULL,
        "Message" text NOT NULL,
        "Data" text,
        "ActionUrl" text,
        "IsRead" boolean NOT NULL,
        "ReadAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
      );
    `);
    console.log('Created Notifications table');

    await client.query(`
      CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
    `);
    console.log('Created index on UserId');

    await client.query(`
      INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
      VALUES ('20251108193340_AddNotificationSystem', '8.0.4');
    `);
    console.log('Updated migration history');

    console.log('✅ Migration applied successfully!');

  } catch (err) {
    console.error('Error:', err.message);
    console.error('Stack:', err.stack);
  } finally {
    await client.end();
  }
}

applyMigration();
