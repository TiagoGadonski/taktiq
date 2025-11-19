const { Client } = require('pg');

async function checkNotifications() {
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

    // Check if Notifications table exists
    const tableCheck = await client.query(`
      SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_name = 'Notifications'
      );
    `);
    console.log('Notifications table exists:', tableCheck.rows[0].exists);

    if (tableCheck.rows[0].exists) {
      // Count notifications
      const countResult = await client.query('SELECT COUNT(*) FROM "Notifications"');
      console.log('Total notifications:', countResult.rows[0].count);

      // Get recent notifications
      const recentNotifications = await client.query(`
        SELECT "Id", "UserId", "Type", "Title", "IsRead", "CreatedAt"
        FROM "Notifications"
        ORDER BY "CreatedAt" DESC
        LIMIT 10
      `);
      console.log('Recent notifications:', JSON.stringify(recentNotifications.rows, null, 2));
    }

  } catch (err) {
    console.error('Error:', err.message);
  } finally {
    await client.end();
  }
}

checkNotifications();
