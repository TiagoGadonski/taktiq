'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Sparkles, Loader2, Dumbbell, Calendar, Share2, RefreshCw, Info } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import Link from 'next/link';

// Types
interface Exercise {
  name: string;
  bodyPart: string;
  equipment: string;
  sets: number;
  reps: string;
  rest: string;
  instructions: string[];
  gifUrl?: string;
  videoUrl?: string;
}

interface AIWorkoutResponse {
  title: string;
  description: string;
  duration: number;
  exercises: Exercise[];
}

interface WorkoutDay {
  dayName: string;
  title: string;
  focus: string;
  exercises: Exercise[];
}

interface AIWorkoutPlanResponse {
  title: string;
  description: string;
  weeksCount: number;
  daysPerWeek: number;
  goal: string;
  days: WorkoutDay[];
}

interface Friend {
  friendshipId: string;
  friendId: string;
  friendName: string;
  friendEmail: string;
}

interface UserProfile {
  injuries?: string | null;
  healthConditions?: string | null;
  exerciseGoal?: string | null;
  trainingSplit?: string | null;
}

export default function AIWorkoutPage() {
  // Single workout state
  const [prompt, setPrompt] = useState('');
  const [fitnessLevel, setFitnessLevel] = useState<'beginner' | 'intermediate' | 'advanced'>('intermediate');
  const [duration, setDuration] = useState(45); // Default 45 minutes
  const [generatedWorkout, setGeneratedWorkout] = useState<AIWorkoutResponse | null>(null);
  const [rejectedExercises, setRejectedExercises] = useState<Record<number, string[]>>({});

  // Plan state
  const [planPrompt, setPlanPrompt] = useState('');
  const [planFitnessLevel, setPlanFitnessLevel] = useState<'beginner' | 'intermediate' | 'advanced'>('intermediate');
  const [daysPerWeek, setDaysPerWeek] = useState(4);
  const [generatedPlan, setGeneratedPlan] = useState<AIWorkoutPlanResponse | null>(null);
  const [savedPlanId, setSavedPlanId] = useState<string | null>(null);
  const [rejectedPlanExercises, setRejectedPlanExercises] = useState<Record<string, string[]>>({});

  // Exercise detail modal state
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null);
  const [isExerciseModalOpen, setIsExerciseModalOpen] = useState(false);

  // Share workout state
  const [isShareDialogOpen, setIsShareDialogOpen] = useState(false);
  const [workoutToShare, setWorkoutToShare] = useState<'single' | 'plan' | null>(null);
  const [selectedFriends, setSelectedFriends] = useState<string[]>([]);

  const queryClient = useQueryClient();
  const router = useRouter();

  const openExerciseModal = (exercise: Exercise) => {
    setSelectedExercise(exercise);
    setIsExerciseModalOpen(true);
  };

  const openShareDialog = (workoutType: 'single' | 'plan') => {
    setWorkoutToShare(workoutType);
    setIsShareDialogOpen(true);
    setSelectedFriends([]);
  };

  const toggleFriendSelection = (friendId: string) => {
    setSelectedFriends((prev) =>
      prev.includes(friendId) ? prev.filter((id) => id !== friendId) : [...prev, friendId]
    );
  };

  // Fetch friends for sharing
  const { data: friends = [] } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: async () => {
      return apiClient.get('/friends');
    },
  });

  // Fetch user profile for completeness check
  const { data: userProfile } = useQuery<UserProfile>({
    queryKey: ['user-profile'],
    queryFn: async () => {
      return apiClient.get('/me');
    },
  });

  // Calculate profile completeness
  const profileFields = [
    userProfile?.injuries,
    userProfile?.healthConditions,
    userProfile?.exerciseGoal,
  ];
  const filledFields = profileFields.filter(field => field && field.trim() !== '').length;
  const profileCompleteness = Math.round((filledFields / profileFields.length) * 100);
  const isProfileIncomplete = profileCompleteness < 100;

  // Get today's training split suggestion
  const getTodaySplit = () => {
    if (!userProfile?.trainingSplit) return null;
    try {
      const split = JSON.parse(userProfile.trainingSplit);
      const today = new Date().getDay(); // 0 = Sunday, 1 = Monday, etc.
      return split[today.toString()] || null;
    } catch {
      return null;
    }
  };

  const todaysSplit = getTodaySplit();

  const useTodaySuggestion = () => {
    if (todaysSplit) {
      setPrompt(`Treino de ${todaysSplit} para hipertrofia`);
      toast({
        title: 'Sugestão aplicada!',
        description: `Prompt preenchido com o treino de hoje: ${todaysSplit}`,
      });
    }
  };

  // Single workout mutation
  const generateWorkoutMutation = useMutation({
    mutationFn: async (request: { prompt: string; fitnessLevel?: string }) => {
      return apiClient.post<AIWorkoutResponse>('/ai/generate-workout', request);
    },
    onSuccess: (data) => {
      setGeneratedWorkout(data);
      setRejectedExercises({}); // Reset rejected exercises for new workout
      toast({
        title: 'Treino gerado com sucesso!',
        description: 'Seu treino personalizado está pronto.',
      });
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao gerar treino',
        description: error instanceof Error ? error.message : 'Tente novamente mais tarde',
      });
    },
  });

  // Plan mutation
  const generatePlanMutation = useMutation({
    mutationFn: async (request: { prompt: string; fitnessLevel?: string; daysPerWeek?: number }) => {
      return apiClient.post<AIWorkoutPlanResponse>('/ai/generate-plan', request);
    },
    onSuccess: (data) => {
      setGeneratedPlan(data);
      setRejectedPlanExercises({}); // Reset rejected exercises for new plan
      toast({
        title: 'Plano de treino gerado com sucesso!',
        description: `Seu plano de ${data.daysPerWeek} dias está pronto.`,
      });
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao gerar plano',
        description: error instanceof Error ? error.message : 'Tente novamente mais tarde',
      });
    },
  });

  const handleGenerateWorkout = () => {
    if (!prompt.trim()) {
      toast({
        variant: 'destructive',
        title: 'Digite sua solicitação',
        description: 'Descreva o tipo de treino que você deseja',
      });
      return;
    }
    // Add duration to prompt if not already mentioned
    const finalPrompt = prompt.toLowerCase().includes('minuto')
      ? prompt
      : `${prompt}, ${duration} minutos`;
    generateWorkoutMutation.mutate({ prompt: finalPrompt, fitnessLevel });
  };

  const handleGeneratePlan = () => {
    if (!planPrompt.trim()) {
      toast({
        variant: 'destructive',
        title: 'Digite sua solicitação',
        description: 'Descreva o plano de treino que você deseja',
      });
      return;
    }
    generatePlanMutation.mutate({ prompt: planPrompt, fitnessLevel: planFitnessLevel, daysPerWeek });
  };

  // Replace exercise function - generates a new alternative
  const handleReplaceExercise = async (exerciseIndex: number) => {
    if (!generatedWorkout) return;

    try {
      const exerciseToReplace = generatedWorkout.exercises[exerciseIndex];

      // Track this exercise as rejected
      const currentRejected = rejectedExercises[exerciseIndex] || [];
      const allRejected = [...currentRejected, exerciseToReplace.name];

      // Get all existing exercise names in the workout (excluding the one being replaced)
      const existingExerciseNames = generatedWorkout.exercises
        .filter((_, idx) => idx !== exerciseIndex)
        .map(ex => ex.name);

      // Combine rejected and existing exercises for exclusion
      const allExcluded = Array.from(new Set([...allRejected, ...existingExerciseNames]));

      // Build exclusion list for prompt
      const exclusionList = allExcluded.join(', ');

      // Generate a single-exercise workout targeting the same muscle group, excluding all rejected and existing ones
      const replacementPrompt = `Um exercício de ${exerciseToReplace.bodyPart} diferente de: ${exclusionList}`;

      const response = await apiClient.post<AIWorkoutResponse>('/ai/generate-workout', {
        prompt: replacementPrompt,
        fitnessLevel,
      });

      if (response.exercises && response.exercises.length > 0) {
        const newExercise = response.exercises[0];

        // Check if the new exercise is already in the workout
        const isDuplicate = generatedWorkout.exercises.some(
          (ex, idx) => idx !== exerciseIndex && ex.name.toLowerCase() === newExercise.name.toLowerCase()
        );

        if (isDuplicate) {
          toast({
            variant: 'destructive',
            title: 'Exercício duplicado',
            description: 'Este exercício já está no treino. Tentando novamente...',
          });
          // Recursively try again
          await handleReplaceExercise(exerciseIndex);
          return;
        }

        // Update rejected exercises list
        setRejectedExercises({
          ...rejectedExercises,
          [exerciseIndex]: allRejected,
        });

        // Update the workout with the new exercise
        const updatedExercises = [...generatedWorkout.exercises];
        updatedExercises[exerciseIndex] = newExercise;

        setGeneratedWorkout({
          ...generatedWorkout,
          exercises: updatedExercises,
        });

        toast({
          title: 'Exercício substituído!',
          description: `${exerciseToReplace.name} → ${newExercise.name}`,
        });
      }
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Erro ao substituir exercício',
        description: 'Tente novamente',
      });
    }
  };

  // Replace exercise in plan
  const handleReplacePlanExercise = async (dayIndex: number, exerciseIndex: number) => {
    if (!generatedPlan) return;

    try {
      const day = generatedPlan.days[dayIndex];
      const exerciseToReplace = day.exercises[exerciseIndex];

      // Create unique key for this position in the plan
      const positionKey = `${dayIndex}-${exerciseIndex}`;

      // Track this exercise as rejected
      const currentRejected = rejectedPlanExercises[positionKey] || [];
      const allRejected = [...currentRejected, exerciseToReplace.name];

      // Get all existing exercise names in THIS DAY (excluding the one being replaced)
      const existingExerciseNamesInDay = day.exercises
        .filter((_, idx) => idx !== exerciseIndex)
        .map(ex => ex.name);

      // Combine rejected and existing exercises for exclusion
      const allExcluded = Array.from(new Set([...allRejected, ...existingExerciseNamesInDay]));

      // Build exclusion list for prompt
      const exclusionList = allExcluded.join(', ');

      // Generate a replacement excluding all rejected and existing ones
      const replacementPrompt = `Um exercício de ${exerciseToReplace.bodyPart} diferente de: ${exclusionList}`;

      const response = await apiClient.post<AIWorkoutResponse>('/ai/generate-workout', {
        prompt: replacementPrompt,
        fitnessLevel: planFitnessLevel,
      });

      if (response.exercises && response.exercises.length > 0) {
        const newExercise = response.exercises[0];

        // Check if the new exercise is already in this day's workout
        const isDuplicate = day.exercises.some(
          (ex, idx) => idx !== exerciseIndex && ex.name.toLowerCase() === newExercise.name.toLowerCase()
        );

        if (isDuplicate) {
          toast({
            variant: 'destructive',
            title: 'Exercício duplicado',
            description: 'Este exercício já está neste dia. Tentando novamente...',
          });
          // Recursively try again
          await handleReplacePlanExercise(dayIndex, exerciseIndex);
          return;
        }

        // Update rejected exercises list
        setRejectedPlanExercises({
          ...rejectedPlanExercises,
          [positionKey]: allRejected,
        });

        // Update the plan with the new exercise
        const updatedDays = [...generatedPlan.days];
        const updatedExercises = [...updatedDays[dayIndex].exercises];
        updatedExercises[exerciseIndex] = newExercise;
        updatedDays[dayIndex] = {
          ...updatedDays[dayIndex],
          exercises: updatedExercises,
        };

        setGeneratedPlan({
          ...generatedPlan,
          days: updatedDays,
        });

        toast({
          title: 'Exercício substituído!',
          description: `${exerciseToReplace.name} → ${newExercise.name}`,
        });
      }
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Erro ao substituir exercício',
        description: 'Tente novamente',
      });
    }
  };

  const handleSavePlan = async () => {
    if (!generatedPlan) return;

    try {
      // Step 1: Create the workout plan
      const planData = {
        name: generatedPlan.title,
        goal: generatedPlan.goal,
      };

      const createdPlan = await apiClient.post<{ id: string }>('/workout-plans', planData);
      const planId = createdPlan.id;

      if (!planId) {
        throw new Error('Plan ID not returned from server');
      }

      // Step 2: Get all exercises from the database to check which ones already exist
      const existingExercises = await apiClient.get<any[]>('/exercises');
      const exerciseMap = new Map(existingExercises.map((e: any) => [e.name.toLowerCase(), e]));

      // Step 3: Create workouts (days) and add exercises to them
      for (let dayIndex = 0; dayIndex < generatedPlan.days.length; dayIndex++) {
        const day = generatedPlan.days[dayIndex];

        // Create the workout (day)
        const workoutData = {
          name: day.title || day.dayName || `Dia ${dayIndex + 1}`,
          dayOfWeek: null,
          order: dayIndex + 1,
        };

        const workoutResponse = await apiClient.post<{ id: string }>(`/workout-plans/${planId}/workouts`, workoutData);
        const workoutId = workoutResponse.id;

        // Add exercises to this workout
        for (let exIndex = 0; exIndex < day.exercises.length; exIndex++) {
          const ex = day.exercises[exIndex];
          let exerciseId: string;

          // Check if exercise already exists in database
          const existingExercise = exerciseMap.get(ex.name.toLowerCase());

          if (existingExercise) {
            // Check if existing exercise needs video URL update
            const needsUpdate = (!existingExercise.videoUrl && ex.videoUrl) || (!existingExercise.imageUrl && ex.gifUrl);

            if (needsUpdate) {
              await apiClient.put(`/exercises/${existingExercise.id}`, {
                name: existingExercise.name,
                muscleGroup: existingExercise.muscleGroup,
                category: existingExercise.category,
                equipment: existingExercise.equipment,
                notes: ex.instructions ? ex.instructions.join('. ') : existingExercise.notes,
                videoUrl: ex.videoUrl || existingExercise.videoUrl,
                imageUrl: ex.gifUrl || existingExercise.imageUrl,
              });
            }

            exerciseId = existingExercise.id;
          } else {
            // Create new exercise
            const newExercise = await apiClient.post<{ id: string }>('/exercises', {
              name: ex.name,
              muscleGroup: ex.bodyPart || 'Other',
              category: 'strength',
              equipment: ex.equipment || 'bodyweight',
              notes: ex.instructions ? ex.instructions.join('. ') : null,
              videoUrl: ex.videoUrl || null,
              imageUrl: ex.gifUrl || null,
            });
            exerciseId = newExercise.id;
            exerciseMap.set(ex.name.toLowerCase(), newExercise);
          }

          // Add exercise to the workout day
          const exerciseData = {
            exerciseId: exerciseId,
            order: exIndex + 1,
            targetSets: ex.sets,
            targetReps: typeof ex.reps === 'string' ? parseInt(ex.reps.split('-')[0]) || 12 : ex.reps,
            targetLoad: 0,
          };

          await apiClient.post(`/workout-plans/${planId}/workouts/${workoutId}/exercises`, exerciseData);
        }
      }

      toast({
        title: 'Plano salvo!',
        description: 'Seu plano de treino foi salvo com sucesso.',
      });

      // Redirect to plans page
      router.push('/plans');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao salvar plano',
        description: error.response?.data?.message || error.message || 'Não foi possível salvar o plano',
      });
    }
  };

  const handleStartPlan = async () => {
    if (!generatedPlan) return;

    try {
      // Step 1: Save the plan first
      const planData = {
        name: generatedPlan.title,
        goal: generatedPlan.goal,
      };

      const createdPlan = await apiClient.post<{ id: string }>('/workout-plans', planData);
      const planId = createdPlan.id;

      if (!planId) {
        throw new Error('Plan ID not returned from server');
      }

      // Save plan ID in state for sharing
      setSavedPlanId(planId);

      // Step 2: Get all exercises from the database to check which ones already exist
      const existingExercises = await apiClient.get<any[]>('/exercises');
      const exerciseMap = new Map(existingExercises.map((e: any) => [e.name.toLowerCase(), e]));

      // Step 3: Create workouts (days) and add exercises to them
      for (let dayIndex = 0; dayIndex < generatedPlan.days.length; dayIndex++) {
        const day = generatedPlan.days[dayIndex];

        // Create the workout (day)
        const workoutData = {
          name: day.title || day.dayName || `Dia ${dayIndex + 1}`,
          dayOfWeek: null,
          order: dayIndex + 1,
        };

        const workoutResponse = await apiClient.post<{ id: string }>(`/workout-plans/${planId}/workouts`, workoutData);
        const workoutId = workoutResponse.id;

        // Add exercises to this workout
        for (let exIndex = 0; exIndex < day.exercises.length; exIndex++) {
          const ex = day.exercises[exIndex];
          let exerciseId: string;

          const existingExercise = exerciseMap.get(ex.name.toLowerCase());

          if (existingExercise) {
            // Check if existing exercise needs video URL update
            const needsUpdate = (!existingExercise.videoUrl && ex.videoUrl) || (!existingExercise.imageUrl && ex.gifUrl);

            if (needsUpdate) {
              await apiClient.put(`/exercises/${existingExercise.id}`, {
                name: existingExercise.name,
                muscleGroup: existingExercise.muscleGroup,
                category: existingExercise.category,
                equipment: existingExercise.equipment,
                notes: ex.instructions ? ex.instructions.join('. ') : existingExercise.notes,
                videoUrl: ex.videoUrl || existingExercise.videoUrl,
                imageUrl: ex.gifUrl || existingExercise.imageUrl,
              });
            }

            exerciseId = existingExercise.id;
          } else {
            const newExercise = await apiClient.post<{ id: string }>('/exercises', {
              name: ex.name,
              muscleGroup: ex.bodyPart || 'Other',
              category: 'strength',
              equipment: ex.equipment || 'bodyweight',
              notes: ex.instructions ? ex.instructions.join('. ') : null,
              videoUrl: ex.videoUrl || null,
              imageUrl: ex.gifUrl || null,
            });
            exerciseId = newExercise.id;
            exerciseMap.set(ex.name.toLowerCase(), newExercise);
          }

          const exerciseData = {
            exerciseId: exerciseId,
            order: exIndex + 1,
            targetSets: ex.sets,
            targetReps: typeof ex.reps === 'string' ? parseInt(ex.reps.split('-')[0]) || 12 : ex.reps,
            targetLoad: 0,
          };

          await apiClient.post(`/workout-plans/${planId}/workouts/${workoutId}/exercises`, exerciseData);
        }
      }

      // Step 4: Activate the plan
      await apiClient.patch(`/workout-plans/${planId}/activate`);

      toast({
        title: 'Plano ativado!',
        description: 'Seu plano foi salvo e ativado. Redirecionando para iniciar o treino...',
      });

      // Redirect to workout page
      setTimeout(() => {
        router.push('/workout');
      }, 1000);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao iniciar plano',
        description: error.response?.data?.message || error.message || 'Não foi possível iniciar o plano',
      });
    }
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <div>
        <h1 className="text-2xl font-bold sm:text-3xl flex items-center gap-2">
          <Sparkles className="h-7 w-7 sm:h-8 sm:w-8 text-primary" />
          Gerador de Treino com IA
        </h1>
        <p className="text-sm text-muted-foreground sm:text-base mt-1">
          Crie treinos únicos ou planos semanais completos com inteligência artificial
        </p>
      </div>

      {/* Profile Completeness Banner */}
      {isProfileIncomplete && (
        <Alert className="border-blue-200 bg-blue-50 dark:bg-blue-950/20">
          <Info className="h-4 w-4 text-blue-600 dark:text-blue-400" />
          <AlertTitle className="text-blue-900 dark:text-blue-100">
            Melhore seus treinos com IA
          </AlertTitle>
          <AlertDescription className="text-blue-800 dark:text-blue-200">
            Seu perfil está {profileCompleteness}% completo. Quanto mais informações você fornecer sobre lesões,
            condições de saúde e objetivos, melhor a IA poderá personalizar seus treinos para você!{' '}
            <Link href="/profile" className="font-medium underline underline-offset-4 hover:text-blue-600">
              Complete seu perfil
            </Link>
          </AlertDescription>
        </Alert>
      )}

      {/* Training Split Suggestion Banner */}
      {todaysSplit && (
        <Alert className="border-primary/30 bg-primary/5 dark:bg-primary/10">
          <Calendar className="h-4 w-4 text-primary" />
          <AlertTitle className="text-foreground flex items-center gap-2">
            📅 Sugestão de Hoje
          </AlertTitle>
          <AlertDescription className="text-muted-foreground flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
            <span>
              Seu treino de hoje é: <strong className="text-foreground">{todaysSplit}</strong>
            </span>
            <div className="flex gap-2">
              <Button
                size="sm"
                variant="outline"
                onClick={useTodaySuggestion}
                className="hover-lift tap-scale"
              >
                <Sparkles className="mr-1 h-3 w-3" />
                Usar Sugestão
              </Button>
              <Link href="/training-split">
                <Button size="sm" variant="ghost" className="hover-lift tap-scale">
                  Configurar
                </Button>
              </Link>
            </div>
          </AlertDescription>
        </Alert>
      )}

      {/* No Training Split Banner */}
      {!todaysSplit && userProfile && (
        <Alert className="border-amber-200 bg-amber-50 dark:bg-amber-950/20">
          <Info className="h-4 w-4 text-amber-600 dark:text-amber-400" />
          <AlertTitle className="text-amber-900 dark:text-amber-100">
            Configure sua divisão de treinos
          </AlertTitle>
          <AlertDescription className="text-amber-800 dark:text-amber-200">
            Crie uma divisão semanal e receba sugestões automáticas de treino para cada dia!{' '}
            <Link href="/training-split" className="font-medium underline underline-offset-4 hover:text-amber-600">
              Configurar agora
            </Link>
          </AlertDescription>
        </Alert>
      )}

      <Tabs defaultValue="workout" className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="workout" className="flex items-center gap-2">
            <Dumbbell className="h-4 w-4" />
            Treino Único
          </TabsTrigger>
          <TabsTrigger value="plan" className="flex items-center gap-2">
            <Calendar className="h-4 w-4" />
            Plano Semanal
          </TabsTrigger>
        </TabsList>

        {/* Single Workout Tab */}
        <TabsContent value="workout" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Crie um treino personalizado</CardTitle>
              <CardDescription>
                Descreva o treino que você quer e a IA criará um plano detalhado com exercícios e instruções
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="prompt">Descreva seu treino ideal</Label>
                <textarea
                  id="prompt"
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder="Ex: Treino de peito e tríceps para hipertrofia..."
                  className="w-full min-h-[120px] p-3 rounded-md border border-input bg-background text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
                  disabled={generateWorkoutMutation.isPending}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="duration">Duração do Treino: {duration} minutos</Label>
                <input
                  id="duration"
                  type="range"
                  min="15"
                  max="120"
                  step="15"
                  value={duration}
                  onChange={(e) => setDuration(parseInt(e.target.value))}
                  disabled={generateWorkoutMutation.isPending}
                  className="w-full h-2 bg-secondary rounded-lg appearance-none cursor-pointer accent-primary"
                />
                <div className="flex justify-between text-xs text-muted-foreground">
                  <span>15 min</span>
                  <span>30 min</span>
                  <span>45 min</span>
                  <span>60 min</span>
                  <span>90 min</span>
                  <span>120 min</span>
                </div>
              </div>

              <div className="space-y-2">
                <Label>Nível de Condicionamento</Label>
                <div className="grid grid-cols-3 gap-2">
                  {(['beginner', 'intermediate', 'advanced'] as const).map((level) => (
                    <button
                      key={level}
                      onClick={() => setFitnessLevel(level)}
                      disabled={generateWorkoutMutation.isPending}
                      className={`px-3 py-2 text-sm rounded-md border transition-colors ${
                        fitnessLevel === level
                          ? 'bg-primary text-primary-foreground border-primary'
                          : 'bg-background border-input hover:bg-accent'
                      }`}
                    >
                      {level === 'beginner' && 'Iniciante'}
                      {level === 'intermediate' && 'Intermediário'}
                      {level === 'advanced' && 'Avançado'}
                    </button>
                  ))}
                </div>
              </div>

              <Button
                onClick={handleGenerateWorkout}
                disabled={generateWorkoutMutation.isPending}
                className="w-full"
                size="lg"
              >
                {generateWorkoutMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Gerando treino...
                  </>
                ) : (
                  <>
                    <Sparkles className="mr-2 h-4 w-4" />
                    Gerar Treino
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Display generated workout */}
          {generatedWorkout && (
            <Card>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="flex items-center gap-2">
                      <Dumbbell className="h-5 w-5" />
                      {generatedWorkout.title}
                    </CardTitle>
                    <CardDescription>{generatedWorkout.description}</CardDescription>
                    <p className="text-sm text-muted-foreground">
                      Duração estimada: {generatedWorkout.duration} minutos
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => openShareDialog('single')}
                    className="ml-2"
                  >
                    <Share2 className="h-4 w-4 mr-2" />
                    Compartilhar
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {generatedWorkout.exercises.map((exercise, index) => (
                    <Card
                      key={index}
                      className="border-l-4 border-l-primary cursor-pointer hover:shadow-lg transition-shadow"
                      onClick={() => openExerciseModal(exercise)}
                    >
                      <CardContent className="pt-6">
                        <div className="flex flex-col sm:flex-row gap-4">
                          {exercise.videoUrl && (
                            <div className="sm:w-48 shrink-0">
                              <a
                                href={exercise.videoUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="block relative group"
                              >
                                <img
                                  src={`https://img.youtube.com/vi/${exercise.videoUrl.split('v=')[1]}/mqdefault.jpg`}
                                  alt={exercise.name}
                                  className="w-full h-auto rounded-md bg-muted"
                                />
                                <div className="absolute inset-0 flex items-center justify-center bg-black/30 group-hover:bg-black/50 transition-colors rounded-md">
                                  <div className="w-12 h-12 bg-red-600 rounded-full flex items-center justify-center">
                                    <svg className="w-6 h-6 text-white ml-1" fill="currentColor" viewBox="0 0 24 24">
                                      <path d="M8 5v14l11-7z"/>
                                    </svg>
                                  </div>
                                </div>
                              </a>
                            </div>
                          )}
                          {!exercise.videoUrl && exercise.gifUrl && (
                            <div className="sm:w-48 shrink-0">
                              <img
                                src={exercise.gifUrl}
                                alt={exercise.name}
                                className="w-full h-auto rounded-md bg-muted"
                              />
                            </div>
                          )}
                          <div className="flex-1 space-y-2">
                            <div className="flex items-start justify-between gap-2">
                              <h3 className="font-semibold text-lg flex-1">
                                {index + 1}. {exercise.name}
                              </h3>
                              <Button
                                variant="ghost"
                                size="sm"
                                className="shrink-0"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleReplaceExercise(index);
                                }}
                                title="Substituir exercício"
                              >
                                <RefreshCw className="h-4 w-4" />
                              </Button>
                            </div>
                            <div className="flex flex-wrap gap-2 text-xs sm:text-sm">
                              <span className="px-2 py-1 bg-secondary rounded-full">
                                {exercise.bodyPart}
                              </span>
                              <span className="px-2 py-1 bg-secondary rounded-full">
                                {exercise.equipment}
                              </span>
                            </div>
                            <div className="flex flex-wrap gap-2 text-sm font-medium">
                              <div className="px-3 py-1 bg-primary/10 rounded-full">
                                {exercise.sets} séries
                              </div>
                              <div className="px-3 py-1 bg-primary/10 rounded-full">
                                {exercise.reps} reps
                              </div>
                              <div className="px-3 py-1 bg-primary/10 rounded-full">
                                Descanso: {exercise.rest}
                              </div>
                            </div>
                            <div className="pt-2">
                              <p className="text-sm font-medium mb-1">Instruções:</p>
                              <ol className="text-sm text-muted-foreground space-y-1 list-decimal list-inside">
                                {exercise.instructions.map((instruction, i) => (
                                  <li key={i}>{instruction}</li>
                                ))}
                              </ol>
                            </div>
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Weekly Plan Tab */}
        <TabsContent value="plan" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Crie um plano semanal completo</CardTitle>
              <CardDescription>
                Descreva seus objetivos e a IA criará um plano de treino completo para a semana
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="plan-prompt">Descreva seus objetivos</Label>
                <textarea
                  id="plan-prompt"
                  value={planPrompt}
                  onChange={(e) => setPlanPrompt(e.target.value)}
                  placeholder="Ex: Quero ganhar massa muscular, tenho equipamento completo de academia..."
                  className="w-full min-h-[120px] p-3 rounded-md border border-input bg-background text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
                  disabled={generatePlanMutation.isPending}
                />
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Nível de Condicionamento</Label>
                  <div className="grid grid-cols-3 gap-2">
                    {(['beginner', 'intermediate', 'advanced'] as const).map((level) => (
                      <button
                        key={level}
                        onClick={() => setPlanFitnessLevel(level)}
                        disabled={generatePlanMutation.isPending}
                        className={`px-2 py-2 text-xs sm:text-sm rounded-md border transition-colors ${
                          planFitnessLevel === level
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-background border-input hover:bg-accent'
                        }`}
                      >
                        {level === 'beginner' && 'Iniciante'}
                        {level === 'intermediate' && 'Interm.'}
                        {level === 'advanced' && 'Avançado'}
                      </button>
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>Dias por Semana</Label>
                  <div className="grid grid-cols-4 gap-2">
                    {[2, 3, 4, 5].map((days) => (
                      <button
                        key={days}
                        onClick={() => setDaysPerWeek(days)}
                        disabled={generatePlanMutation.isPending}
                        className={`px-3 py-2 text-sm rounded-md border transition-colors ${
                          daysPerWeek === days
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-background border-input hover:bg-accent'
                        }`}
                      >
                        {days}
                      </button>
                    ))}
                  </div>
                </div>
              </div>

              <Button
                onClick={handleGeneratePlan}
                disabled={generatePlanMutation.isPending}
                className="w-full"
                size="lg"
              >
                {generatePlanMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Gerando plano...
                  </>
                ) : (
                  <>
                    <Calendar className="mr-2 h-4 w-4" />
                    Gerar Plano Semanal
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Display generated plan */}
          {generatedPlan && (
            <Card>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="flex items-center gap-2">
                      <Calendar className="h-5 w-5" />
                      {generatedPlan.title}
                    </CardTitle>
                    <CardDescription>{generatedPlan.description}</CardDescription>
                    <div className="flex flex-wrap gap-2 text-sm text-muted-foreground mt-2">
                      <span>{generatedPlan.daysPerWeek} dias por semana</span>
                      <span>•</span>
                      <span>{generatedPlan.weeksCount} semanas</span>
                      <span>•</span>
                      <span>{generatedPlan.goal}</span>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => openShareDialog('plan')}
                    className="ml-2"
                  >
                    <Share2 className="h-4 w-4 mr-2" />
                    Compartilhar
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="space-y-6">
                {generatedPlan.days.map((day, dayIndex) => (
                  <Card key={dayIndex} className="border-2">
                    <CardHeader className="bg-muted/50">
                      <CardTitle className="text-lg">{day.dayName}: {day.title}</CardTitle>
                      <CardDescription>{day.focus}</CardDescription>
                    </CardHeader>
                    <CardContent className="pt-4 space-y-3">
                      {day.exercises.map((exercise, exIndex) => (
                        <Card
                          key={exIndex}
                          className="border-l-4 border-l-primary/50 cursor-pointer hover:shadow-lg transition-shadow"
                          onClick={() => openExerciseModal(exercise)}
                        >
                          <CardContent className="pt-4">
                            <div className="flex flex-col sm:flex-row gap-4">
                              {exercise.videoUrl && (
                                <div className="sm:w-32 shrink-0">
                                  <a
                                    href={exercise.videoUrl}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="block relative group"
                                    onClick={(e) => e.stopPropagation()}
                                  >
                                    <img
                                      src={`https://img.youtube.com/vi/${exercise.videoUrl.split('v=')[1]}/mqdefault.jpg`}
                                      alt={exercise.name}
                                      className="w-full h-auto rounded-md bg-muted"
                                    />
                                    <div className="absolute inset-0 flex items-center justify-center bg-black/30 group-hover:bg-black/50 transition-colors rounded-md">
                                      <div className="w-10 h-10 bg-red-600 rounded-full flex items-center justify-center">
                                        <svg className="w-5 h-5 text-white ml-0.5" fill="currentColor" viewBox="0 0 24 24">
                                          <path d="M8 5v14l11-7z"/>
                                        </svg>
                                      </div>
                                    </div>
                                  </a>
                                </div>
                              )}
                              {!exercise.videoUrl && exercise.gifUrl && (
                                <div className="sm:w-32 shrink-0">
                                  <img
                                    src={exercise.gifUrl}
                                    alt={exercise.name}
                                    className="w-full h-auto rounded-md bg-muted"
                                  />
                                </div>
                              )}
                              <div className="flex-1 space-y-2">
                                <div className="flex items-start justify-between gap-2">
                                  <h4 className="font-semibold flex-1">
                                    {exIndex + 1}. {exercise.name}
                                  </h4>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    className="shrink-0"
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleReplacePlanExercise(dayIndex, exIndex);
                                    }}
                                    title="Substituir exercício"
                                  >
                                    <RefreshCw className="h-4 w-4" />
                                  </Button>
                                </div>
                                <div className="flex flex-wrap gap-2 text-xs">
                                  <span className="px-2 py-1 bg-secondary rounded-full text-xs">
                                    {exercise.sets} × {exercise.reps}
                                  </span>
                                  <span className="px-2 py-1 bg-secondary rounded-full text-xs">
                                    {exercise.rest} descanso
                                  </span>
                                  <span className="px-2 py-1 bg-secondary rounded-full text-xs">
                                    {exercise.equipment}
                                  </span>
                                </div>
                                {(exercise as any).progressionNotes && (
                                  <div className="mt-2 p-2 bg-primary/5 rounded-md border border-primary/10">
                                    <p className="text-xs font-medium text-primary mb-1">
                                      📈 Progressão Semanal:
                                    </p>
                                    <p className="text-xs text-muted-foreground">
                                      {(exercise as any).progressionNotes}
                                    </p>
                                  </div>
                                )}
                              </div>
                            </div>
                          </CardContent>
                        </Card>
                      ))}
                    </CardContent>
                  </Card>
                ))}

                <div className="mt-6 flex flex-col sm:flex-row gap-3">
                  <Button variant="outline" className="flex-1" onClick={handleSavePlan}>
                    Salvar Plano
                  </Button>
                  <Button className="flex-1" onClick={handleStartPlan}>
                    Começar Este Plano
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>

      {/* Exercise Detail Modal */}
      <Dialog open={isExerciseModalOpen} onOpenChange={setIsExerciseModalOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          {selectedExercise && (
            <>
              <DialogHeader>
                <DialogTitle className="text-2xl">{selectedExercise.name}</DialogTitle>
                <DialogDescription>
                  Detalhes do exercício
                </DialogDescription>
                <div className="flex flex-wrap gap-2 mt-2">
                  <Badge variant="secondary">{selectedExercise.bodyPart}</Badge>
                  <Badge variant="secondary">{selectedExercise.equipment}</Badge>
                </div>
              </DialogHeader>

              <div className="space-y-6 mt-4">
                {/* Exercise Video/GIF */}
                {selectedExercise.videoUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    <iframe
                      width="100%"
                      height="400"
                      src={selectedExercise.videoUrl.replace('watch?v=', 'embed/')}
                      title={selectedExercise.name}
                      frameBorder="0"
                      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                      allowFullScreen
                      className="rounded-md"
                    ></iframe>
                  </div>
                )}
                {!selectedExercise.videoUrl && selectedExercise.gifUrl && (
                  <div className="flex justify-center bg-muted rounded-lg p-4">
                    <img
                      src={selectedExercise.gifUrl}
                      alt={selectedExercise.name}
                      className="max-w-full h-auto rounded-md"
                      style={{ maxHeight: '400px' }}
                    />
                  </div>
                )}

                {/* Exercise Parameters */}
                <div className="grid grid-cols-3 gap-4">
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.sets}</p>
                      <p className="text-sm text-muted-foreground mt-1">Séries</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.reps}</p>
                      <p className="text-sm text-muted-foreground mt-1">Repetições</p>
                    </CardContent>
                  </Card>
                  <Card>
                    <CardContent className="pt-6 text-center">
                      <p className="text-3xl font-bold text-primary">{selectedExercise.rest}</p>
                      <p className="text-sm text-muted-foreground mt-1">Descanso</p>
                    </CardContent>
                  </Card>
                </div>

                {/* Exercise Instructions */}
                <Card>
                  <CardHeader>
                    <CardTitle className="text-lg">Como Executar</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <ol className="space-y-3">
                      {selectedExercise.instructions.map((instruction, index) => (
                        <li key={index} className="flex gap-3">
                          <span className="flex-shrink-0 flex items-center justify-center w-6 h-6 rounded-full bg-primary text-primary-foreground text-sm font-semibold">
                            {index + 1}
                          </span>
                          <span className="flex-1 pt-0.5">{instruction}</span>
                        </li>
                      ))}
                    </ol>
                  </CardContent>
                </Card>

                {/* Progression Notes */}
                {(selectedExercise as any).progressionNotes && (
                  <Card className="bg-gradient-to-br from-primary/10 to-primary/5 border-primary/20">
                    <CardHeader>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <svg className="h-5 w-5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
                        </svg>
                        Progressão Semanal
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      <p className="text-sm whitespace-pre-line">
                        {(selectedExercise as any).progressionNotes}
                      </p>
                      <p className="text-xs text-muted-foreground mt-3 italic">
                        💡 Siga esta progressão ao longo das 4 semanas para obter os melhores resultados
                      </p>
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
            </>
          )}
        </DialogContent>
      </Dialog>

      {/* Share Workout Dialog */}
      <Dialog open={isShareDialogOpen} onOpenChange={setIsShareDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Share2 className="h-5 w-5 text-primary" />
              Compartilhar {workoutToShare === 'single' ? 'Treino' : 'Plano'}
            </DialogTitle>
            <DialogDescription>
              {friends.length > 0
                ? 'Selecione os amigos com quem deseja compartilhar este treino'
                : 'Você ainda não tem amigos para compartilhar. Adicione amigos primeiro!'}
            </DialogDescription>
          </DialogHeader>

          {friends.length > 0 ? (
            <div className="space-y-4 mt-4">
              <Card>
                <CardContent className="pt-6">
                  <div className="space-y-3 max-h-64 overflow-y-auto">
                    {friends.map((friend) => (
                      <div key={friend.friendId} className="flex items-center space-x-2">
                        <Checkbox
                          id={`share-${friend.friendId}`}
                          checked={selectedFriends.includes(friend.friendId)}
                          onCheckedChange={() => toggleFriendSelection(friend.friendId)}
                        />
                        <label
                          htmlFor={`share-${friend.friendId}`}
                          className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer flex items-center gap-2"
                        >
                          <div className="h-6 w-6 rounded-full bg-primary/10 flex items-center justify-center">
                            <span className="text-xs font-semibold text-primary">
                              {friend.friendName.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          {friend.friendName}
                        </label>
                      </div>
                    ))}
                  </div>
                  {selectedFriends.length > 0 && (
                    <p className="text-sm text-muted-foreground mt-3">
                      {selectedFriends.length} {selectedFriends.length === 1 ? 'amigo selecionado' : 'amigos selecionados'}
                    </p>
                  )}
                </CardContent>
              </Card>

              <div className="flex gap-2">
                <Button
                  variant="outline"
                  className="flex-1"
                  onClick={() => {
                    setIsShareDialogOpen(false);
                    setSelectedFriends([]);
                  }}
                >
                  Cancelar
                </Button>
                <Button
                  className="flex-1"
                  onClick={async () => {
                    if (selectedFriends.length === 0) {
                      toast({
                        variant: 'destructive',
                        title: 'Selecione pelo menos um amigo',
                        description: 'Escolha com quem deseja compartilhar',
                      });
                      return;
                    }

                    if (!savedPlanId) {
                      toast({
                        variant: 'destructive',
                        title: 'Erro ao compartilhar',
                        description: 'Por favor, salve o treino primeiro usando o botão "Iniciar Este Plano"',
                      });
                      return;
                    }

                    try {
                      await apiClient.post(`/workout-plans/${savedPlanId}/share`, {
                        friendIds: selectedFriends,
                      });

                      toast({
                        title: 'Treino compartilhado!',
                        description: `Seu treino foi compartilhado com ${selectedFriends.length} ${selectedFriends.length === 1 ? 'amigo' : 'amigos'}.`,
                      });

                      setIsShareDialogOpen(false);
                      setSelectedFriends([]);
                    } catch (error) {
                      toast({
                        variant: 'destructive',
                        title: 'Erro ao compartilhar',
                        description: error instanceof Error ? error.message : 'Não foi possível compartilhar o treino',
                      });
                    }
                  }}
                  disabled={selectedFriends.length === 0}
                >
                  <Share2 className="mr-2 h-4 w-4" />
                  Compartilhar
                </Button>
              </div>
            </div>
          ) : (
            <div className="text-center py-8">
              <p className="text-sm text-muted-foreground mb-4">
                Adicione amigos primeiro na página de Amigos para compartilhar seus treinos!
              </p>
              <Button
                onClick={() => {
                  setIsShareDialogOpen(false);
                  window.location.href = '/friends';
                }}
              >
                Ir para Amigos
              </Button>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}