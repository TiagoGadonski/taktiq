const axios = require('axios');

// Configuration
const API_BASE_URL = 'https://localhost:7219/api';
const API_KEY = 'your-api-key-here';

// Workout Location enum: 0 = Gym, 1 = Home, 2 = Both
const WorkoutLocation = {
  Gym: 0,
  Home: 1,
  Both: 2
};

// Home and Calisthenics Exercises
const homeExercises = [
  // PUSH EXERCISES (Chest, Shoulders, Triceps)
  {
    name: 'Push-ups',
    muscleGroup: 'chest',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Classic bodyweight exercise for chest, shoulders, and triceps. Keep core tight and body in straight line.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Wide Push-ups',
    muscleGroup: 'chest',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Wider hand placement emphasizes chest more. Great for building pec strength.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Diamond Push-ups',
    muscleGroup: 'triceps',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Hands close together in diamond shape. Excellent for triceps development.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Decline Push-ups',
    muscleGroup: 'chest',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Feet elevated on chair or couch. Targets upper chest and shoulders.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Pike Push-ups',
    muscleGroup: 'shoulders',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Hips high in air, pike position. Excellent shoulder builder, progression to handstand push-ups.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Handstand Push-ups (Wall)',
    muscleGroup: 'shoulders',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Advanced shoulder exercise against wall. Build incredible shoulder and upper body strength.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Tricep Dips (Chair)',
    muscleGroup: 'triceps',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Using chair or couch edge. Excellent for triceps, can add weight on lap for progression.',
    workoutLocation: WorkoutLocation.Home
  },

  // PULL EXERCISES (Back, Biceps)
  {
    name: 'Pull-ups (Doorway Bar)',
    muscleGroup: 'lats',
    category: 'strength',
    equipment: 'pull-up bar',
    notes: 'King of bodyweight back exercises. Use doorway pull-up bar. Palms facing away.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Chin-ups (Doorway Bar)',
    muscleGroup: 'lats',
    category: 'strength',
    equipment: 'pull-up bar',
    notes: 'Palms facing towards you. Great for lats and biceps development.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Australian Pull-ups (Table)',
    muscleGroup: 'back',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Horizontal pulling using sturdy table. Feet on ground, pull chest to table edge. Great back builder.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Towel Rows',
    muscleGroup: 'back',
    category: 'strength',
    equipment: 'towel',
    notes: 'Wrap towel around door handle or sturdy post. Pull towel rowing motion. Works lats and rhomboids.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Superman Holds',
    muscleGroup: 'lower back',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Lying face down, lift arms and legs off ground. Excellent for lower back and posterior chain.',
    workoutLocation: WorkoutLocation.Home
  },

  // LEGS EXERCISES
  {
    name: 'Bodyweight Squats',
    muscleGroup: 'quadriceps',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Foundation leg exercise. Feet shoulder width, sit back and down. Keep chest up.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Bulgarian Split Squats',
    muscleGroup: 'quadriceps',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Rear foot elevated on chair or couch. Excellent single-leg strength and balance.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Lunges',
    muscleGroup: 'quadriceps',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Walking or stationary lunges. Great for legs and glutes, improves balance.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Jump Squats',
    muscleGroup: 'quadriceps',
    category: 'plyometrics',
    equipment: 'bodyweight',
    notes: 'Explosive squat jumps. Builds power and burns calories. Great for athleticism.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Single-Leg Romanian Deadlifts',
    muscleGroup: 'hamstrings',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Balance on one leg, hinge at hips. Excellent for hamstrings, glutes, and balance.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Glute Bridges',
    muscleGroup: 'glutes',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Lying on back, lift hips up. Essential glute activation and strength exercise.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Single-Leg Glute Bridges',
    muscleGroup: 'glutes',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'One leg version for increased difficulty. Builds unilateral glute strength.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Calf Raises',
    muscleGroup: 'calves',
    category: 'strength',
    equipment: 'bodyweight',
    notes: 'Stand on edge of step or book, raise up on toes. Can do single-leg for progression.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Wall Sits',
    muscleGroup: 'quadriceps',
    category: 'isometric',
    equipment: 'bodyweight',
    notes: 'Back against wall, slide down to 90 degree squat. Hold position. Burns quads intensely.',
    workoutLocation: WorkoutLocation.Home
  },

  // CORE EXERCISES
  {
    name: 'Plank',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Hold push-up position on forearms. King of core exercises. Builds incredible core strength.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Side Plank',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Lateral plank on one forearm. Excellent for obliques and lateral core stability.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Mountain Climbers',
    muscleGroup: 'abdominals',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Push-up position, drive knees to chest alternating. Great cardio and core workout.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Bicycle Crunches',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Lying on back, pedaling motion with opposite elbow to knee. Excellent for obliques.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Leg Raises',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Lying flat, raise straight legs up. Advanced lower ab exercise.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flutter Kicks',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Small alternating leg kicks while lying down. Burns lower abs intensely.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Russian Twists',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Seated position, twist side to side. Can hold book or water bottle for resistance. Great for obliques.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Dead Bug',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'On back, alternate extending opposite arm and leg. Excellent core stability exercise.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Hollow Body Hold',
    muscleGroup: 'abdominals',
    category: 'core',
    equipment: 'bodyweight',
    notes: 'Advanced core hold position. Builds incredible core strength for gymnastics and calisthenics.',
    workoutLocation: WorkoutLocation.Home
  },

  // CARDIO & FULL BODY
  {
    name: 'Burpees',
    muscleGroup: 'full body',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Full body explosive movement. Drop down, push-up, jump up. Ultimate conditioning exercise.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Jumping Jacks',
    muscleGroup: 'full body',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Classic cardio exercise. Great for warm-up or high-intensity intervals.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'High Knees',
    muscleGroup: 'full body',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Running in place with knees high. Excellent cardio and leg conditioning.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Butt Kicks',
    muscleGroup: 'hamstrings',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Running in place kicking heels to glutes. Great hamstring and cardio work.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Inchworms',
    muscleGroup: 'full body',
    category: 'mobility',
    equipment: 'bodyweight',
    notes: 'Walk hands out to plank, walk feet to hands. Great dynamic stretch and core workout.',
    workoutLocation: WorkoutLocation.Home
  },

  // EXERCISES THAT WORK BOTH GYM AND HOME
  {
    name: 'Jumping Rope',
    muscleGroup: 'full body',
    category: 'cardio',
    equipment: 'jump rope',
    notes: 'Excellent cardio workout. Improves coordination, burns calories, builds calf endurance.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Shadow Boxing',
    muscleGroup: 'full body',
    category: 'cardio',
    equipment: 'bodyweight',
    notes: 'Throwing punches in air with movement. Great cardio and upper body conditioning.',
    workoutLocation: WorkoutLocation.Both
  }
];

async function seedExercises() {
  console.log('🏋️ Starting to seed home/calisthenics exercises...\n');

  let successCount = 0;
  let errorCount = 0;

  for (const exercise of homeExercises) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/exercises`,
        exercise,
        {
          headers: {
            'Content-Type': 'application/json',
            // Add authorization header if needed
            // 'Authorization': `Bearer ${API_KEY}`
          },
          // Accept self-signed certificates for development
          httpsAgent: new (require('https').Agent)({
            rejectUnauthorized: false
          })
        }
      );

      successCount++;
      console.log(`✅ Added: ${exercise.name} (${exercise.muscleGroup})`);

    } catch (error) {
      errorCount++;
      if (error.response) {
        console.error(`❌ Failed to add ${exercise.name}: ${error.response.data.message || error.response.statusText}`);
      } else {
        console.error(`❌ Failed to add ${exercise.name}: ${error.message}`);
      }
    }
  }

  console.log(`\n📊 Summary:`);
  console.log(`✅ Successfully added: ${successCount} exercises`);
  console.log(`❌ Failed: ${errorCount} exercises`);
  console.log(`📝 Total: ${homeExercises.length} exercises`);
}

// Run the seed function
seedExercises()
  .then(() => {
    console.log('\n🎉 Seeding completed!');
    process.exit(0);
  })
  .catch((error) => {
    console.error('\n💥 Seeding failed:', error.message);
    process.exit(1);
  });
