'use client';

import { useState } from 'react';
import { ChevronDown, Plus, Trash2, Check, RefreshCw, Pencil } from 'lucide-react';
import type { WorkoutPlanExercise, WorkoutExercise, WorkoutSet } from '@gymhero/shared';
import { Card, CardHeader, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import Image from 'next/image';

interface ExerciseCardProps {
  exercise: WorkoutPlanExercise | WorkoutExercise;
  sets: WorkoutSet[];
  onAddSet: (data: { reps: number; weight?: number; rpe?: number }) => void;
  onDeleteSet: (setId: string) => void;
  onEditSet?: (setId: string, data: { reps: number; weight?: number; rpe?: number }) => void;
  onExerciseClick?: () => void;
  onReplaceExercise?: () => void;
  isCreating?: boolean;
}

export function ExerciseCard({ exercise, sets, onAddSet, onDeleteSet, onEditSet, onExerciseClick, onReplaceExercise, isCreating = false }: ExerciseCardProps) {
  const exerciseSets = sets.filter((s) => s.exerciseId === exercise.exerciseId);
  const isCompleted = exerciseSets.length >= exercise.targetSets;
  const [showSetForm, setShowSetForm] = useState(false);
  const [newSetReps, setNewSetReps] = useState(exercise.targetReps);
  const [newSetWeight, setNewSetWeight] = useState(exercise.targetLoad || 0);

  // ✅ NEW: Edit set state
  const [editingSetId, setEditingSetId] = useState<string | null>(null);
  const [editSetReps, setEditSetReps] = useState(0);
  const [editSetWeight, setEditSetWeight] = useState(0);

  const handleAddSetWithValues = () => {
    onAddSet({
      reps: newSetReps,
      weight: newSetWeight > 0 ? newSetWeight : undefined,
      rpe: undefined,
    });
    setShowSetForm(false);
    // Keep values for next set
  };

  const handleQuickAddSet = () => {
    onAddSet({
      reps: exercise.targetReps,
      weight: exercise.targetLoad > 0 ? exercise.targetLoad : undefined,
      rpe: undefined,
    });
  };

  const handleCompleteExercise = () => {
    const remainingSets = exercise.targetSets - exerciseSets.length;
    for (let i = 0; i < remainingSets; i++) {
      onAddSet({
        reps: exercise.targetReps,
        weight: exercise.targetLoad > 0 ? exercise.targetLoad : undefined,
        rpe: undefined,
      });
    }
  };

  // ✅ NEW: Edit set handlers
  const handleStartEdit = (set: WorkoutSet) => {
    setEditingSetId(set.id);
    setEditSetReps(set.reps);
    setEditSetWeight(set.weight || 0);
  };

  const handleSaveEdit = () => {
    if (editingSetId && onEditSet) {
      onEditSet(editingSetId, {
        reps: editSetReps,
        weight: editSetWeight > 0 ? editSetWeight : undefined,
        rpe: undefined,
      });
      setEditingSetId(null);
    }
  };

  const handleCancelEdit = () => {
    setEditingSetId(null);
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
                  Meta: {exercise.targetSets} séries × {exercise.targetReps} reps
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
                <p className="text-xl sm:text-2xl font-bold text-primary">{exerciseSets.length}</p>
                <p className="text-xs text-muted-foreground">de {exercise.targetSets}</p>
              </div>
            </div>

            {/* Details and Replace Buttons */}
            <div className="flex gap-1 sm:gap-2 mt-2 -ml-2">
              {onExerciseClick && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={onExerciseClick}
                  className="text-xs sm:text-sm h-8 px-2 sm:px-3"
                >
                  <ChevronDown className="h-3 w-3 sm:h-4 sm:w-4 sm:mr-1" />
                  <span className="hidden sm:inline">Ver detalhes</span>
                  <span className="sm:hidden">Detalhes</span>
                </Button>
              )}
              {onReplaceExercise && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={onReplaceExercise}
                  title="Substituir exercício"
                  className="text-xs sm:text-sm h-8 px-2 sm:px-3"
                >
                  <RefreshCw className="h-3 w-3 sm:h-4 sm:w-4 sm:mr-1" />
                  <span className="hidden sm:inline">Substituir</span>
                  <span className="sm:hidden">Trocar</span>
                </Button>
              )}
            </div>
          </div>
        </div>
      </CardHeader>

      {/* Completed Sets List */}
      {exerciseSets.length > 0 && (
        <CardContent className="pt-0 pb-3">
          <div className="space-y-2">
            <p className="text-xs font-medium text-muted-foreground mb-2">Séries completadas:</p>
            {exerciseSets.map((set, index) => (
              <div key={set.id}>
                {/* ✅ EDIT MODE: Show edit form */}
                {editingSetId === set.id ? (
                  <div className="bg-muted/30 rounded-lg p-3 space-y-3">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="font-medium text-primary text-sm">#{index + 1}</span>
                      <span className="text-xs text-muted-foreground">Editando série</span>
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
                          Repetições
                        </label>
                        <Input
                          type="number"
                          value={editSetReps}
                          onChange={(e) => setEditSetReps(parseInt(e.target.value) || 0)}
                          className="h-9"
                          min="1"
                          autoFocus
                        />
                      </div>
                      <div>
                        <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
                          Peso (kg)
                        </label>
                        <Input
                          type="number"
                          value={editSetWeight}
                          onChange={(e) => setEditSetWeight(parseFloat(e.target.value) || 0)}
                          className="h-9"
                          min="0"
                          step="0.5"
                        />
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        onClick={handleSaveEdit}
                        size="sm"
                        className="flex-1"
                        disabled={isCreating}
                      >
                        <Check className="h-4 w-4 mr-1" />
                        Salvar
                      </Button>
                      <Button
                        variant="outline"
                        onClick={handleCancelEdit}
                        size="sm"
                        disabled={isCreating}
                      >
                        Cancelar
                      </Button>
                    </div>
                  </div>
                ) : (
                  /* ✅ NORMAL MODE: Show set info with edit/delete buttons */
                  <div className="flex items-center justify-between bg-muted/50 rounded-md px-3 py-2 text-sm">
                    <div className="flex items-center gap-3">
                      <span className="font-medium text-primary">#{index + 1}</span>
                      <span>{set.reps} reps</span>
                      {set.weight && <span className="font-medium">{set.weight} kg</span>}
                    </div>
                    <div className="flex items-center gap-1">
                      {/* ✅ NEW: Edit button */}
                      {onEditSet && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleStartEdit(set)}
                          className="h-7 w-7 p-0 hover:bg-primary/10 hover:text-primary"
                          title="Editar série"
                        >
                          <Pencil className="h-3.5 w-3.5" />
                        </Button>
                      )}
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => onDeleteSet(set.id)}
                        className="h-7 w-7 p-0 hover:bg-destructive/10 hover:text-destructive"
                        title="Deletar série"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        </CardContent>
      )}

      {/* Add Set Section */}
      {!isCompleted && (
        <CardContent className="pt-0">
          <div className="space-y-3">
            {/* Set Input Form */}
            {showSetForm ? (
              <div className="bg-muted/30 rounded-lg p-3 space-y-3">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
                      Repetições
                    </label>
                    <Input
                      type="number"
                      value={newSetReps}
                      onChange={(e) => setNewSetReps(parseInt(e.target.value) || 0)}
                      className="h-9"
                      min="1"
                    />
                  </div>
                  <div>
                    <label className="text-xs font-medium text-muted-foreground mb-1.5 block">
                      Peso (kg)
                    </label>
                    <Input
                      type="number"
                      value={newSetWeight}
                      onChange={(e) => setNewSetWeight(parseFloat(e.target.value) || 0)}
                      className="h-9"
                      min="0"
                      step="0.5"
                    />
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button
                    onClick={handleAddSetWithValues}
                    size="sm"
                    className="flex-1"
                    disabled={isCreating}
                  >
                    <Check className="h-4 w-4 mr-1" />
                    {isCreating ? 'Adicionando...' : 'Adicionar'}
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => setShowSetForm(false)}
                    size="sm"
                    disabled={isCreating}
                  >
                    Cancelar
                  </Button>
                </div>
              </div>
            ) : (
              <div className="flex gap-2 flex-col sm:flex-row">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setShowSetForm(true)}
                  className="flex-1"
                  disabled={isCreating}
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Adicionar série
                </Button>
                {exerciseSets.length > 0 && exerciseSets.length < exercise.targetSets && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleCompleteExercise}
                    disabled={isCreating}
                    className="whitespace-nowrap"
                  >
                    <span className="hidden sm:inline">
                      Concluir ({exercise.targetSets - exerciseSets.length} restantes)
                    </span>
                    <span className="sm:hidden">
                      Concluir ({exercise.targetSets - exerciseSets.length})
                    </span>
                  </Button>
                )}
              </div>
            )}
          </div>
        </CardContent>
      )}

      {/* Completed Badge */}
      {isCompleted && (
        <CardContent className="pt-0">
          <div className="flex items-center justify-center gap-2 text-primary bg-primary/10 rounded-md py-2">
            <Check className="h-4 w-4" />
            <span className="text-sm font-medium">Exercício completado!</span>
          </div>
        </CardContent>
      )}
    </Card>
  );
}
