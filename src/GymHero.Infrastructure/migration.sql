CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "Exercises" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "MuscleGroup" text NOT NULL,
        "Equipment" text,
        "Notes" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Exercises" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Email" character varying(256) NOT NULL,
        "PasswordHash" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "Badges" (
        "Id" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Title" text NOT NULL,
        "Description" text NOT NULL,
        "EarnedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Badges" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Badges_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "Challenges" (
        "Id" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        "Title" text NOT NULL,
        "Type" text NOT NULL,
        "TargetValue" double precision NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "Status" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Challenges" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Challenges_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "ProgressMetrics" (
        "Id" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        "Type" text NOT NULL,
        "Value" double precision NOT NULL,
        "Unit" text NOT NULL,
        "Date" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ProgressMetrics" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProgressMetrics_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "WorkoutPlans" (
        "Id" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        "Name" text NOT NULL,
        "Goal" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WorkoutPlans" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkoutPlans_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "ChallengeProgresses" (
        "Id" uuid NOT NULL,
        "ChallengeId" uuid NOT NULL,
        "CurrentValue" double precision NOT NULL,
        "LastUpdate" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ChallengeProgresses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ChallengeProgresses_Challenges_ChallengeId" FOREIGN KEY ("ChallengeId") REFERENCES "Challenges" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "WorkoutExercises" (
        "Id" uuid NOT NULL,
        "WorkoutPlanId" uuid NOT NULL,
        "ExerciseId" uuid NOT NULL,
        "Order" integer NOT NULL,
        "TargetSets" integer NOT NULL,
        "TargetReps" integer NOT NULL,
        "TargetLoad" double precision NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WorkoutExercises" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkoutExercises_Exercises_ExerciseId" FOREIGN KEY ("ExerciseId") REFERENCES "Exercises" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId" FOREIGN KEY ("WorkoutPlanId") REFERENCES "WorkoutPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "WorkoutSessions" (
        "Id" uuid NOT NULL,
        "WorkoutPlanId" uuid NOT NULL,
        "Date" timestamp with time zone NOT NULL,
        "Notes" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WorkoutSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId" FOREIGN KEY ("WorkoutPlanId") REFERENCES "WorkoutPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE TABLE "WorkoutSets" (
        "Id" uuid NOT NULL,
        "WorkoutSessionId" uuid NOT NULL,
        "ExerciseId" uuid NOT NULL,
        "SetNumber" integer NOT NULL,
        "Reps" integer NOT NULL,
        "Load" double precision NOT NULL,
        "Rpe" integer,
        "Completed" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WorkoutSets" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkoutSets_Exercises_ExerciseId" FOREIGN KEY ("ExerciseId") REFERENCES "Exercises" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_WorkoutSets_WorkoutSessions_WorkoutSessionId" FOREIGN KEY ("WorkoutSessionId") REFERENCES "WorkoutSessions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_Badges_OwnerId" ON "Badges" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ChallengeProgresses_ChallengeId" ON "ChallengeProgresses" ("ChallengeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_Challenges_OwnerId" ON "Challenges" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_ProgressMetrics_OwnerId" ON "ProgressMetrics" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutExercises_ExerciseId" ON "WorkoutExercises" ("ExerciseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutExercises_WorkoutPlanId" ON "WorkoutExercises" ("WorkoutPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutPlans_OwnerId" ON "WorkoutPlans" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutSessions_WorkoutPlanId" ON "WorkoutSessions" ("WorkoutPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutSets_ExerciseId" ON "WorkoutSets" ("ExerciseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    CREATE INDEX "IX_WorkoutSets_WorkoutSessionId" ON "WorkoutSets" ("WorkoutSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250827235446_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250827235446_InitialCreate', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250830042818_UpdateWorkoutSessionWithCompletionDate') THEN
    ALTER TABLE "WorkoutSessions" RENAME COLUMN "Date" TO "StartedAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250830042818_UpdateWorkoutSessionWithCompletionDate') THEN
    ALTER TABLE "WorkoutSessions" ADD "CompletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250830042818_UpdateWorkoutSessionWithCompletionDate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250830042818_UpdateWorkoutSessionWithCompletionDate', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Challenges" DROP CONSTRAINT "FK_Challenges_Users_OwnerId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    DROP INDEX "IX_ChallengeProgresses_ChallengeId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Challenges" RENAME COLUMN "OwnerId" TO "CreatorId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER INDEX "IX_Challenges_OwnerId" RENAME TO "IX_Challenges_CreatorId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Users" ADD "PersonalTrainerId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Users" ADD "Role" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "ChallengeProgresses" ADD "ParticipantId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    CREATE TABLE "BadgeDefinitions" (
        "Id" uuid NOT NULL,
        "Code" text NOT NULL,
        "Title" text NOT NULL,
        "Description" text NOT NULL,
        "TriggerType" text NOT NULL,
        "ThresholdValue" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_BadgeDefinitions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    CREATE INDEX "IX_Users_PersonalTrainerId" ON "Users" ("PersonalTrainerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    CREATE INDEX "IX_ChallengeProgresses_ChallengeId" ON "ChallengeProgresses" ("ChallengeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    CREATE INDEX "IX_ChallengeProgresses_ParticipantId" ON "ChallengeProgresses" ("ParticipantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "ChallengeProgresses" ADD CONSTRAINT "FK_ChallengeProgresses_Users_ParticipantId" FOREIGN KEY ("ParticipantId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Challenges" ADD CONSTRAINT "FK_Challenges_Users_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Users_PersonalTrainerId" FOREIGN KEY ("PersonalTrainerId") REFERENCES "Users" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250911003329_FinalizeAppFeaturesAndFixes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250911003329_FinalizeAppFeaturesAndFixes', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "Bio" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "DateOfBirth" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "Height" double precision;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "Location" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "ProfilePictureUrl" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    ALTER TABLE "Users" ADD "Weight" double precision;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    CREATE TABLE "Friendships" (
        "RequesterId" uuid NOT NULL,
        "AddresseeId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "Id" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Friendships" PRIMARY KEY ("RequesterId", "AddresseeId"),
        CONSTRAINT "FK_Friendships_Users_AddresseeId" FOREIGN KEY ("AddresseeId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Friendships_Users_RequesterId" FOREIGN KEY ("RequesterId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    CREATE INDEX "IX_Friendships_AddresseeId" ON "Friendships" ("AddresseeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913213709_AddFriendshipSystem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250913213709_AddFriendshipSystem', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913223508_AddCategoryToExercise') THEN
    ALTER TABLE "Exercises" ADD "Category" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250913223508_AddCategoryToExercise') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250913223508_AddCategoryToExercise', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250918142437_AddImageAndVideoUrlToExercise') THEN
    ALTER TABLE "Exercises" ADD "ImageUrl" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250918142437_AddImageAndVideoUrlToExercise') THEN
    ALTER TABLE "Exercises" ADD "VideoUrl" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250918142437_AddImageAndVideoUrlToExercise') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250918142437_AddImageAndVideoUrlToExercise', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017182930_AddGymNameAndPhoneNumberToUser') THEN
    ALTER TABLE "Users" ADD "GymName" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017182930_AddGymNameAndPhoneNumberToUser') THEN
    ALTER TABLE "Users" ADD "PhoneNumber" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017182930_AddGymNameAndPhoneNumberToUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251017182930_AddGymNameAndPhoneNumberToUser', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017231152_AddIsActiveToWorkoutPlan') THEN
    ALTER TABLE "WorkoutPlans" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251017231152_AddIsActiveToWorkoutPlan') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251017231152_AddIsActiveToWorkoutPlan', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" DROP CONSTRAINT "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" RENAME COLUMN "WorkoutPlanId" TO "WorkoutId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER INDEX "IX_WorkoutExercises_WorkoutPlanId" RENAME TO "IX_WorkoutExercises_WorkoutId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutPlans" ADD "Description" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutPlans" ADD "Duration" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD "Notes" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD "RestSeconds" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD "TargetRepsRange" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD "TargetRpe" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD "WorkoutPlanId1" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    CREATE TABLE "Workouts" (
        "Id" uuid NOT NULL,
        "PlanId" uuid NOT NULL,
        "Name" text NOT NULL,
        "DayOfWeek" integer,
        "Order" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Workouts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Workouts_WorkoutPlans_PlanId" FOREIGN KEY ("PlanId") REFERENCES "WorkoutPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN

                    INSERT INTO "Workouts" ("Id", "PlanId", "Name", "DayOfWeek", "Order", "CreatedAt")
                    SELECT
                        gen_random_uuid(),
                        wp."Id",
                        'Treino Completo',
                        NULL,
                        1,
                        NOW()
                    FROM "WorkoutPlans" wp
                    WHERE EXISTS (
                        SELECT 1 FROM "WorkoutExercises" we WHERE we."WorkoutId" = wp."Id"
                    );

                    -- Update all WorkoutExercises to point to the new default Workout
                    UPDATE "WorkoutExercises" we
                    SET "WorkoutId" = w."Id"
                    FROM "Workouts" w
                    WHERE w."PlanId" = we."WorkoutId";
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    CREATE INDEX "IX_WorkoutExercises_WorkoutPlanId1" ON "WorkoutExercises" ("WorkoutPlanId1");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    CREATE INDEX "IX_Workouts_PlanId" ON "Workouts" ("PlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD CONSTRAINT "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1" FOREIGN KEY ("WorkoutPlanId1") REFERENCES "WorkoutPlans" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    ALTER TABLE "WorkoutExercises" ADD CONSTRAINT "FK_WorkoutExercises_Workouts_WorkoutId" FOREIGN KEY ("WorkoutId") REFERENCES "Workouts" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018030650_AddWorkoutEntity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251018030650_AddWorkoutEntity', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018033316_AddUserIsActiveProperty') THEN
    ALTER TABLE "Users" ADD "IsActive" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018033316_AddUserIsActiveProperty') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251018033316_AddUserIsActiveProperty', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018203447_MakeWorkoutPlanIdOptionalInWorkoutSession') THEN
    ALTER TABLE "WorkoutSessions" DROP CONSTRAINT "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018203447_MakeWorkoutPlanIdOptionalInWorkoutSession') THEN
    ALTER TABLE "WorkoutSessions" ALTER COLUMN "WorkoutPlanId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018203447_MakeWorkoutPlanIdOptionalInWorkoutSession') THEN
    ALTER TABLE "WorkoutSessions" ADD CONSTRAINT "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId" FOREIGN KEY ("WorkoutPlanId") REFERENCES "WorkoutPlans" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251018203447_MakeWorkoutPlanIdOptionalInWorkoutSession') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251018203447_MakeWorkoutPlanIdOptionalInWorkoutSession', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    ALTER TABLE "WorkoutExercises" DROP CONSTRAINT "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    DROP INDEX "IX_WorkoutExercises_WorkoutPlanId1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    ALTER TABLE "WorkoutExercises" DROP COLUMN "WorkoutPlanId1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    ALTER TABLE "WorkoutSets" ALTER COLUMN "Reps" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    ALTER TABLE "WorkoutSets" ALTER COLUMN "Load" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251023142448_MakeRepsAndLoadOptional') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251023142448_MakeRepsAndLoadOptional', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251025030418_AddChallengeTargetTypeAndIsDefault') THEN
    ALTER TABLE "Challenges" ADD "IsDefault" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251025030418_AddChallengeTargetTypeAndIsDefault') THEN
    ALTER TABLE "Challenges" ADD "TargetType" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251025030418_AddChallengeTargetTypeAndIsDefault') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251025030418_AddChallengeTargetTypeAndIsDefault', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "WorkoutPlans" ADD "AllowCopying" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "WorkoutPlans" ADD "IsPublic" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "WorkoutPlans" ADD "PublishedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "WorkoutPlans" ADD "ViewCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "WorkoutPlans" ADD "VisibilityLevel" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    ALTER TABLE "Challenges" ADD "IconName" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    CREATE TABLE "PasswordResetTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "IsUsed" boolean NOT NULL,
        "UsedAt" timestamp with time zone,
        CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PasswordResetTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    CREATE INDEX "IX_PasswordResetTokens_UserId" ON "PasswordResetTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251031194916_AddWorkoutPlanVisibility') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251031194916_AddWorkoutPlanVisibility', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101024441_AddDefaultChallenges') THEN

                    DO $$
                    DECLARE
                        admin_user_id UUID;
                    BEGIN
                        -- Get the ID of the first admin user
                        SELECT "Id" INTO admin_user_id
                        FROM "Users"
                        WHERE "Role" = 'Admin'
                        LIMIT 1;

                        -- Only insert challenges if an admin user exists
                        IF admin_user_id IS NOT NULL THEN
                            INSERT INTO "Challenges" ("Id", "CreatorId", "Title", "Type", "TargetValue", "StartDate", "EndDate", "Status", "TargetType", "IsDefault", "IconName", "CreatedAt")
                            VALUES
                            -- Beginner Challenges (Short-term, motivational)
                            ('11111111-1111-1111-1111-111111111111', admin_user_id, 'Primeira Semana de Treinos', 'WorkoutCount', 3, NOW(), NOW() + INTERVAL '30 days', 'Active', 0, true, 'flame', NOW()),
                            ('22222222-2222-2222-2222-222222222222', admin_user_id, '10 Treinos Completados', 'WorkoutCount', 10, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'trophy', NOW()),
                            ('33333333-3333-3333-3333-333333333333', admin_user_id, 'Guerreiro de 50 Séries', 'SetCount', 50, NOW(), NOW() + INTERVAL '60 days', 'Active', 0, true, 'star', NOW()),

                            -- Intermediate Challenges (Medium-term)
                            ('44444444-4444-4444-4444-444444444444', admin_user_id, 'Mês de Consistência', 'WorkoutCount', 12, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'zap', NOW()),
                            ('55555555-5555-5555-5555-555555555555', admin_user_id, '100 Séries Completas', 'SetCount', 100, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'medal', NOW()),
                            ('66666666-6666-6666-6666-666666666666', admin_user_id, 'Mestre dos Pesos', 'TotalWeight', 5000, NOW(), NOW() + INTERVAL '90 days', 'Active', 0, true, 'dumbbell', NOW()),

                            -- Advanced Challenges (Long-term, aspirational)
                            ('77777777-7777-7777-7777-777777777777', admin_user_id, 'Centenário', 'WorkoutCount', 100, NOW(), NOW() + INTERVAL '365 days', 'Active', 0, true, 'crown', NOW()),
                            ('88888888-8888-8888-8888-888888888888', admin_user_id, 'Lenda das 500 Séries', 'SetCount', 500, NOW(), NOW() + INTERVAL '180 days', 'Active', 0, true, 'award', NOW()),
                            ('99999999-9999-9999-9999-999999999999', admin_user_id, 'Hércules', 'TotalWeight', 50000, NOW(), NOW() + INTERVAL '365 days', 'Active', 0, true, 'shield', NOW());
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101024441_AddDefaultChallenges') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251101024441_AddDefaultChallenges', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101230127_AddOwnerIdToWorkoutSession') THEN
    ALTER TABLE "WorkoutSessions" ADD "OwnerId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101230127_AddOwnerIdToWorkoutSession') THEN

                    UPDATE "WorkoutSessions" ws
                    SET "OwnerId" = wp."OwnerId"
                    FROM "WorkoutPlans" wp
                    WHERE ws."WorkoutPlanId" = wp."Id"
                    AND ws."OwnerId" IS NULL;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101230127_AddOwnerIdToWorkoutSession') THEN

                    DELETE FROM "WorkoutSessions"
                    WHERE "OwnerId" IS NULL;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101230127_AddOwnerIdToWorkoutSession') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251101230127_AddOwnerIdToWorkoutSession', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103140804_AddGenderToUser') THEN
    ALTER TABLE "Users" ADD "Gender" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103140804_AddGenderToUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251103140804_AddGenderToUser', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103164729_AddInjuriesToUser') THEN
    ALTER TABLE "Users" ADD "Injuries" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103164729_AddInjuriesToUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251103164729_AddInjuriesToUser', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103231622_AddLastLoginAtToUsers') THEN
    ALTER TABLE "Users" ADD "LastLoginAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103231622_AddLastLoginAtToUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251103231622_AddLastLoginAtToUsers', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106113426_AddPerformanceIndexes') THEN
    CREATE INDEX "IX_WorkoutSessions_OwnerId" ON "WorkoutSessions" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106113426_AddPerformanceIndexes') THEN
    CREATE INDEX "IX_WorkoutSessions_OwnerId_CompletedAt" ON "WorkoutSessions" ("OwnerId", "CompletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251106113426_AddPerformanceIndexes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251106113426_AddPerformanceIndexes', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251107132714_AddTrainingSplitToUser') THEN
    ALTER TABLE "Users" ADD "TrainingSplit" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251107132714_AddTrainingSplitToUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251107132714_AddTrainingSplitToUser', '8.0.4');
    END IF;
END $EF$;
COMMIT;

