'use client';

import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { Play, CheckCircle2, XCircle, Clock, Sparkles } from 'lucide-react';
import { api } from '@/lib/api';
import { useSession } from '@/hooks/use-session';
import { useSets } from '@/hooks/use-sets';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ExerciseCard } from '@/components/workout/exercise-card';
import { ExerciseSelectorModal } from '@/components/workout/exercise-selector-modal';
import { WorkoutCompletionModal } from '@/components/workout/workout-completion-modal';
import { useToast } from '@/components/ui/use-toast';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import type { CreateSetInput, Workout, WorkoutExercise, WorkoutSet } from '@gymhero/shared';
import { getRandomSetMessage, getRandomWorkoutMessage, getMilestoneMessage } from '@gymhero/shared';

export default function WorkoutPage() {
  const router = useRouter();
  const { toast } = useToast();
  const { currentSession, hasActiveSession, startSession, completeSession, isStarting, isCompleting } = useSession();
  const { createSet, deleteSet, isCreating } = useSets();
  const [notes, setNotes] = useState('');
  const [selectedWorkoutId, setSelectedWorkoutId] = useState<string | null>(null);
  const [selectedExercise, setSelectedExercise] = useState<WorkoutExercise | null>(null);
  const [isExerciseModalOpen, setIsExerciseModalOpen] = useState(false);
  const [replacedExercises, setReplacedExercises] = useState<Record<string, any>>({});
  const [exerciseToReplace, setExerciseToReplace] = useState<WorkoutExercise | null>(null);
  const [isReplaceSelectorOpen, setIsReplaceSelectorOpen] = useState(false);
  const [showCompletionModal, setShowCompletionModal] = useState(false);

  // Auto-select first workout when session starts
  useEffect(() => {
    if (currentSession?.workoutPlan?.workouts && currentSession.workoutPlan.workouts.length > 0 && !selectedWorkoutId) {
      setSelectedWorkoutId(currentSession.workoutPlan.workouts[0].id);
    }
  }, [currentSession, selectedWorkoutId]);

  const { data: activePlan } = useQuery({
    queryKey: ['workout-plans', 'active'],
    queryFn: async () => {
      const plans = await api.workoutPlans.getAll();
      return plans.find((p) => p.isActive);
    },
  });

  const { data: allExercises = [] } = useQuery({
    queryKey: ['exercises'],
    queryFn: async () => {
      return await api.exercises.getAll();
    },
  });

  const openExerciseModal = (exercise: WorkoutExercise) => {
    setSelectedExercise(exercise);
    setIsExerciseModalOpen(true);
  };

  const handleOpenReplaceModal = (exercise: WorkoutExercise) => {
    setExerciseToReplace(exercise);
    setIsReplaceSelectorOpen(true);
  };

  const handleReplaceExercise = (newExercise: any) => {
    if (!exerciseToReplace) return;

    // Store the replacement mapping
    setReplacedExercises((prev) => ({
      ...prev,
      [exerciseToReplace.id]: {
        ...exerciseToReplace,
        exerciseId: newExercise.id,
        exerciseName: newExercise.name,
        exercise: newExercise,
      },
    }));

    toast({
      title: 'Exercício substituído!',
      description: `${exerciseToReplace.exerciseName} → ${newExercise.name}`,
    });

    setIsReplaceSelectorOpen(false);
    setExerciseToReplace(null);
  };

  const checkWorkoutCompletion = (exercises: WorkoutExercise[], sets: WorkoutSet[]) => {
    // If no exercises, don't auto-complete
    if (!exercises || exercises.length === 0) return false;

    // Check if all exercises have completed their target sets
    const allExercisesCompleted = exercises.every((exercise) => {
      const displayExercise = replacedExercises[exercise.id] || exercise;
      const exerciseSets = sets.filter((s) => s.exerciseId === displayExercise.exerciseId);
      return exerciseSets.length >= exercise.targetSets;
    });

    return allExercisesCompleted;
  };

  const handleStartSession = async (workoutId?: string) => {
    try {
      await startSession(workoutId);
      toast({
        title: 'Treino iniciado!',
        description: 'Boa sorte no treino de hoje!',
      });
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao iniciar treino',
        description: error.message,
      });
    }
  };

  const handleCompleteSession = async () => {
    if (!currentSession) return;

    try {
      await completeSession({ sessionId: currentSession.id, notes });
      const motivationalMessage = getRandomWorkoutMessage();
      toast({
        title: motivationalMessage.title,
        description: motivationalMessage.description,
      });
      router.push('/dashboard');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao concluir treino',
        description: error.message,
      });
    }
  };

  const handleAddSet = async (exerciseId: string, data: { reps: number; weight?: number; rpe?: number }) => {
    if (!currentSession) return;

    const setNumber = (currentSession.sets?.filter((s: WorkoutSet) => s.exerciseId === exerciseId).length || 0) + 1;

    const setData: CreateSetInput = {
      sessionId: currentSession.id,
      exerciseId,
      setNumber,
      ...data,
    };

    try {
      await createSet(setData);

      // Get the total number of sets completed in this session
      const totalSets = (currentSession.sets?.length || 0) + 1;

      // Check for milestone messages first
      const milestoneMessage = getMilestoneMessage(totalSets);

      if (milestoneMessage) {
        // Show milestone message for special achievements
        toast({
          title: milestoneMessage.title,
          description: milestoneMessage.description,
        });
      } else {
        // Show random motivational message
        const motivationalMessage = getRandomSetMessage();
        toast({
          title: motivationalMessage.title,
          description: motivationalMessage.description,
        });
      }

      // Check if all exercises are completed after adding this set
      // We need to simulate the updated sets array by adding the new set
      const selectedWorkout = currentSession.workoutPlan?.workouts?.find(
        (w: Workout) => w.id === selectedWorkoutId
      );
      const exercisesToCheck = selectedWorkout?.exercises || currentSession.workoutPlan?.exercises || [];

      if (exercisesToCheck.length > 0) {
        // Simulate the new sets array with the just-added set
        const updatedSets = [...(currentSession.sets || []), { ...setData, id: 'temp', exerciseId } as WorkoutSet];

        if (checkWorkoutCompletion(exercisesToCheck, updatedSets)) {
          // All exercises completed! Show completion modal
          setShowCompletionModal(true);
        }
      }
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao adicionar série',
        description: error.message,
      });
    }
  };

  const handleDeleteSet = async (setId: string) => {
    try {
      await deleteSet(setId);
      toast({
        title: 'Série removida',
      });
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao remover série',
        description: error.message,
      });
    }
  };

  // Calculate session duration
  const sessionDuration = currentSession
    ? Math.floor((new Date().getTime() - new Date(currentSession.startedAt).getTime()) / 60000)
    : 0;

  // No active session - show start options
  if (!hasActiveSession) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Treino do Dia</h1>
          <p className="text-muted-foreground">Escolha um treino para começar</p>
        </div>

        {activePlan ? (
          <>
            {activePlan.workouts && activePlan.workouts.length > 0 ? (
              <div className="space-y-6">
                <div>
                  <h2 className="text-xl font-semibold mb-2">{activePlan.name}</h2>
                  <p className="text-sm text-muted-foreground">
                    {activePlan.goal || 'Selecione um treino para começar'}
                  </p>
                </div>
                <div className="space-y-4">
                  {activePlan.workouts.map((workout) => (
                    <Card key={workout.id}>
                      <CardHeader className="pb-3">
                        <div className="flex items-center justify-between">
                          <div>
                            <CardTitle className="text-lg">{workout.name}</CardTitle>
                            <CardDescription>
                              {workout.exercises?.length || 0} exercícios
                            </CardDescription>
                          </div>
                          <Button
                            onClick={() => {
                              setSelectedWorkoutId(workout.id);
                              handleStartSession(activePlan.id);
                            }}
                            disabled={isStarting}
                            size="sm"
                          >
                            <Play className="mr-2 h-4 w-4" />
                            {isStarting ? 'Iniciando...' : 'Iniciar'}
                          </Button>
                        </div>
                      </CardHeader>
                      <CardContent>
                        <div className="space-y-2">
                          {workout.exercises && workout.exercises.length > 0 ? (
                            workout.exercises.map((exercise, index) => {
                              const exerciseData = exercise as any;
                              return (
                              <div
                                key={exercise.id || index}
                                className="flex items-center justify-between py-2 px-3 bg-muted/50 rounded-md text-sm cursor-pointer hover:bg-muted transition-colors"
                                onClick={() => openExerciseModal(exercise)}
                              >
                                <div className="flex items-center gap-2">
                                  <span className="font-medium text-muted-foreground">
                                    {index + 1}.
                                  </span>
                                  <span>{exerciseData.exerciseName || exerciseData.name}</span>
                                </div>
                                <span className="text-muted-foreground">
                                  {exercise.targetSets}x{exercise.targetReps}
                                </span>
                              </div>
                              );
                            })
                          ) : (
                            <p className="text-sm text-muted-foreground">Nenhum exercício configurado</p>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </div>
            ) : (
              <Card className="cursor-pointer transition-colors hover:border-primary">
                <CardHeader>
                  <CardTitle>{activePlan.name}</CardTitle>
                  <CardDescription>
                    {activePlan.exercises?.length || 0} exercícios
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Button
                    onClick={() => handleStartSession(activePlan.id)}
                    disabled={isStarting}
                    className="w-full"
                  >
                    <Play className="mr-2 h-4 w-4" />
                    {isStarting ? 'Iniciando...' : 'Iniciar Treino'}
                  </Button>
                </CardContent>
              </Card>
            )}
          </>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle>Nenhum plano ativo</CardTitle>
              <CardDescription>
                Você precisa criar e ativar um plano de treino primeiro.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button onClick={() => router.push('/plans')}>Criar Plano de Treino</Button>
            </CardContent>
          </Card>
        )}

        {/* Quick start without workout */}
        <Card>
          <CardHeader>
            <CardTitle>Treino Livre</CardTitle>
            <CardDescription>Comece um treino sem plano pré-definido</CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              variant="outline"
              onClick={() => handleStartSession()}
              disabled={isStarting}
              className="w-full"
            >
              <Play className="mr-2 h-4 w-4" />
              Iniciar Treino Livre
            </Button>
          </CardContent>
        </Card>

        {/* Exercise Detail Modal */}
        <Dialog open={isExerciseModalOpen} onOpenChange={setIsExerciseModalOpen}>
          <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle className="text-2xl">
                {selectedExercise?.exerciseName || selectedExercise?.exercise?.name || 'Detalhes do Exercício'}
              </DialogTitle>
              <DialogDescription>
                Detalhes do exercício
              </DialogDescription>
              {selectedExercise?.exercise && (
                <div className="flex flex-wrap gap-2 mt-2">
                  {(selectedExercise.exercise as any).muscleGroup && (
                    <Badge variant="secondary">{(selectedExercise.exercise as any).muscleGroup}</Badge>
                  )}
                  {(selectedExercise.exercise as any).equipment && (
                    <Badge variant="secondary">{(selectedExercise.exercise as any).equipment}</Badge>
                  )}
                  {(selectedExercise.exercise as any).category && (
                    <Badge variant="secondary">{(selectedExercise.exercise as any).category}</Badge>
                  )}
                </div>
              )}
            </DialogHeader>

            {selectedExercise?.exercise && (() => {
              const exerciseData = selectedExercise.exercise as any;
              return (
              <div className="space-y-6 mt-4">
                {/* Exercise Video/Image */}
                {exerciseData.videoUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    {exerciseData.videoUrl.includes('youtube.com') || exerciseData.videoUrl.includes('youtu.be') ? (
                      <iframe
                        width="100%"
                        height="400"
                        src={exerciseData.videoUrl.replace('watch?v=', 'embed/')}
                        title={exerciseData.name}
                        frameBorder="0"
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                        allowFullScreen
                        className="rounded-md"
                      ></iframe>
                    ) : (
                      <video
                        src={exerciseData.videoUrl}
                        controls
                        className="w-full rounded-md"
                        style={{ maxHeight: '400px' }}
                      />
                    )}
                  </div>
                )}
                {!exerciseData.videoUrl && exerciseData.imageUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    <img
                      src={exerciseData.imageUrl}
                      alt={exerciseData.name}
                      className="max-w-full h-auto rounded-md"
                      style={{ maxHeight: '400px' }}
                    />
                  </div>
                )}

                {/* Exercise Parameters */}
                <div className="grid grid-cols-3 gap-4">
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetSets}</p>
                      <p className="text-sm text-muted-foreground mt-1">Séries</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetReps}</p>
                      <p className="text-sm text-muted-foreground mt-1">Repetições</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetLoad || '—'}</p>
                      <p className="text-sm text-muted-foreground mt-1">Carga (kg)</p>
                    </CardContent>
                  </Card>
                </div>

                {/* Exercise Notes/Instructions */}
                {exerciseData.notes && (
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">Instruções</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <p className="whitespace-pre-wrap text-sm">{exerciseData.notes}</p>
                    </CardContent>
                  </Card>
                )}

                {/* Tips Card */}
                <Card className="bg-primary/5 border-primary/20">
                  <CardHeader>
                    <CardTitle className="text-lg flex items-center gap-2">
                      <Sparkles className="h-5 w-5 text-primary" />
                      Dicas Importantes
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <ul className="space-y-2 text-sm">
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Mantenha uma postura adequada durante todo o movimento</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Controle o movimento tanto na fase concêntrica quanto excêntrica</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Respire corretamente: expire no esforço, inspire no relaxamento</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Se sentir dor (não confundir com desconforto muscular), pare imediatamente</span>
                      </li>
                    </ul>
                  </CardContent>
                </Card>
              </div>
              );
            })()}
          </DialogContent>
        </Dialog>
      </div>
    );
  }

  // Get the selected workout and its exercises
  const selectedWorkout = currentSession.workoutPlan?.workouts?.find(
    (w: Workout) => w.id === selectedWorkoutId
  );
  const exercisesToShow = selectedWorkout?.exercises || currentSession.workoutPlan?.exercises || [];

  // Active session - show workout execution
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            {selectedWorkout?.name || currentSession.workoutPlan?.name || 'Treino Livre'}
          </h1>
          <div className="mt-2 flex items-center gap-4 text-sm text-muted-foreground">
            <div className="flex items-center gap-1">
              <Clock className="h-4 w-4" />
              {sessionDuration} minutos
            </div>
            <div>
              {currentSession.sets?.length || 0} séries completadas
            </div>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => router.push('/dashboard')}>
            <XCircle className="mr-2 h-4 w-4" />
            Pausar
          </Button>
          <Button onClick={handleCompleteSession} disabled={isCompleting}>
            <CheckCircle2 className="mr-2 h-4 w-4" />
            {isCompleting ? 'Concluindo...' : 'Concluir Treino'}
          </Button>
        </div>
      </div>

      {/* Exercise List */}
      {exercisesToShow.length > 0 ? (
        <div className="space-y-4">
          {exercisesToShow.map((exercise: WorkoutExercise) => {
            // Use replaced exercise if it exists, otherwise use original
            const displayExercise = replacedExercises[exercise.id] || exercise;

            return (
              <ExerciseCard
                key={exercise.id}
                exercise={displayExercise}
                sets={currentSession.sets || []}
                onAddSet={(data) => handleAddSet(displayExercise.exerciseId, data)}
                onDeleteSet={handleDeleteSet}
                onExerciseClick={() => openExerciseModal(displayExercise)}
                onReplaceExercise={() => handleOpenReplaceModal(exercise)}
                isCreating={isCreating}
              />
            );
          })}
        </div>
      ) : (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">
              {selectedWorkoutId
                ? 'Este treino não tem exercícios configurados.'
                : 'Este é um treino livre. Adicione exercícios conforme necessário.'}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Session Notes */}
      <Card>
        <CardHeader>
          <CardTitle>Notas do Treino</CardTitle>
        </CardHeader>
        <CardContent>
          <textarea
            className="w-full rounded-md border bg-background p-3 text-sm"
            placeholder="Como foi o treino? Alguma observação?"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            rows={4}
          />
        </CardContent>
      </Card>

      {/* Exercise Detail Modal */}
      <Dialog open={isExerciseModalOpen} onOpenChange={setIsExerciseModalOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="text-2xl">
              {selectedExercise?.exerciseName || selectedExercise?.exercise?.name || 'Detalhes do Exercício'}
            </DialogTitle>
            <DialogDescription>
              Detalhes do exercício
            </DialogDescription>
            {selectedExercise?.exercise && (
              <div className="flex flex-wrap gap-2 mt-2">
                {(selectedExercise.exercise as any).muscleGroup && (
                  <Badge variant="secondary">{(selectedExercise.exercise as any).muscleGroup}</Badge>
                )}
                {(selectedExercise.exercise as any).equipment && (
                  <Badge variant="secondary">{(selectedExercise.exercise as any).equipment}</Badge>
                )}
                {(selectedExercise.exercise as any).category && (
                  <Badge variant="secondary">{(selectedExercise.exercise as any).category}</Badge>
                )}
              </div>
            )}
          </DialogHeader>

          {selectedExercise?.exercise && (() => {
            const exerciseData = selectedExercise.exercise as any;
            return (
            <div className="space-y-6 mt-4">
                {/* Exercise Video/Image */}
                {exerciseData.videoUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    {exerciseData.videoUrl.includes('youtube.com') || exerciseData.videoUrl.includes('youtu.be') ? (
                      <iframe
                        width="100%"
                        height="400"
                        src={exerciseData.videoUrl.replace('watch?v=', 'embed/')}
                        title={exerciseData.name}
                        frameBorder="0"
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                        allowFullScreen
                        className="rounded-md"
                      ></iframe>
                    ) : (
                      <video
                        src={exerciseData.videoUrl}
                        controls
                        className="w-full rounded-md"
                        style={{ maxHeight: '400px' }}
                      />
                    )}
                  </div>
                )}
                {!exerciseData.videoUrl && exerciseData.imageUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    <img
                      src={exerciseData.imageUrl}
                      alt={exerciseData.name}
                      className="max-w-full h-auto rounded-md"
                      style={{ maxHeight: '400px' }}
                    />
                  </div>
                )}

                {/* Exercise Parameters */}
                <div className="grid grid-cols-3 gap-4">
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetSets}</p>
                      <p className="text-sm text-muted-foreground mt-1">Séries</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetReps}</p>
                      <p className="text-sm text-muted-foreground mt-1">Repetições</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.targetLoad || '—'}</p>
                      <p className="text-sm text-muted-foreground mt-1">Carga (kg)</p>
                    </CardContent>
                  </Card>
                </div>

                {/* Exercise Notes/Instructions */}
                {exerciseData.notes && (
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">Instruções</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <p className="whitespace-pre-wrap text-sm">{exerciseData.notes}</p>
                    </CardContent>
                  </Card>
                )}

                {/* Tips Card */}
                <Card className="bg-primary/5 border-primary/20">
                  <CardHeader>
                    <CardTitle className="text-lg flex items-center gap-2">
                      <Sparkles className="h-5 w-5 text-primary" />
                      Dicas Importantes
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <ul className="space-y-2 text-sm">
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Mantenha uma postura adequada durante todo o movimento</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Controle o movimento tanto na fase concêntrica quanto excêntrica</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Respire corretamente: expire no esforço, inspire no relaxamento</span>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-0.5">•</span>
                        <span>Se sentir dor (não confundir com desconforto muscular), pare imediatamente</span>
                      </li>
                    </ul>
                  </CardContent>
                </Card>
              </div>
              );
            })()}
        </DialogContent>
      </Dialog>

      {/* Exercise Replacement Modal */}
      <ExerciseSelectorModal
        open={isReplaceSelectorOpen}
        onClose={() => setIsReplaceSelectorOpen(false)}
        onSelectExercise={handleReplaceExercise}
        currentExercise={exerciseToReplace ? {
          id: exerciseToReplace.exerciseId,
          name: exerciseToReplace.exerciseName || 'Unknown Exercise',
          muscleGroup: exerciseToReplace.exercise?.muscleGroup || 'Other',
          equipment: exerciseToReplace.exercise?.equipment || '',
        } : undefined}
        exercises={allExercises}
      />

      {/* Workout Completion Modal */}
      <WorkoutCompletionModal
        open={showCompletionModal}
        onComplete={handleCompleteSession}
        autoCompleteDelay={5000}
      />
    </div>
  );
}
