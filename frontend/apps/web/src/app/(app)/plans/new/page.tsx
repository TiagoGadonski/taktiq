'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Search, Trash2, ChevronDown, ChevronUp, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

const planSchema = z.object({
  name: z.string().min(3, 'Nome deve ter no mínimo 3 caracteres'),
  description: z.string().optional(),
  duration: z.string().optional(),
  goal: z.string().optional(),
});

type PlanFormData = z.infer<typeof planSchema>;

interface Exercise {
  id: string;
  name: string;
  equipment: string;
  primaryMuscles: string[];
  secondaryMuscles?: string[];
  images: string[];
  instructions?: string[];
  category?: string;
  level?: string;
  gifUrl?: string;
  namePt?: string;
  equipmentPt?: string;
  primaryMusclesPt?: string[];
}

interface SelectedExercise extends Exercise {
  sets: number;
  reps: number;
  rest: number;
  notes: string;
}

interface WorkoutDay {
  id: string;
  name: string;
  exercises: SelectedExercise[];
  isExpanded: boolean;
}

export default function NewPlanPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<Exercise[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [workoutDays, setWorkoutDays] = useState<WorkoutDay[]>([
    { id: '1', name: 'Dia 1', exercises: [], isExpanded: true }
  ]);
  const [selectedDayId, setSelectedDayId] = useState('1');
  const [muscleFilter, setMuscleFilter] = useState<string>('all');
  const [difficultyFilter, setDifficultyFilter] = useState<string>('all');

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PlanFormData>({
    resolver: zodResolver(planSchema),
  });

  const searchExercises = async () => {
    if (!searchQuery.trim() && muscleFilter === 'all' && difficultyFilter === 'all') {
      toast({
        variant: 'destructive',
        title: 'Digite algo para buscar',
        description: 'Digite um nome de exercício ou selecione um filtro',
      });
      return;
    }

    setIsSearching(true);
    try {
      const params = new URLSearchParams();
      if (searchQuery.trim()) params.append('query', searchQuery);
      if (muscleFilter !== 'all') params.append('muscle', muscleFilter);
      if (difficultyFilter !== 'all') params.append('level', difficultyFilter);

      const response = await apiClient.get(`/ai/search-exercises?${params.toString()}`);
      const data = response.data || response;
      setSearchResults(data);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao buscar exercícios',
        description: error.response?.data?.message || 'Não foi possível carregar os exercícios. Tente novamente.',
      });
    } finally {
      setIsSearching(false);
    }
  };

  const addWorkoutDay = () => {
    const newId = (workoutDays.length + 1).toString();
    setWorkoutDays([
      ...workoutDays,
      { id: newId, name: `Dia ${workoutDays.length + 1}`, exercises: [], isExpanded: true }
    ]);
  };

  const removeWorkoutDay = (dayId: string) => {
    if (workoutDays.length <= 1) {
      toast({
        variant: 'destructive',
        title: 'Não é possível remover',
        description: 'O plano deve ter pelo menos um dia de treino',
      });
      return;
    }
    setWorkoutDays(workoutDays.filter(day => day.id !== dayId));
  };

  const toggleDayExpanded = (dayId: string) => {
    setWorkoutDays(workoutDays.map(day =>
      day.id === dayId ? { ...day, isExpanded: !day.isExpanded } : day
    ));
  };

  const addExerciseToDay = (exercise: Exercise) => {
    const selectedDay = workoutDays.find(day => day.id === selectedDayId);
    if (!selectedDay) return;

    const newExercise: SelectedExercise = {
      ...exercise,
      sets: 3,
      reps: 12,
      rest: 60,
      notes: ''
    };

    setWorkoutDays(workoutDays.map(day =>
      day.id === selectedDayId
        ? { ...day, exercises: [...day.exercises, newExercise] }
        : day
    ));

    toast({
      title: 'Exercício adicionado!',
      description: `${exercise.namePt || exercise.name} foi adicionado ao ${selectedDay.name}`,
    });
  };

  const removeExercise = (dayId: string, exerciseIndex: number) => {
    setWorkoutDays(workoutDays.map(day =>
      day.id === dayId
        ? { ...day, exercises: day.exercises.filter((_, i) => i !== exerciseIndex) }
        : day
    ));
  };

  const updateExercise = (dayId: string, exerciseIndex: number, field: keyof SelectedExercise, value: any) => {
    setWorkoutDays(workoutDays.map(day =>
      day.id === dayId
        ? {
            ...day,
            exercises: day.exercises.map((ex, i) =>
              i === exerciseIndex ? { ...ex, [field]: value } : ex
            )
          }
        : day
    ));
  };

  const onSubmit = async (data: PlanFormData) => {
    // Validate that we have at least one exercise
    const hasExercises = workoutDays.some(day => day.exercises.length > 0);
    if (!hasExercises) {
      toast({
        variant: 'destructive',
        title: 'Adicione exercícios',
        description: 'O plano deve ter pelo menos um exercício',
      });
      return;
    }

    try {
      // Step 1: Create the workout plan (name + goal only)
      const planData = {
        name: data.name,
        goal: data.goal || null,
      };

      console.log('Creating plan with data:', planData);
      const createdPlan = await apiClient.post('/workout-plans', planData);
      console.log('Plan created:', createdPlan);
      const planId = createdPlan.id;

      if (!planId) {
        throw new Error('Plan ID not returned from server');
      }

      // Step 2: Get all exercises from the database to check which ones already exist
      const existingExercises = await apiClient.get('/exercises');
      const exerciseMap = new Map(existingExercises.map((e: any) => [e.name.toLowerCase(), e.id]));

      // Step 3: Create workouts (days) and add exercises to them
      console.log(`Creating ${workoutDays.length} workout days`);

      for (let dayIndex = 0; dayIndex < workoutDays.length; dayIndex++) {
        const day = workoutDays[dayIndex];

        // Create the workout (day)
        const workoutData = {
          name: day.name,
          dayOfWeek: null,
          order: dayIndex + 1,
        };

        console.log(`Creating workout day "${day.name}":`, workoutData);
        const workoutResponse = await apiClient.post(`/workout-plans/${planId}/workouts`, workoutData);
        const workoutId = workoutResponse.id;

        // Add exercises to this workout
        for (let exIndex = 0; exIndex < day.exercises.length; exIndex++) {
          const ex = day.exercises[exIndex];
          let exerciseId: string;

          // Check if exercise already exists in database
          const existingId = exerciseMap.get(ex.name.toLowerCase());

          if (existingId) {
            // Exercise already exists, use its ID
            exerciseId = existingId;
            console.log(`Exercise "${ex.name}" already exists with ID: ${exerciseId}`);
          } else {
            // Exercise doesn't exist, create it first
            console.log(`Creating new exercise: ${ex.name}`);
            const newExercise = await apiClient.post('/exercises', {
              name: ex.name,
              muscleGroup: (ex.primaryMuscles && ex.primaryMuscles.length > 0) ? ex.primaryMuscles[0] : 'Other',
              category: ex.category || 'strength',
              equipment: ex.equipment || 'bodyweight',
              notes: ex.instructions ? ex.instructions.join('. ') : null,
            });
            exerciseId = newExercise.id;
            exerciseMap.set(ex.name.toLowerCase(), exerciseId);
          }

          // Now add the exercise to the workout day
          const exerciseData = {
            exerciseId: exerciseId,
            order: exIndex + 1,
            targetSets: ex.sets,
            targetReps: ex.reps,
            targetLoad: 0, // Default load
          };

          console.log(`Adding exercise to workout ${workoutId}:`, exerciseData);
          await apiClient.post(`/workout-plans/${planId}/workouts/${workoutId}/exercises`, exerciseData);
        }
      }

      console.log('All workout days and exercises created successfully');

      toast({
        title: 'Plano criado!',
        description: 'Seu plano de treino foi criado com sucesso.',
      });

      router.push('/plans');
    } catch (error: any) {
      console.error('Error creating plan:', error);
      toast({
        variant: 'destructive',
        title: 'Erro ao criar plano',
        description: error.response?.data?.message || error.message || 'Não foi possível criar o plano',
      });
    }
  };

  const muscles = ['all', 'abdominals', 'abductors', 'adductors', 'biceps', 'calves', 'chest', 'forearms', 'glutes', 'hamstrings', 'lats', 'lower back', 'middle back', 'neck', 'quadriceps', 'traps', 'triceps', 'shoulders'];
  const difficulties = ['all', 'beginner', 'intermediate', 'expert'];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Criar Plano de Treino</h1>
          <p className="text-muted-foreground">Monte seu plano personalizado selecionando exercícios</p>
        </div>
        <Button variant="outline" onClick={() => router.push('/plans')}>
          Cancelar
        </Button>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Plan Details */}
        <Card>
          <CardHeader>
            <CardTitle>Detalhes do Plano</CardTitle>
            <CardDescription>Informações básicas sobre o plano de treino</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="name">Nome do Plano *</Label>
                <Input
                  id="name"
                  placeholder="Ex: Treino de Hipertrofia"
                  {...register('name')}
                />
                {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
              </div>

              <div className="space-y-2">
                <Label htmlFor="duration">Duração (semanas)</Label>
                <Input
                  id="duration"
                  type="number"
                  placeholder="Ex: 12"
                  {...register('duration')}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="goal">Objetivo</Label>
              <Input
                id="goal"
                placeholder="Ex: Ganho de massa muscular"
                {...register('goal')}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Descrição</Label>
              <Textarea
                id="description"
                placeholder="Descreva o plano de treino..."
                rows={3}
                {...register('description')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Exercise Search */}
        <Card>
          <CardHeader>
            <CardTitle>Buscar Exercícios</CardTitle>
            <CardDescription>Pesquise exercícios para adicionar ao seu plano</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex gap-2">
              <Input
                placeholder="Buscar exercício..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), searchExercises())}
              />
              <Select value={muscleFilter} onValueChange={setMuscleFilter}>
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="Músculo" />
                </SelectTrigger>
                <SelectContent>
                  {muscles.map(muscle => (
                    <SelectItem key={muscle} value={muscle}>
                      {muscle === 'all' ? 'Todos' : muscle}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Select value={difficultyFilter} onValueChange={setDifficultyFilter}>
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="Nível" />
                </SelectTrigger>
                <SelectContent>
                  {difficulties.map(diff => (
                    <SelectItem key={diff} value={diff}>
                      {diff === 'all' ? 'Todos' : diff}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button type="button" onClick={searchExercises} disabled={isSearching}>
                <Search className="h-4 w-4 mr-2" />
                {isSearching ? 'Buscando...' : 'Buscar'}
              </Button>
            </div>

            <div className="space-y-2">
              <Label>Adicionar ao:</Label>
              <Select value={selectedDayId} onValueChange={setSelectedDayId}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {workoutDays.map(day => (
                    <SelectItem key={day.id} value={day.id}>
                      {day.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {searchResults.length > 0 && (
              <div className="space-y-2 max-h-96 overflow-y-auto">
                <Label>Resultados ({searchResults.length})</Label>
                <div className="grid gap-2">
                  {searchResults.map((exercise) => (
                    <Card key={exercise.id} className="cursor-pointer hover:bg-accent/50 transition-colors">
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between gap-4">
                          {exercise.gifUrl && (
                            <img
                              src={exercise.gifUrl}
                              alt={exercise.name}
                              className="w-20 h-20 object-cover rounded"
                            />
                          )}
                          <div className="flex-1">
                            <h4 className="font-medium">{exercise.namePt || exercise.name}</h4>
                            <p className="text-sm text-muted-foreground">
                              {(exercise.primaryMusclesPt || exercise.primaryMuscles).join(', ')} • {exercise.equipmentPt || exercise.equipment}
                              {exercise.level && ` • ${exercise.level}`}
                            </p>
                          </div>
                          <Button
                            type="button"
                            size="sm"
                            onClick={() => addExerciseToDay(exercise)}
                          >
                            <Plus className="h-4 w-4" />
                          </Button>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Workout Days */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-2xl font-bold">Dias de Treino</h2>
            <Button type="button" variant="outline" onClick={addWorkoutDay}>
              <Plus className="h-4 w-4 mr-2" />
              Adicionar Dia
            </Button>
          </div>

          {workoutDays.map((day) => (
            <Card key={day.id}>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="flex items-center gap-2">
                    <Input
                      value={day.name}
                      onChange={(e) => setWorkoutDays(workoutDays.map(d =>
                        d.id === day.id ? { ...d, name: e.target.value } : d
                      ))}
                      className="w-48"
                    />
                    <span className="text-sm text-muted-foreground">
                      ({day.exercises.length} exercícios)
                    </span>
                  </CardTitle>
                  <div className="flex gap-2">
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => toggleDayExpanded(day.id)}
                    >
                      {day.isExpanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => removeWorkoutDay(day.id)}
                      disabled={workoutDays.length <= 1}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardHeader>
              {day.isExpanded && (
                <CardContent className="space-y-3">
                  {day.exercises.length === 0 ? (
                    <p className="text-sm text-muted-foreground text-center py-4">
                      Nenhum exercício adicionado. Use a busca acima para adicionar exercícios.
                    </p>
                  ) : (
                    day.exercises.map((exercise, index) => (
                      <Card key={index} className="border-2">
                        <CardContent className="p-4 space-y-3">
                          <div className="flex items-start justify-between gap-3">
                            {exercise.gifUrl && (
                              <img
                                src={exercise.gifUrl}
                                alt={exercise.name}
                                className="w-16 h-16 object-cover rounded"
                              />
                            )}
                            <div className="flex-1">
                              <h4 className="font-medium">{exercise.namePt || exercise.name}</h4>
                              <p className="text-sm text-muted-foreground">
                                {(exercise.primaryMusclesPt || exercise.primaryMuscles).join(', ')} • {exercise.equipmentPt || exercise.equipment}
                              </p>
                            </div>
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => removeExercise(day.id, index)}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                          <div className="grid grid-cols-3 gap-3">
                            <div>
                              <Label className="text-xs">Séries</Label>
                              <Input
                                type="number"
                                value={exercise.sets}
                                onChange={(e) => updateExercise(day.id, index, 'sets', parseInt(e.target.value))}
                                min="1"
                              />
                            </div>
                            <div>
                              <Label className="text-xs">Repetições</Label>
                              <Input
                                type="number"
                                value={exercise.reps}
                                onChange={(e) => updateExercise(day.id, index, 'reps', parseInt(e.target.value))}
                                min="1"
                              />
                            </div>
                            <div>
                              <Label className="text-xs">Descanso (s)</Label>
                              <Input
                                type="number"
                                value={exercise.rest}
                                onChange={(e) => updateExercise(day.id, index, 'rest', parseInt(e.target.value))}
                                min="0"
                              />
                            </div>
                          </div>
                          <div>
                            <Label className="text-xs">Observações</Label>
                            <Input
                              value={exercise.notes}
                              onChange={(e) => updateExercise(day.id, index, 'notes', e.target.value)}
                              placeholder="Ex: Executar lentamente..."
                            />
                          </div>
                        </CardContent>
                      </Card>
                    ))
                  )}
                </CardContent>
              )}
            </Card>
          ))}
        </div>

        {/* Submit Button */}
        <div className="flex gap-2">
          <Button type="submit" size="lg">
            <Save className="h-4 w-4 mr-2" />
            Criar Plano
          </Button>
          <Button type="button" variant="outline" size="lg" onClick={() => router.push('/plans')}>
            Cancelar
          </Button>
        </div>
      </form>
    </div>
  );
}
