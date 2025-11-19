const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function testUserDeletionWithLogs() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });
  
  try {
    await client.connect();
    console.log('✓ Connected to database\n');
    
    // Create a test user
    console.log('Creating test user...');
    const createResult = await client.query(`
      INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt", "PreferredWorkoutLocation")
      VALUES (gen_random_uuid(), 'Test User With Logs', 'test-with-logs@example.com', 'hash', 'Aluno', true, NOW(), 0)
      RETURNING "Id", "Name", "Email"
    `);
    
    const testUser = createResult.rows[0];
    console.log('✓ Created test user:', testUser);
    console.log('');
    
    // Add some activity logs for this user
    console.log('Adding activity logs...');
    await client.query(`
      INSERT INTO "UserActivityLogs" ("Id", "UserId", "Action", "HttpMethod", "Endpoint", "Timestamp", "CreatedAt")
      VALUES 
        (gen_random_uuid(), $1, 'Login', 'POST', '/api/auth/login', NOW(), NOW()),
        (gen_random_uuid(), $1, 'ViewProfile', 'GET', '/api/me', NOW(), NOW()),
        (gen_random_uuid(), $1, 'CreateWorkout', 'POST', '/api/workouts', NOW(), NOW())
    `, [testUser.Id]);
    console.log('✓ Added 3 activity logs');
    console.log('');
    
    // Verify logs were created
    const logsCheck = await client.query(`
      SELECT COUNT(*) as count FROM "UserActivityLogs" WHERE "UserId" = $1
    `, [testUser.Id]);
    console.log('Activity logs count:', logsCheck.rows[0].count);
    console.log('');
    
    // Try to delete the user
    console.log('Attempting to delete user...');
    try {
      await client.query(`DELETE FROM "Users" WHERE "Id" = $1`, [testUser.Id]);
      console.log('✓ User deleted successfully!');
      
      // Check if logs still exist
      const remainingLogs = await client.query(`
        SELECT COUNT(*) as count FROM "UserActivityLogs" WHERE "UserId" = $1
      `, [testUser.Id]);
      console.log('Remaining activity logs:', remainingLogs.rows[0].count);
      
      // Check for orphaned logs
      const orphanedLogs = await client.query(`
        SELECT COUNT(*) as count FROM "UserActivityLogs" WHERE "UserId" IS NULL
      `);
      console.log('Total orphaned logs (UserId IS NULL):', orphanedLogs.rows[0].count);
      
    } catch (deleteError) {
      console.error('✗ Failed to delete user!');
      console.error('Error code:', deleteError.code);
      console.error('Error message:', deleteError.message);
      console.error('Error detail:', deleteError.detail);
      console.error('Constraint:', deleteError.constraint);
      
      // Clean up the test user
      await client.query(`DELETE FROM "UserActivityLogs" WHERE "UserId" = $1`, [testUser.Id]);
      await client.query(`DELETE FROM "Users" WHERE "Id" = $1`, [testUser.Id]);
      console.log('\n✓ Test data cleaned up');
    }
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await client.end();
  }
}

testUserDeletionWithLogs();
