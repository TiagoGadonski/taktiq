const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function testUserDeletion() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });
  
  try {
    await client.connect();
    console.log('✓ Connected to database\n');
    
    // First, check if Users table has PreferredWorkoutLocation column
    const columnCheck = await client.query(`
      SELECT column_name, data_type, is_nullable
      FROM information_schema.columns
      WHERE table_name = 'Users'
      AND column_name IN ('PreferredWorkoutLocation')
    `);
    
    console.log('PreferredWorkoutLocation column exists:', columnCheck.rows.length > 0);
    if (columnCheck.rows.length > 0) {
      console.log('Column details:', columnCheck.rows[0]);
    }
    console.log('');
    
    // Create a test user
    console.log('Creating test user...');
    const createResult = await client.query(`
      INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt", "PreferredWorkoutLocation")
      VALUES (gen_random_uuid(), 'Test User Delete', 'test-delete@example.com', 'hash', 'Aluno', true, NOW(), 0)
      RETURNING "Id", "Name", "Email"
    `);
    
    const testUser = createResult.rows[0];
    console.log('✓ Created test user:', testUser);
    console.log('');
    
    // Try to delete the user
    console.log('Attempting to delete user...');
    try {
      await client.query(`DELETE FROM "Users" WHERE "Id" = $1`, [testUser.Id]);
      console.log('✓ User deleted successfully!');
      console.log('No issues with user deletion.');
    } catch (deleteError) {
      console.error('✗ Failed to delete user!');
      console.error('Error code:', deleteError.code);
      console.error('Error message:', deleteError.message);
      console.error('Error detail:', deleteError.detail);
      
      // Check what's blocking the deletion
      console.log('\nChecking for related records...');
      
      const activityLogs = await client.query(`
        SELECT COUNT(*) as count FROM "UserActivityLogs" WHERE "UserId" = $1
      `, [testUser.Id]);
      console.log('- UserActivityLogs:', activityLogs.rows[0].count);
      
      const friendships = await client.query(`
        SELECT COUNT(*) as count FROM "Friendships" WHERE "RequesterId" = $1 OR "AddresseeId" = $1
      `, [testUser.Id]);
      console.log('- Friendships:', friendships.rows[0].count);
      
      const notifications = await client.query(`
        SELECT COUNT(*) as count FROM "Notifications" WHERE "UserId" = $1
      `, [testUser.Id]);
      console.log('- Notifications:', notifications.rows[0].count);
      
      // Show what constraint is causing the issue
      console.log('\nConstraint causing issue:', deleteError.constraint);
    }
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await client.end();
  }
}

testUserDeletion();
