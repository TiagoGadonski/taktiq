#!/bin/bash

# Script to replace all 'api' imports with 'apiClient' in the frontend

echo "Fixing all API imports..."

# List of files that need to be fixed
files=(
  "frontend/apps/web/src/app/(app)/stripe-connect/page.tsx"
  "frontend/apps/web/src/hooks/use-sets.ts"
  "frontend/apps/web/src/hooks/use-session.ts"
  "frontend/apps/web/src/components/workout/share-settings-dialog.tsx"
  "frontend/apps/web/src/components/workout/renew-plan-dialog.tsx"
  "frontend/apps/web/src/app/(auth)/reset-password/page.tsx"
  "frontend/apps/web/src/app/(auth)/forgot-password/page.tsx"
  "frontend/apps/web/src/app/(app)/workout/page.tsx"
  "frontend/apps/web/src/app/(app)/progress/page.tsx"
  "frontend/apps/web/src/app/(app)/plans/public/[id]/page.tsx"
  "frontend/apps/web/src/app/(app)/plans/page.tsx"
  "frontend/apps/web/src/app/(app)/history/page.tsx"
  "frontend/apps/web/src/app/(app)/dashboard/page.tsx"
  "frontend/apps/web/src/app/(app)/activity/page.tsx"
)

for file in "${files[@]}"; do
  if [ -f "$file" ]; then
    echo "Fixing $file..."
    # Replace import { api } with import { apiClient }
    sed -i 's/import { api } from/import { apiClient } from/g' "$file"
    sed -i 's/import { api, /import { apiClient, /g' "$file"
    # Note: Actual usage (api.get -> apiClient.get) will need manual review
  fi
done

echo "Done! Please review and test the changes."
