'use client';

import { ChevronDown } from 'lucide-react';
import type { WorkoutPlanExercise, WorkoutExercise, WorkoutSet } from '@gymhero/shared';
import { Card, CardHeader } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import Image from 'next/image';

interface ExerciseCardProps {
  exercise: WorkoutPlanExercise | WorkoutExercise;
  sets: WorkoutSet[];
  onAddSet: (data: { reps: number; weight?: number; rpe?: number }) => void;
  onDeleteSet: (setId: string) => void;
  onExerciseClick?: () => void;
}

export function ExerciseCard({ exercise, sets, onAddSet, onDeleteSet, onExerciseClick }: ExerciseCardProps) {
  const exerciseSets = sets.filter((s) => s.exerciseId === exercise.exerciseId);
  const isCompleted = exerciseSets.length >= exercise.targetSets;

  const handleAddSingleSet = (checked: boolean) => {
    if (checked && !isCompleted) {
      // Add one set
      onAddSet({
        reps: exercise.targetReps,
        weight: exercise.targetLoad > 0 ? exercise.targetLoad : undefined,
        rpe: undefined,
      });
    }
  };

  const handleCompleteExercise = (checked: boolean) => {
    if (checked && !isCompleted) {
      // Add all remaining sets for the exercise
      const remainingSets = exercise.targetSets - exerciseSets.length;
      for (let i = 0; i < remainingSets; i++) {
        onAddSet({
          reps: exercise.targetReps,
          weight: exercise.targetLoad > 0 ? exercise.targetLoad : undefined,
          rpe: undefined,
        });
      }
    }
  };

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-start gap-3">
          {/* Thumbnail Image */}
          {exercise.exercise?.imageUrl && (
            <div className="relative h-16 w-16 flex-shrink-0 overflow-hidden rounded-md">
              <Image
                src={exercise.exercise.imageUrl}
                alt={exercise.exercise.name}
                fill
                className="object-cover"
              />
            </div>
          )}

          {/* Exercise Info */}
          <div className="flex-1 min-w-0">
            <div className="flex items-start justify-between gap-2">
              <div className="flex-1 min-w-0">
                <h3
                  className={`text-base font-semibold truncate ${onExerciseClick ? 'cursor-pointer hover:text-primary transition-colors' : ''}`}
                  onClick={onExerciseClick}
                >
                  {exercise.exercise?.name || exercise.exerciseName}
                </h3>
                <p className="text-sm text-muted-foreground">
                  {exercise.targetSets} séries × {exercise.targetReps} reps
                  {exercise.targetLoad > 0 && ` @ ${exercise.targetLoad} kg`}
                </p>
                {exercise.exercise?.muscleGroup && (
                  <p className="text-xs text-muted-foreground capitalize">
                    {exercise.exercise.muscleGroup}
                  </p>
                )}
              </div>

              {/* Progress Indicator */}
              <div className="text-right flex-shrink-0">
                <p className="text-2xl font-bold text-primary">{exerciseSets.length}</p>
                <p className="text-xs text-muted-foreground">de {exercise.targetSets}</p>
              </div>
            </div>

            {/* Action Buttons Row */}
            <div className="flex items-center gap-4 mt-3">
              {!isCompleted && (
                <>
                  {/* Add One Set Checkbox */}
                  <div className="flex items-center gap-2">
                    <Checkbox
                      onCheckedChange={handleAddSingleSet}
                      checked={false}
                      className="h-5 w-5"
                    />
                    <label className="text-sm text-muted-foreground cursor-pointer">
                      +1 série
                    </label>
                  </div>

                  {/* Complete Exercise Checkbox */}
                  <div className="flex items-center gap-2">
                    <Checkbox
                      onCheckedChange={handleCompleteExercise}
                      checked={false}
                      className="h-5 w-5"
                    />
                    <label className="text-sm text-muted-foreground cursor-pointer">
                      Concluir tudo
                    </label>
                  </div>
                </>
              )}

              {isCompleted && (
                <div className="flex items-center gap-2 text-primary">
                  <span className="text-sm font-medium">✓ Completado</span>
                </div>
              )}

              {/* Details Button - Opens Modal */}
              {onExerciseClick && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={onExerciseClick}
                  className="ml-auto"
                >
                  <ChevronDown className="h-4 w-4 mr-1" />
                  Detalhes
                </Button>
              )}
            </div>
          </div>
        </div>
      </CardHeader>
    </Card>
  );
}
