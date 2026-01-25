'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { Sparkles, Calendar, Dumbbell, Play, Save, AlertCircle } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { useSession } from '@/hooks/use-session';
import { QuickWorkoutGenerator } from '@/components/ai-workout/quick-workout-generator';
import { WeeklyPlanGenerator } from '@/components/ai-workout/weekly-plan-generator';
import Link from 'next/link';

// Types for the generated workout
interface GeneratedExercise {
  exerciseId: string;
  exerciseName: string;
  muscleGroup: string;
  equipment: string;
  sets: number;
  reps: string;
  restSeconds: number;
  videoUrl?: string;
  imageUrl?: string;
  notes?: string;
  exerciseType?: string;
}

interface QuickWorkoutResponse {
  name: string;
  description: string;
  estimatedDuration: number;
  goal: string;
  level: string;
  warmup: GeneratedExercise[];
  main: GeneratedExercise[];
  cooldown: GeneratedExercise[];
}

interface PlanWorkoutDay {
  dayNumber: number;
  dayName: string;
  focus: string;
  exercises: GeneratedExercise[];
}

interface PlanWeek {
  weekNumber: number;
  focus: string;
  workouts: PlanWorkoutDay[];
}

interface WeeklyPlanResponse {
  name: string;
  description: string;
  goal: string;
  level: string;
  splitType: string;
  weeksCount: number;
  daysPerWeek: number;
  weeks: PlanWeek[];
  progressionNotes: string;
}

export default function AIWorkoutPage() {
  const router = useRouter();
  const { startSession, hasActiveSession, isStarting } = useSession();
  const [generatedWorkout, setGeneratedWorkout] = useState<QuickWorkoutResponse | null>(null);
  const [generatedPlan, setGeneratedPlan] = useState<WeeklyPlanResponse | null>(null);
  const [activeTab, setActiveTab] = useState<'single' | 'plan'>('single');

  // Handle starting a workout session
  const handleStartWorkout = async () => {
    if (!generatedWorkout) return;

    try {
      if (hasActiveSession) {
        toast({
          variant: 'destructive',
          title: 'Treino em andamento',
          description: 'Voce ja tem um treino ativo. Complete ou cancele antes de iniciar outro.',
        });
        return;
      }

      // Convert to the format expected by the workout page
      const allExercises = [
        ...generatedWorkout.warmup.map(e => ({
          name: e.exerciseName,
          bodyPart: e.muscleGroup,
          equipment: e.equipment,
          sets: e.sets,
          reps: e.reps,
          rest: `${e.restSeconds}s`,
          instructions: [],
          videoUrl: e.videoUrl,
          gifUrl: e.imageUrl,
          exerciseType: 'warmup',
        })),
        ...generatedWorkout.main.map(e => ({
          name: e.exerciseName,
          bodyPart: e.muscleGroup,
          equipment: e.equipment,
          sets: e.sets,
          reps: e.reps,
          rest: `${e.restSeconds}s`,
          instructions: [],
          videoUrl: e.videoUrl,
          gifUrl: e.imageUrl,
          exerciseType: 'main',
        })),
        ...generatedWorkout.cooldown.map(e => ({
          name: e.exerciseName,
          bodyPart: e.muscleGroup,
          equipment: e.equipment,
          sets: e.sets,
          reps: e.reps,
          rest: `${e.restSeconds}s`,
          instructions: [],
          videoUrl: e.videoUrl,
          gifUrl: e.imageUrl,
          exerciseType: 'cooldown',
        })),
      ];

      localStorage.setItem('ai_workout_exercises', JSON.stringify(allExercises));
      localStorage.setItem('ai_workout_title', generatedWorkout.name);

      await startSession({});

      toast({
        title: 'Treino iniciado!',
        description: 'Redirecionando para a pagina de treino...',
      });

      router.push('/workout');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao iniciar treino',
        description: error.message,
      });
    }
  };

  // Handle saving the generated workout as a plan
  const handleSaveWorkout = async () => {
    if (!generatedWorkout) return;

    try {
      // Create a workout plan from the generated workout
      const planData = {
        name: generatedWorkout.name,
        goal: generatedWorkout.description,
        duration: 1,
      };

      const createdPlan = await apiClient.post<{ id: string }>('/workout-plans', planData);
      const planId = createdPlan.id;

      // Create a single workout day
      const workoutData = {
        name: generatedWorkout.name,
        dayOfWeek: null,
        order: 1,
      };

      const workoutResponse = await apiClient.post<{ id: string }>(
        `/workout-plans/${planId}/workouts`,
        workoutData
      );
      const workoutId = workoutResponse.id;

      // Add all exercises to the workout
      const allExercises = [
        ...generatedWorkout.warmup,
        ...generatedWorkout.main,
        ...generatedWorkout.cooldown,
      ];

      for (let i = 0; i < allExercises.length; i++) {
        const ex = allExercises[i];
        await apiClient.post(`/workout-plans/${planId}/workouts/${workoutId}/exercises`, {
          exerciseId: ex.exerciseId,
          order: i + 1,
          targetSets: ex.sets,
          targetReps: typeof ex.reps === 'string' ? parseInt(ex.reps.split('-')[0]) || 12 : ex.reps,
          targetLoad: 0,
        });
      }

      toast({
        title: 'Treino salvo!',
        description: 'Seu treino foi salvo e esta disponivel em Meus Planos.',
      });

      router.push('/plans');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao salvar treino',
        description: error.message,
      });
    }
  };

  // Handle saving the weekly plan
  const handleSavePlan = async () => {
    if (!generatedPlan) return;

    try {
      // Create the workout plan
      const planData = {
        name: generatedPlan.name,
        goal: generatedPlan.goal,
        duration: generatedPlan.weeksCount,
      };

      const createdPlan = await apiClient.post<{ id: string }>('/workout-plans', planData);
      const planId = createdPlan.id;

      // Create workouts for the first week (they repeat)
      const firstWeek = generatedPlan.weeks[0];
      if (firstWeek) {
        for (const day of firstWeek.workouts) {
          const workoutData = {
            name: `${day.dayName} - ${day.focus}`,
            dayOfWeek: day.dayNumber,
            order: day.dayNumber,
          };

          const workoutResponse = await apiClient.post<{ id: string }>(
            `/workout-plans/${planId}/workouts`,
            workoutData
          );
          const workoutId = workoutResponse.id;

          // Add exercises
          for (let i = 0; i < day.exercises.length; i++) {
            const ex = day.exercises[i];
            await apiClient.post(`/workout-plans/${planId}/workouts/${workoutId}/exercises`, {
              exerciseId: ex.exerciseId,
              order: i + 1,
              targetSets: ex.sets,
              targetReps: typeof ex.reps === 'string' ? parseInt(ex.reps.split('-')[0]) || 12 : ex.reps,
              targetLoad: 0,
            });
          }
        }
      }

      toast({
        title: 'Plano salvo!',
        description: 'Seu plano de treino foi salvo e esta disponivel em Meus Planos.',
      });

      router.push('/plans');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao salvar plano',
        description: error.message,
      });
    }
  };

  return (
    <div className="container max-w-4xl py-8">
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold flex items-center justify-center gap-2">
          <Sparkles className="h-8 w-8 text-primary" />
          Treino com IA
        </h1>
        <p className="text-muted-foreground mt-2">
          Gere treinos personalizados com inteligencia artificial
        </p>
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as 'single' | 'plan')} className="w-full">
        <TabsList className="grid w-full grid-cols-2 mb-8">
          <TabsTrigger value="single" className="flex items-center gap-2">
            <Dumbbell className="h-4 w-4" />
            Treino Unico
          </TabsTrigger>
          <TabsTrigger value="plan" className="flex items-center gap-2">
            <Calendar className="h-4 w-4" />
            Plano Semanal
          </TabsTrigger>
        </TabsList>

        <TabsContent value="single">
          {generatedWorkout ? (
            <WorkoutResult
              workout={generatedWorkout}
              onBack={() => setGeneratedWorkout(null)}
              onStart={handleStartWorkout}
              onSave={handleSaveWorkout}
              isStarting={isStarting}
            />
          ) : (
            <QuickWorkoutGenerator onWorkoutGenerated={setGeneratedWorkout} />
          )}
        </TabsContent>

        <TabsContent value="plan">
          {generatedPlan ? (
            <PlanResult
              plan={generatedPlan}
              onBack={() => setGeneratedPlan(null)}
              onSave={handleSavePlan}
            />
          ) : (
            <WeeklyPlanGenerator onPlanGenerated={setGeneratedPlan} />
          )}
        </TabsContent>
      </Tabs>

      {/* Link to old version */}
      <div className="mt-8 text-center">
        <Link href="/ai-workout/advanced" className="text-sm text-muted-foreground hover:underline">
          Prefere o formulario avancado? Clique aqui
        </Link>
      </div>
    </div>
  );
}

// Workout Result Component
function WorkoutResult({
  workout,
  onBack,
  onStart,
  onSave,
  isStarting,
}: {
  workout: QuickWorkoutResponse;
  onBack: () => void;
  onStart: () => void;
  onSave: () => void;
  isStarting: boolean;
}) {
  const [expandedExercise, setExpandedExercise] = useState<string | null>(null);

  const allExercises = [
    ...workout.warmup.map(e => ({ ...e, section: 'Aquecimento' })),
    ...workout.main.map(e => ({ ...e, section: 'Principal' })),
    ...workout.cooldown.map(e => ({ ...e, section: 'Alongamento' })),
  ];

  // Count exercises with video
  const withVideo = allExercises.filter(e => e.videoUrl).length;
  const withoutVideo = allExercises.length - withVideo;

  return (
    <Card className="max-w-2xl mx-auto">
      <CardHeader>
        <CardTitle>{workout.name}</CardTitle>
        <CardDescription>
          {workout.description} • {workout.estimatedDuration} minutos
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Video coverage info */}
        {withoutVideo > 0 && (
          <Alert>
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              {withVideo} de {allExercises.length} exercicios tem video demonstrativo.
            </AlertDescription>
          </Alert>
        )}

        {/* Exercise sections */}
        {workout.warmup.length > 0 && (
          <div>
            <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase">
              Aquecimento ({workout.warmup.length})
            </h3>
            <div className="space-y-2">
              {workout.warmup.map((ex, idx) => (
                <ExerciseCard
                  key={`warmup-${idx}`}
                  exercise={ex}
                  expanded={expandedExercise === `warmup-${idx}`}
                  onToggle={() => setExpandedExercise(
                    expandedExercise === `warmup-${idx}` ? null : `warmup-${idx}`
                  )}
                />
              ))}
            </div>
          </div>
        )}

        <div>
          <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase">
            Exercicios Principais ({workout.main.length})
          </h3>
          <div className="space-y-2">
            {workout.main.map((ex, idx) => (
              <ExerciseCard
                key={`main-${idx}`}
                exercise={ex}
                expanded={expandedExercise === `main-${idx}`}
                onToggle={() => setExpandedExercise(
                  expandedExercise === `main-${idx}` ? null : `main-${idx}`
                )}
              />
            ))}
          </div>
        </div>

        {workout.cooldown.length > 0 && (
          <div>
            <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase">
              Alongamento ({workout.cooldown.length})
            </h3>
            <div className="space-y-2">
              {workout.cooldown.map((ex, idx) => (
                <ExerciseCard
                  key={`cooldown-${idx}`}
                  exercise={ex}
                  expanded={expandedExercise === `cooldown-${idx}`}
                  onToggle={() => setExpandedExercise(
                    expandedExercise === `cooldown-${idx}` ? null : `cooldown-${idx}`
                  )}
                />
              ))}
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 pt-4 border-t">
          <Button variant="outline" onClick={onBack} className="flex-1">
            Voltar
          </Button>
          <Button variant="outline" onClick={onSave} className="flex-1">
            <Save className="h-4 w-4 mr-2" />
            Salvar
          </Button>
          <Button onClick={onStart} disabled={isStarting} className="flex-1">
            <Play className="h-4 w-4 mr-2" />
            {isStarting ? 'Iniciando...' : 'Iniciar Treino'}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// Exercise Card Component
function ExerciseCard({
  exercise,
  expanded,
  onToggle,
}: {
  exercise: GeneratedExercise;
  expanded: boolean;
  onToggle: () => void;
}) {
  return (
    <div className="border rounded-lg overflow-hidden">
      <button
        onClick={onToggle}
        className="w-full p-3 flex items-center justify-between hover:bg-muted/50 transition-colors"
      >
        <div className="flex items-center gap-3">
          {exercise.videoUrl && (
            <span className="text-xs bg-green-100 text-green-800 px-2 py-0.5 rounded">
              Video
            </span>
          )}
          <div className="text-left">
            <h4 className="font-medium">{exercise.exerciseName}</h4>
            <p className="text-sm text-muted-foreground">
              {exercise.muscleGroup} • {exercise.equipment}
            </p>
          </div>
        </div>
        <div className="text-sm text-muted-foreground">
          {exercise.sets} x {exercise.reps} • {exercise.restSeconds}s
        </div>
      </button>

      {expanded && (
        <div className="border-t p-3 bg-muted/30">
          {exercise.videoUrl ? (
            <div className="aspect-video">
              <iframe
                src={exercise.videoUrl.replace('watch?v=', 'embed/')}
                className="w-full h-full rounded-lg"
                allowFullScreen
              />
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-4">
              Video nao disponivel para este exercicio
            </p>
          )}
        </div>
      )}
    </div>
  );
}

// Plan Result Component
function PlanResult({
  plan,
  onBack,
  onSave,
}: {
  plan: WeeklyPlanResponse;
  onBack: () => void;
  onSave: () => void;
}) {
  const [selectedWeek, setSelectedWeek] = useState(0);
  const [expandedDay, setExpandedDay] = useState<number | null>(null);

  const currentWeek = plan.weeks[selectedWeek];

  return (
    <Card className="max-w-3xl mx-auto">
      <CardHeader>
        <CardTitle>{plan.name}</CardTitle>
        <CardDescription>
          {plan.description} • {plan.splitType} • {plan.weeksCount} semanas
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Week selector */}
        <div className="flex gap-2 overflow-x-auto pb-2">
          {plan.weeks.map((week, idx) => (
            <button
              key={idx}
              onClick={() => setSelectedWeek(idx)}
              className={`px-4 py-2 rounded-lg border whitespace-nowrap transition-all ${
                selectedWeek === idx
                  ? 'border-primary bg-primary text-primary-foreground'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              Semana {week.weekNumber}
              <span className="block text-xs opacity-75">{week.focus}</span>
            </button>
          ))}
        </div>

        {/* Current week workouts */}
        {currentWeek && (
          <div className="space-y-3">
            {currentWeek.workouts.map((day, idx) => (
              <div key={idx} className="border rounded-lg overflow-hidden">
                <button
                  onClick={() => setExpandedDay(expandedDay === idx ? null : idx)}
                  className="w-full p-4 flex items-center justify-between hover:bg-muted/50 transition-colors"
                >
                  <div className="text-left">
                    <span className="font-medium">{day.dayName}</span>
                    <span className="text-sm text-muted-foreground ml-2">• {day.focus}</span>
                  </div>
                  <span className="text-sm text-muted-foreground">
                    {day.exercises.length} exercicios
                  </span>
                </button>

                {expandedDay === idx && (
                  <div className="border-t p-4 space-y-2 bg-muted/30">
                    {day.exercises.map((ex, exIdx) => (
                      <div
                        key={exIdx}
                        className="flex items-center justify-between p-2 bg-background rounded"
                      >
                        <div className="flex items-center gap-2">
                          {ex.videoUrl && (
                            <span className="text-xs bg-green-100 text-green-800 px-2 py-0.5 rounded">
                              Video
                            </span>
                          )}
                          <span className="font-medium">{ex.exerciseName}</span>
                          <span className="text-sm text-muted-foreground">({ex.muscleGroup})</span>
                        </div>
                        <span className="text-sm">
                          {ex.sets} x {ex.reps}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Progression notes */}
        {plan.progressionNotes && (
          <div className="p-4 bg-muted/50 rounded-lg">
            <h4 className="font-medium mb-2">Progressao</h4>
            <p className="text-sm text-muted-foreground">{plan.progressionNotes}</p>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 pt-4 border-t">
          <Button variant="outline" onClick={onBack} className="flex-1">
            Voltar
          </Button>
          <Button onClick={onSave} className="flex-1">
            <Save className="h-4 w-4 mr-2" />
            Salvar Plano
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
