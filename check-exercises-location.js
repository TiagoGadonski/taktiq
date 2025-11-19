const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

async function checkExercises() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });

  try {
    await client.connect();
    console.log('✓ Connected to database\n');

    // Check if WorkoutLocation column exists
    const columnCheck = await client.query(`
      SELECT column_name, data_type, is_nullable
      FROM information_schema.columns
      WHERE table_name = 'Exercises'
      AND column_name = 'WorkoutLocation'
    `);

    console.log('WorkoutLocation column exists:', columnCheck.rows.length > 0);
    if (columnCheck.rows.length > 0) {
      console.log('Column details:', columnCheck.rows[0]);
    }
    console.log('');

    // Count exercises by WorkoutLocation
    const locationCounts = await client.query(`
      SELECT
        "WorkoutLocation",
        CASE
          WHEN "WorkoutLocation" = 0 THEN 'Gym'
          WHEN "WorkoutLocation" = 1 THEN 'Home'
          WHEN "WorkoutLocation" = 2 THEN 'Both'
          ELSE 'Unknown'
        END as location_name,
        COUNT(*) as count
      FROM "Exercises"
      GROUP BY "WorkoutLocation"
      ORDER BY "WorkoutLocation"
    `);

    console.log('Exercises by location:');
    locationCounts.rows.forEach(row => {
      console.log(`  ${row.location_name} (${row.WorkoutLocation}): ${row.count} exercises`);
    });
    console.log('');

    // Show sample Home exercises
    const homeExercises = await client.query(`
      SELECT "Name", "MuscleGroup", "Equipment"
      FROM "Exercises"
      WHERE "WorkoutLocation" = 1
      LIMIT 10
    `);

    console.log('Sample Home exercises:');
    homeExercises.rows.forEach(ex => {
      console.log(`  - ${ex.Name} (${ex.MuscleGroup}) - Equipment: ${ex.Equipment || 'None'}`);
    });
    console.log('');

    // Show sample Both exercises
    const bothExercises = await client.query(`
      SELECT "Name", "MuscleGroup", "Equipment"
      FROM "Exercises"
      WHERE "WorkoutLocation" = 2
      LIMIT 10
    `);

    console.log('Sample Both (Gym+Home) exercises:');
    bothExercises.rows.forEach(ex => {
      console.log(`  - ${ex.Name} (${ex.MuscleGroup}) - Equipment: ${ex.Equipment || 'None'}`);
    });
    console.log('');

    // Check user preferences
    const userPrefs = await client.query(`
      SELECT
        "PreferredWorkoutLocation",
        CASE
          WHEN "PreferredWorkoutLocation" = 0 THEN 'Gym'
          WHEN "PreferredWorkoutLocation" = 1 THEN 'Home'
          WHEN "PreferredWorkoutLocation" = 2 THEN 'Both'
        END as preference_name,
        COUNT(*) as count
      FROM "Users"
      GROUP BY "PreferredWorkoutLocation"
      ORDER BY "PreferredWorkoutLocation"
    `);

    console.log('User workout location preferences:');
    userPrefs.rows.forEach(row => {
      console.log(`  ${row.preference_name} (${row.PreferredWorkoutLocation}): ${row.count} users`);
    });

  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await client.end();
  }
}

checkExercises();
