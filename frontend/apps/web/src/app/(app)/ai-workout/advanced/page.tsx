'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { ArrowLeft, Sparkles, Play, Save, Loader2 } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { useSession } from '@/hooks/use-session';
import { GenerateWorkoutForm, StructuredWorkoutRequest } from '@/components/ai-workout/generate-workout-form';
import Link from 'next/link';

// Response types from v1 endpoint
interface ExerciseInstruction {
  name: string;
  bodyPart: string;
  equipment: string;
  sets: number;
  reps: string;
  rest: string;
  instructions: string[];
  gifUrl?: string;
  videoUrl?: string;
  progressionNotes?: string;
  rpe?: string;
  tempo?: string;
  warmupSets?: string;
  exerciseType?: string;
}

interface AIWorkoutResponse {
  title: string;
  description: string;
  duration: number;
  exercises: ExerciseInstruction[];
}

export default function AdvancedAIWorkoutPage() {
  const router = useRouter();
  const { startSession, hasActiveSession, isStarting } = useSession();
  const [generatedWorkout, setGeneratedWorkout] = useState<AIWorkoutResponse | null>(null);

  const generateMutation = useMutation({
    mutationFn: async (request: StructuredWorkoutRequest) => {
      // Map frontend request to backend format
      const response = await apiClient.post<AIWorkoutResponse>('/ai/generate-workout', {
        prompt: request.prompt,
        fitnessLevel: request.fitnessLevel,
        duration: request.duration,
        equipment: request.equipment,
        workoutLocation: request.workoutLocation,
        includeWarmup: request.includeWarmup,
        includeCooldown: request.includeCooldown,
        includeMobility: request.includeMobility,
        goal: request.goal,
        secondaryGoal: request.secondaryGoal,
        targetMuscles: request.targetMuscles,
        priorityMuscles: request.priorityMuscles,
        avoidMuscles: request.avoidMuscles,
        injuries: request.injuries,
        restrictedExercises: request.restrictedExercises,
        favoriteExercises: request.favoriteExercises,
        trainingSplit: request.trainingSplit,
        intensityPreference: request.intensityPreference,
        setsPerMuscle: request.setsPerMuscle,
        restPreference: request.restPreference,
        additionalNotes: request.additionalNotes,
      });
      return response;
    },
    onSuccess: (data) => {
      setGeneratedWorkout(data);
      toast({
        title: 'Treino gerado!',
        description: `${data.title} - ${data.exercises.length} exercicios`,
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao gerar treino',
        description: error?.message || 'Tente novamente',
      });
    },
  });

  const handleGenerate = (request: StructuredWorkoutRequest) => {
    generateMutation.mutate(request);
  };

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

      // Store in localStorage for the workout page
      localStorage.setItem('ai_workout_exercises', JSON.stringify(generatedWorkout.exercises));
      localStorage.setItem('ai_workout_title', generatedWorkout.title);

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

  const handleSaveWorkout = async () => {
    if (!generatedWorkout) return;

    try {
      // Create a workout plan from the generated workout
      const planData = {
        name: generatedWorkout.title,
        goal: generatedWorkout.description,
        duration: 1,
      };

      const createdPlan = await apiClient.post<{ id: string }>('/workout-plans', planData);
      const planId = createdPlan.id;

      // Create a single workout day
      const workoutData = {
        name: generatedWorkout.title,
        dayOfWeek: null,
        order: 1,
      };

      const workoutResponse = await apiClient.post<{ id: string }>(
        `/workout-plans/${planId}/workouts`,
        workoutData
      );
      const workoutId = workoutResponse.id;

      // Note: exercises from v1 don't have exerciseId, so we can't link them directly
      // We would need to match by name or skip this step

      toast({
        title: 'Treino salvo!',
        description: 'Seu treino foi salvo em Meus Planos.',
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

  // Group exercises by type
  const groupedExercises = generatedWorkout ? {
    warmup: generatedWorkout.exercises.filter(e => e.exerciseType === 'warmup'),
    mobility: generatedWorkout.exercises.filter(e => e.exerciseType === 'mobility'),
    main: generatedWorkout.exercises.filter(e => e.exerciseType === 'main' || !e.exerciseType),
    cooldown: generatedWorkout.exercises.filter(e => e.exerciseType === 'cooldown'),
  } : null;

  return (
    <div className="container max-w-4xl py-8">
      {/* Header */}
      <div className="mb-8">
        <Link
          href="/ai-workout"
          className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-1" />
          Voltar para versao simplificada
        </Link>
        <div className="text-center">
          <h1 className="text-3xl font-bold flex items-center justify-center gap-2">
            <Sparkles className="h-8 w-8 text-primary" />
            Treino Avancado com IA
          </h1>
          <p className="text-muted-foreground mt-2">
            Personalize cada detalhe do seu treino
          </p>
        </div>
      </div>

      {/* Form or Result */}
      {generatedWorkout && groupedExercises ? (
        <Card>
          <CardHeader>
            <CardTitle>{generatedWorkout.title}</CardTitle>
            <CardDescription>
              {generatedWorkout.description} - {generatedWorkout.duration} minutos
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Warmup Section */}
            {groupedExercises.warmup.length > 0 && (
              <ExerciseSection title="Aquecimento" exercises={groupedExercises.warmup} />
            )}

            {/* Mobility Section */}
            {groupedExercises.mobility.length > 0 && (
              <ExerciseSection title="Mobilidade" exercises={groupedExercises.mobility} />
            )}

            {/* Main Exercises */}
            <ExerciseSection title="Exercicios Principais" exercises={groupedExercises.main} />

            {/* Cooldown Section */}
            {groupedExercises.cooldown.length > 0 && (
              <ExerciseSection title="Alongamento" exercises={groupedExercises.cooldown} />
            )}

            {/* Actions */}
            <div className="flex gap-3 pt-4 border-t">
              <Button
                variant="outline"
                onClick={() => setGeneratedWorkout(null)}
                className="flex-1"
              >
                Gerar Novo
              </Button>
              <Button
                variant="outline"
                onClick={handleSaveWorkout}
                className="flex-1"
              >
                <Save className="h-4 w-4 mr-2" />
                Salvar
              </Button>
              <Button
                onClick={handleStartWorkout}
                disabled={isStarting}
                className="flex-1"
              >
                <Play className="h-4 w-4 mr-2" />
                {isStarting ? 'Iniciando...' : 'Iniciar Treino'}
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <GenerateWorkoutForm
          onGenerate={handleGenerate}
          isLoading={generateMutation.isPending}
        />
      )}
    </div>
  );
}

// Exercise Section Component
function ExerciseSection({
  title,
  exercises,
}: {
  title: string;
  exercises: ExerciseInstruction[];
}) {
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);

  return (
    <div>
      <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase">
        {title} ({exercises.length})
      </h3>
      <div className="space-y-2">
        {exercises.map((exercise, idx) => (
          <div key={idx} className="border rounded-lg overflow-hidden">
            <button
              onClick={() => setExpandedIndex(expandedIndex === idx ? null : idx)}
              className="w-full p-3 flex items-center justify-between hover:bg-muted/50 transition-colors text-left"
            >
              <div className="flex items-center gap-3">
                {exercise.videoUrl && (
                  <span className="text-xs bg-green-100 text-green-800 px-2 py-0.5 rounded dark:bg-green-900 dark:text-green-100">
                    Video
                  </span>
                )}
                <div>
                  <h4 className="font-medium">{exercise.name}</h4>
                  <p className="text-sm text-muted-foreground">
                    {exercise.bodyPart} - {exercise.equipment}
                  </p>
                </div>
              </div>
              <div className="text-sm text-muted-foreground whitespace-nowrap">
                {exercise.sets} x {exercise.reps} - {exercise.rest}
              </div>
            </button>

            {expandedIndex === idx && (
              <div className="border-t p-4 bg-muted/30 space-y-4">
                {/* Video */}
                {exercise.videoUrl && (
                  <div className="aspect-video">
                    <iframe
                      src={exercise.videoUrl.replace('watch?v=', 'embed/')}
                      className="w-full h-full rounded-lg"
                      allowFullScreen
                    />
                  </div>
                )}

                {/* Instructions */}
                {exercise.instructions.length > 0 && (
                  <div>
                    <h5 className="font-medium text-sm mb-2">Instrucoes:</h5>
                    <ul className="list-disc list-inside text-sm text-muted-foreground space-y-1">
                      {exercise.instructions.map((instruction, i) => (
                        <li key={i}>{instruction}</li>
                      ))}
                    </ul>
                  </div>
                )}

                {/* Additional info */}
                <div className="grid grid-cols-2 gap-4 text-sm">
                  {exercise.rpe && (
                    <div>
                      <span className="font-medium">RPE: </span>
                      <span className="text-muted-foreground">{exercise.rpe}</span>
                    </div>
                  )}
                  {exercise.tempo && (
                    <div>
                      <span className="font-medium">Tempo: </span>
                      <span className="text-muted-foreground">{exercise.tempo}</span>
                    </div>
                  )}
                  {exercise.warmupSets && (
                    <div className="col-span-2">
                      <span className="font-medium">Series de aquecimento: </span>
                      <span className="text-muted-foreground">{exercise.warmupSets}</span>
                    </div>
                  )}
                  {exercise.progressionNotes && (
                    <div className="col-span-2">
                      <span className="font-medium">Progressao: </span>
                      <span className="text-muted-foreground">{exercise.progressionNotes}</span>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
