-- Migration: AddHealthConditionsAndExerciseGoalToUser
-- This script adds the HealthConditions and ExerciseGoal columns to the Users table

-- Add HealthConditions column
ALTER TABLE "Users"
ADD COLUMN "HealthConditions" text NULL;

-- Add ExerciseGoal column
ALTER TABLE "Users"
ADD COLUMN "ExerciseGoal" text NULL;

-- Verify the migration
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Users'
AND column_name IN ('HealthConditions', 'ExerciseGoal');
