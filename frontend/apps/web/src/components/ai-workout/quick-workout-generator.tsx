'use client';

import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import {
  Loader2,
  Sparkles,
  ChevronDown,
  ChevronUp,
  Dumbbell,
  Flame,
  Zap,
  Heart,
  Check,
  Play,
  RotateCcw,
  ArrowLeft,
  Clock,
  Video
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { apiClient } from '@/lib/api';

// Types
type Goal = 'Hipertrofia' | 'Emagrecimento' | 'Forca' | 'Condicionamento';
type Level = 'Iniciante' | 'Intermediario' | 'Avancado';
type Location = 'Academia' | 'Casa';
type MuscleGroup = 'Peito' | 'Costas' | 'Ombros' | 'Biceps' | 'Triceps' | 'Pernas' | 'Gluteos' | 'Abdomen' | 'Corpo Todo';

interface QuickWorkoutForm {
  goal: Goal | null;
  level: Level | null;
  location: Location | null;
  muscleGroups: MuscleGroup[];
  duration?: number;
  injuries?: string;
  notes?: string;
}

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

interface QuickWorkoutGeneratorProps {
  onWorkoutGenerated?: (workout: QuickWorkoutResponse) => void;
}

const GOALS: { value: Goal; label: string; description: string; icon: any; color: string }[] = [
  { value: 'Hipertrofia', label: 'Ganhar Musculo', description: 'Aumento de massa muscular', icon: Dumbbell, color: 'text-blue-500' },
  { value: 'Emagrecimento', label: 'Perder Gordura', description: 'Queima de gordura', icon: Flame, color: 'text-orange-500' },
  { value: 'Forca', label: 'Ficar Mais Forte', description: 'Aumento de forca maxima', icon: Zap, color: 'text-yellow-500' },
  { value: 'Condicionamento', label: 'Condicionamento', description: 'Resistencia e saude', icon: Heart, color: 'text-red-500' },
];

const LEVELS: { value: Level; label: string; desc: string }[] = [
  { value: 'Iniciante', label: 'Iniciante', desc: '< 6 meses' },
  { value: 'Intermediario', label: 'Intermediario', desc: '6m - 2 anos' },
  { value: 'Avancado', label: 'Avancado', desc: '2+ anos' },
];

const LOCATIONS: { value: Location; label: string; emoji: string }[] = [
  { value: 'Academia', label: 'Academia', emoji: '🏋️' },
  { value: 'Casa', label: 'Casa', emoji: '🏠' },
];

const MUSCLES: { value: MuscleGroup; label: string }[] = [
  { value: 'Peito', label: 'Peito' },
  { value: 'Costas', label: 'Costas' },
  { value: 'Ombros', label: 'Ombros' },
  { value: 'Biceps', label: 'Biceps' },
  { value: 'Triceps', label: 'Triceps' },
  { value: 'Pernas', label: 'Pernas' },
  { value: 'Gluteos', label: 'Gluteos' },
  { value: 'Abdomen', label: 'Abdomen' },
  { value: 'Corpo Todo', label: 'Corpo Todo' },
];

const DURATIONS = [30, 45, 60, 90];

export function QuickWorkoutGenerator({ onWorkoutGenerated }: QuickWorkoutGeneratorProps) {
  const [form, setForm] = useState<QuickWorkoutForm>({
    goal: null,
    level: null,
    location: null,
    muscleGroups: [],
  });
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [generatedWorkout, setGeneratedWorkout] = useState<QuickWorkoutResponse | null>(null);

  const generateMutation = useMutation({
    mutationFn: async (data: QuickWorkoutForm) => {
      const response = await apiClient.post<QuickWorkoutResponse>('/ai/v2/quick-workout', {
        goal: data.goal,
        level: data.level,
        location: data.location,
        muscleGroups: data.muscleGroups,
        duration: data.duration,
        injuries: data.injuries,
        notes: data.notes,
      });
      return response;
    },
    onSuccess: (data) => {
      setGeneratedWorkout(data);
      onWorkoutGenerated?.(data);
    },
  });

  const toggleMuscle = (muscle: MuscleGroup) => {
    if (muscle === 'Corpo Todo') {
      setForm({ ...form, muscleGroups: ['Corpo Todo'] });
    } else {
      const filtered = form.muscleGroups.filter(m => m !== 'Corpo Todo');
      if (filtered.includes(muscle)) {
        setForm({ ...form, muscleGroups: filtered.filter(m => m !== muscle) });
      } else {
        setForm({ ...form, muscleGroups: [...filtered, muscle] });
      }
    }
  };

  const completedSteps = [
    form.goal !== null,
    form.level !== null,
    form.location !== null,
    form.muscleGroups.length > 0,
  ].filter(Boolean).length;

  const canGenerate = completedSteps === 4;

  const handleGenerate = () => {
    if (!canGenerate) return;
    generateMutation.mutate(form);
  };

  // If we have a generated workout, show it
  if (generatedWorkout) {
    return (
      <WorkoutDisplay
        workout={generatedWorkout}
        onBack={() => setGeneratedWorkout(null)}
        onRegenerate={handleGenerate}
        isLoading={generateMutation.isPending}
      />
    );
  }

  return (
    <Card className="max-w-2xl mx-auto border-0 shadow-lg">
      <CardHeader className="text-center pb-2">
        <div className="mx-auto w-12 h-12 bg-gradient-to-br from-primary to-primary/60 rounded-xl flex items-center justify-center mb-3">
          <Sparkles className="h-6 w-6 text-primary-foreground" />
        </div>
        <CardTitle className="text-2xl">Gerar Treino com IA</CardTitle>
        <CardDescription className="text-base">
          Responda 4 perguntas e tenha seu treino personalizado
        </CardDescription>

        {/* Progress indicator */}
        <div className="flex items-center justify-center gap-2 mt-4">
          {[1, 2, 3, 4].map((step) => (
            <div
              key={step}
              className={cn(
                'w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-all',
                completedSteps >= step
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-muted text-muted-foreground'
              )}
            >
              {completedSteps >= step ? <Check className="h-4 w-4" /> : step}
            </div>
          ))}
        </div>
      </CardHeader>

      <CardContent className="space-y-8 pt-6">
        {/* 1. Objetivo */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className={cn(
              'w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold',
              form.goal ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground'
            )}>
              1
            </span>
            <label className="font-semibold">Qual seu objetivo?</label>
          </div>
          <div className="grid grid-cols-2 gap-3">
            {GOALS.map(({ value, label, description, icon: Icon, color }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, goal: value })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-left group hover:shadow-md',
                  form.goal === value
                    ? 'border-primary bg-primary/5 shadow-md'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <div className="flex items-start gap-3">
                  <div className={cn(
                    'w-10 h-10 rounded-lg flex items-center justify-center transition-colors',
                    form.goal === value ? 'bg-primary/10' : 'bg-muted group-hover:bg-primary/5'
                  )}>
                    <Icon className={cn('h-5 w-5', color)} />
                  </div>
                  <div>
                    <span className="block font-medium">{label}</span>
                    <span className="text-xs text-muted-foreground">{description}</span>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* 2. Nivel */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className={cn(
              'w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold',
              form.level ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground'
            )}>
              2
            </span>
            <label className="font-semibold">Qual seu nivel de experiencia?</label>
          </div>
          <div className="grid grid-cols-3 gap-3">
            {LEVELS.map(({ value, label, desc }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, level: value })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-center hover:shadow-md',
                  form.level === value
                    ? 'border-primary bg-primary/5 shadow-md'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <span className="block font-medium">{label}</span>
                <span className="text-xs text-muted-foreground">{desc}</span>
              </button>
            ))}
          </div>
        </div>

        {/* 3. Local */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className={cn(
              'w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold',
              form.location ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground'
            )}>
              3
            </span>
            <label className="font-semibold">Onde vai treinar?</label>
          </div>
          <div className="grid grid-cols-2 gap-3">
            {LOCATIONS.map(({ value, label, emoji }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, location: value })}
                className={cn(
                  'p-5 rounded-xl border-2 transition-all text-center hover:shadow-md',
                  form.location === value
                    ? 'border-primary bg-primary/5 shadow-md'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <span className="text-3xl block mb-1">{emoji}</span>
                <span className="font-medium">{label}</span>
              </button>
            ))}
          </div>
        </div>

        {/* 4. Musculos */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <span className={cn(
              'w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold',
              form.muscleGroups.length > 0 ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground'
            )}>
              4
            </span>
            <label className="font-semibold">Que musculos quer treinar?</label>
            {form.muscleGroups.length > 0 && (
              <Badge variant="secondary" className="ml-auto">
                {form.muscleGroups.length} selecionado{form.muscleGroups.length > 1 ? 's' : ''}
              </Badge>
            )}
          </div>
          <div className="flex flex-wrap gap-2">
            {MUSCLES.map(({ value, label }) => (
              <button
                key={value}
                onClick={() => toggleMuscle(value)}
                className={cn(
                  'px-4 py-2.5 rounded-full border-2 transition-all font-medium',
                  form.muscleGroups.includes(value)
                    ? 'border-primary bg-primary text-primary-foreground shadow-md'
                    : 'border-border hover:border-primary/50 hover:bg-muted/50'
                )}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* Opcoes Avancadas */}
        <div className="pt-2">
          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="text-primary text-sm flex items-center gap-1 hover:underline font-medium"
          >
            {showAdvanced ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            {showAdvanced ? 'Ocultar opcoes' : 'Personalizar mais'}
          </button>

          {showAdvanced && (
            <div className="mt-4 space-y-4 p-4 bg-muted/30 rounded-xl border">
              <div>
                <label className="block text-sm font-medium mb-2">Tempo disponivel</label>
                <div className="flex gap-2">
                  {DURATIONS.map(min => (
                    <button
                      key={min}
                      onClick={() => setForm({ ...form, duration: form.duration === min ? undefined : min })}
                      className={cn(
                        'px-4 py-2 rounded-lg border-2 font-medium transition-all',
                        form.duration === min
                          ? 'border-primary bg-primary/10 text-primary'
                          : 'border-border hover:border-primary/50'
                      )}
                    >
                      {min}min
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Alguma lesao ou restricao?</label>
                <input
                  type="text"
                  value={form.injuries || ''}
                  onChange={e => setForm({ ...form, injuries: e.target.value })}
                  placeholder="Ex: Dor no ombro direito, evitar agachamento..."
                  className="w-full p-3 border-2 rounded-lg bg-background focus:border-primary focus:outline-none transition-colors"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Observacoes</label>
                <textarea
                  value={form.notes || ''}
                  onChange={e => setForm({ ...form, notes: e.target.value })}
                  placeholder="Algo mais que a IA deva saber?"
                  className="w-full p-3 border-2 rounded-lg bg-background h-20 resize-none focus:border-primary focus:outline-none transition-colors"
                />
              </div>
            </div>
          )}
        </div>

        {/* Error message */}
        {generateMutation.isError && (
          <div className="p-4 bg-destructive/10 text-destructive rounded-xl text-sm border border-destructive/20">
            {(generateMutation.error as any)?.message || 'Erro ao gerar treino. Tente novamente.'}
          </div>
        )}

        {/* Botao Gerar */}
        <Button
          onClick={handleGenerate}
          disabled={!canGenerate || generateMutation.isPending}
          className={cn(
            'w-full py-7 text-lg font-bold rounded-xl transition-all',
            canGenerate && !generateMutation.isPending
              ? 'bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 shadow-lg hover:shadow-xl'
              : ''
          )}
          size="lg"
        >
          {generateMutation.isPending ? (
            <span className="flex items-center gap-2">
              <Loader2 className="h-5 w-5 animate-spin" />
              Gerando seu treino personalizado...
            </span>
          ) : (
            <span className="flex items-center gap-2">
              <Sparkles className="h-5 w-5" />
              Gerar Treino com IA
            </span>
          )}
        </Button>

        {!canGenerate && (
          <p className="text-center text-sm text-muted-foreground">
            Complete todas as 4 etapas para gerar seu treino
          </p>
        )}
      </CardContent>
    </Card>
  );
}

// Workout Display Component
function WorkoutDisplay({
  workout,
  onBack,
  onRegenerate,
  isLoading,
}: {
  workout: QuickWorkoutResponse;
  onBack: () => void;
  onRegenerate: () => void;
  isLoading: boolean;
}) {
  const totalExercises = workout.warmup.length + workout.main.length + workout.cooldown.length;

  return (
    <Card className="max-w-2xl mx-auto border-0 shadow-lg">
      <CardHeader className="pb-4">
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-xl">{workout.name}</CardTitle>
            <CardDescription className="mt-1">
              {workout.description}
            </CardDescription>
          </div>
          <Badge variant="secondary" className="flex items-center gap-1">
            <Clock className="h-3 w-3" />
            {workout.estimatedDuration} min
          </Badge>
        </div>

        {/* Stats */}
        <div className="flex gap-4 mt-4 pt-4 border-t">
          <div className="text-center">
            <div className="text-2xl font-bold text-primary">{totalExercises}</div>
            <div className="text-xs text-muted-foreground">Exercicios</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-primary">{workout.main.length}</div>
            <div className="text-xs text-muted-foreground">Principais</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-primary">{workout.goal}</div>
            <div className="text-xs text-muted-foreground">Objetivo</div>
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-6">
        {/* Warmup */}
        {workout.warmup.length > 0 && (
          <ExerciseSection
            title="Aquecimento"
            exercises={workout.warmup}
            color="text-orange-500"
            bgColor="bg-orange-500/10"
          />
        )}

        {/* Main */}
        <ExerciseSection
          title="Exercicios Principais"
          exercises={workout.main}
          color="text-primary"
          bgColor="bg-primary/10"
        />

        {/* Cooldown */}
        {workout.cooldown.length > 0 && (
          <ExerciseSection
            title="Alongamento"
            exercises={workout.cooldown}
            color="text-green-500"
            bgColor="bg-green-500/10"
          />
        )}

        {/* Actions */}
        <div className="flex gap-3 pt-4 border-t">
          <Button variant="outline" onClick={onBack} className="flex-1">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Voltar
          </Button>
          <Button variant="outline" onClick={onRegenerate} disabled={isLoading} className="flex-1">
            {isLoading ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <RotateCcw className="h-4 w-4 mr-2" />
                Regenerar
              </>
            )}
          </Button>
          <Button className="flex-1 bg-gradient-to-r from-primary to-primary/80">
            <Play className="h-4 w-4 mr-2" />
            Iniciar
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// Exercise Section Component
function ExerciseSection({
  title,
  exercises,
  color,
  bgColor
}: {
  title: string;
  exercises: GeneratedExercise[];
  color: string;
  bgColor: string;
}) {
  return (
    <div>
      <div className="flex items-center gap-2 mb-3">
        <div className={cn('w-1 h-5 rounded-full', bgColor.replace('/10', ''))} />
        <h3 className={cn('font-semibold text-sm uppercase tracking-wide', color)}>
          {title}
        </h3>
        <Badge variant="outline" className="ml-auto text-xs">
          {exercises.length}
        </Badge>
      </div>
      <div className="space-y-2">
        {exercises.map((ex, idx) => (
          <ExerciseCard key={idx} exercise={ex} index={idx + 1} />
        ))}
      </div>
    </div>
  );
}

// Exercise Card Component
function ExerciseCard({ exercise, index }: { exercise: GeneratedExercise; index: number }) {
  const [showVideo, setShowVideo] = useState(false);

  return (
    <div className="p-3 border rounded-xl bg-card hover:shadow-sm transition-shadow">
      <div className="flex items-center gap-3">
        <div className="w-8 h-8 rounded-lg bg-muted flex items-center justify-center text-sm font-bold text-muted-foreground">
          {index}
        </div>
        <div className="flex-1 min-w-0">
          <h4 className="font-medium truncate">{exercise.exerciseName}</h4>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <span>{exercise.sets} x {exercise.reps}</span>
            <span>•</span>
            <span>{exercise.restSeconds}s descanso</span>
          </div>
        </div>
        {exercise.videoUrl && (
          <Button
            variant={showVideo ? "default" : "outline"}
            size="sm"
            onClick={() => setShowVideo(!showVideo)}
            className="shrink-0"
          >
            <Video className="h-4 w-4" />
          </Button>
        )}
      </div>

      {showVideo && exercise.videoUrl && (
        <div className="mt-3 pt-3 border-t">
          <iframe
            src={exercise.videoUrl.replace('watch?v=', 'embed/')}
            className="w-full aspect-video rounded-lg"
            allowFullScreen
          />
        </div>
      )}
    </div>
  );
}
