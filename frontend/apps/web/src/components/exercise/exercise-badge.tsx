'use client';

import { Badge } from '@/components/ui/badge';
import { Home, Dumbbell, Building2 } from 'lucide-react';

interface ExerciseBadgeProps {
  workoutLocation?: number; // 0 = Gym, 1 = Home, 2 = Both
  equipment?: string;
  className?: string;
}

export function ExerciseBadge({ workoutLocation, equipment, className }: ExerciseBadgeProps) {
  return (
    <div className={`flex gap-2 flex-wrap ${className || ''}`}>
      {/* Location Badge */}
      {workoutLocation !== undefined && (
        <Badge
          variant={workoutLocation === 1 ? 'default' : 'secondary'}
          className={`text-xs flex items-center gap-1 ${
            workoutLocation === 1
              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
              : workoutLocation === 0
              ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
              : 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200'
          }`}
        >
          {workoutLocation === 0 && (
            <>
              <Building2 className="h-3 w-3" />
              <span>Academia</span>
            </>
          )}
          {workoutLocation === 1 && (
            <>
              <Home className="h-3 w-3" />
              <span>Casa</span>
            </>
          )}
          {workoutLocation === 2 && (
            <>
              <Building2 className="h-3 w-3" />
              <Home className="h-3 w-3" />
              <span>Ambos</span>
            </>
          )}
        </Badge>
      )}

      {/* Equipment Badge */}
      {equipment && equipment.toLowerCase().includes('bodyweight') && (
        <Badge
          variant="outline"
          className="text-xs flex items-center gap-1 bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950 dark:text-orange-300 dark:border-orange-800"
        >
          <Dumbbell className="h-3 w-3" />
          <span>Peso Corporal</span>
        </Badge>
      )}

      {/* Equipment name if not bodyweight */}
      {equipment && !equipment.toLowerCase().includes('bodyweight') && (
        <Badge variant="outline" className="text-xs">
          {equipment}
        </Badge>
      )}
    </div>
  );
}

// Simplified version for tight spaces
export function ExerciseBadgeCompact({ workoutLocation, equipment }: ExerciseBadgeProps) {
  return (
    <div className="flex gap-1 items-center">
      {/* Location Icon Only */}
      {workoutLocation === 0 && (
        <Badge variant="secondary" className="text-xs px-1.5 py-0.5 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
          <Building2 className="h-3 w-3" />
        </Badge>
      )}
      {workoutLocation === 1 && (
        <Badge variant="default" className="text-xs px-1.5 py-0.5 bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
          <Home className="h-3 w-3" />
        </Badge>
      )}
      {workoutLocation === 2 && (
        <Badge variant="secondary" className="text-xs px-1.5 py-0.5 bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
          <span className="flex gap-0.5">
            <Building2 className="h-3 w-3" />
            <Home className="h-3 w-3" />
          </span>
        </Badge>
      )}

      {/* Equipment Icon */}
      {equipment && equipment.toLowerCase().includes('bodyweight') && (
        <Badge variant="outline" className="text-xs px-1.5 py-0.5 bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950 dark:text-orange-300 dark:border-orange-800">
          <Dumbbell className="h-3 w-3" />
        </Badge>
      )}
    </div>
  );
}
