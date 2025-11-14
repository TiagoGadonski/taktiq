const { Client } = require('pg');

const pgConnectionString = "postgresql://tasktiqadmin:W3rt4juk@taktiq-db.postgres.database.azure.com:5432/postgres?sslmode=require";

// Comprehensive list of home/bodyweight exercises
const homeExercises = [
  // CHEST - Home
  { Name: 'Flexão de Braço', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Flexão Diamante', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Flexão Declinada', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Flexão Inclinada', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Flexão Archer', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Flexão Hindu', MuscleGroup: 'Peito', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both

  // BACK - Home
  { Name: 'Remada Invertida', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Superman', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Prancha Reversa', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Extensão Lombar', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Bird Dog', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Ponte de Glúteos', MuscleGroup: 'Costas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both

  // LEGS - Home
  { Name: 'Agachamento Livre', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Agachamento Pistol', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Agachamento Búlgaro', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Afundo', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Afundo Reverso', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Afundo Lateral', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Elevação de Panturrilha', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Ponte de Glúteos Uma Perna', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Agachamento Sumô', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Step Up', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Wall Sit', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both

  // SHOULDERS - Home
  { Name: 'Flexão Pike', MuscleGroup: 'Ombros', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Prancha Lateral', MuscleGroup: 'Ombros', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Handstand Push-up', MuscleGroup: 'Ombros', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 1 }, // Home only
  { Name: 'Elevação Lateral com Toalha', MuscleGroup: 'Ombros', Equipment: 'Toalha', Category: 'Força', WorkoutLocation: 1 }, // Home only

  // ARMS - Home
  { Name: 'Tríceps no Banco', MuscleGroup: 'Braços', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Tríceps Diamante', MuscleGroup: 'Braços', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Rosca Bíceps Isométrica', MuscleGroup: 'Braços', Equipment: 'Toalha', Category: 'Força', WorkoutLocation: 1 }, // Home only
  { Name: 'Flexão Fechada', MuscleGroup: 'Braços', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both

  // ABS/CORE - Home
  { Name: 'Prancha', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Prancha Lateral', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Abdominal Tradicional', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Abdominal Bicicleta', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Elevação de Pernas', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Prancha com Toque no Ombro', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Mountain Climbers', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Russian Twist', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'V-Up', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Dead Bug', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Hollow Body Hold', MuscleGroup: 'Abdômen', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both

  // CARDIO - Home
  { Name: 'Burpees', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Polichinelos', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'High Knees', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Jumping Jacks', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Corrida Estacionária', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Pular Corda (sem corda)', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Skaters', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
  { Name: 'Box Jumps (sem caixa)', MuscleGroup: 'Pernas', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both

  // FULL BODY - Home
  { Name: 'Inchworms', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Bear Crawl', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Crab Walk', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Força', WorkoutLocation: 2 }, // Both
  { Name: 'Sprawls', MuscleGroup: 'Corpo Todo', Equipment: 'Peso Corporal', Category: 'Cardio', WorkoutLocation: 2 }, // Both
];

async function populateHomeExercises() {
  const client = new Client({
    connectionString: pgConnectionString,
    ssl: {
      rejectUnauthorized: false
    }
  });

  try {
    await client.connect();
    console.log('✓ Connected to database\n');

    console.log(`Adding ${homeExercises.length} home/bodyweight exercises...\n`);

    let added = 0;
    let skipped = 0;
    let updated = 0;

    for (const exercise of homeExercises) {
      // Check if exercise already exists
      const existing = await client.query(
        `SELECT "Id", "WorkoutLocation" FROM "Exercises" WHERE "Name" = $1`,
        [exercise.Name]
      );

      if (existing.rows.length > 0) {
        // Update existing exercise if it's currently Gym-only
        if (existing.rows[0].WorkoutLocation === 0) {
          await client.query(
            `UPDATE "Exercises"
             SET "WorkoutLocation" = $1,
                 "Equipment" = $2,
                 "Category" = $3
             WHERE "Id" = $4`,
            [exercise.WorkoutLocation, exercise.Equipment, exercise.Category, existing.rows[0].Id]
          );
          console.log(`✓ Updated: ${exercise.Name} (${exercise.MuscleGroup})`);
          updated++;
        } else {
          console.log(`  Skipped: ${exercise.Name} (already exists)`);
          skipped++;
        }
      } else {
        // Add new exercise
        await client.query(
          `INSERT INTO "Exercises" ("Id", "Name", "MuscleGroup", "Equipment", "Category", "WorkoutLocation", "CreatedAt")
           VALUES (gen_random_uuid(), $1, $2, $3, $4, $5, NOW())`,
          [exercise.Name, exercise.MuscleGroup, exercise.Equipment, exercise.Category, exercise.WorkoutLocation]
        );
        console.log(`✓ Added: ${exercise.Name} (${exercise.MuscleGroup})`);
        added++;
      }
    }

    console.log('\n─────────────────────────────────────');
    console.log('Summary:');
    console.log(`  Added: ${added}`);
    console.log(`  Updated: ${updated}`);
    console.log(`  Skipped: ${skipped}`);
    console.log('─────────────────────────────────────\n');

    // Show final counts by location
    const locationCounts = await client.query(`
      SELECT
        "WorkoutLocation",
        CASE
          WHEN "WorkoutLocation" = 0 THEN 'Gym'
          WHEN "WorkoutLocation" = 1 THEN 'Home'
          WHEN "WorkoutLocation" = 2 THEN 'Both'
        END as location_name,
        COUNT(*) as count
      FROM "Exercises"
      GROUP BY "WorkoutLocation"
      ORDER BY "WorkoutLocation"
    `);

    console.log('Final exercise distribution:');
    locationCounts.rows.forEach(row => {
      console.log(`  ${row.location_name}: ${row.count} exercises`);
    });

  } catch (error) {
    console.error('Error:', error.message);
    throw error;
  } finally {
    await client.end();
  }
}

populateHomeExercises();
