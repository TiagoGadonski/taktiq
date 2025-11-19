-- Fix UserActivityLogs foreign key constraint to allow user deletion
-- This script drops and recreates the FK with ON DELETE SET NULL behavior

BEGIN;

-- Drop the existing foreign key constraint
ALTER TABLE "UserActivityLogs"
DROP CONSTRAINT IF EXISTS "FK_UserActivityLogs_Users_UserId";

-- Recreate the foreign key with ON DELETE SET NULL behavior
-- This allows user deletion while preserving activity logs for audit purposes
ALTER TABLE "UserActivityLogs"
ADD CONSTRAINT "FK_UserActivityLogs_Users_UserId"
FOREIGN KEY ("UserId")
REFERENCES "Users" ("Id")
ON DELETE SET NULL;

-- Add migration record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251114114209_FixUserActivityLogsForeignKey', '9.0.0');

COMMIT;
