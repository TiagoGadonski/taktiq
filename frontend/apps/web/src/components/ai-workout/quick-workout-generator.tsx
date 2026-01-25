'use client';

import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Loader2, Sparkles, ChevronDown, ChevronUp } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
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

const GOALS: { value: Goal; label: string; description: string }[] = [
  { value: 'Hipertrofia', label: 'Ganhar Musculo', description: 'Aumento de massa muscular' },
  { value: 'Emagrecimento', label: 'Perder Gordura', description: 'Queima de gordura' },
  { value: 'Forca', label: 'Ficar Mais Forte', description: 'Aumento de forca maxima' },
  { value: 'Condicionamento', label: 'Condicionamento', description: 'Resistencia e saude' },
];

const LEVELS: { value: Level; label: string; desc: string }[] = [
  { value: 'Iniciante', label: 'Iniciante', desc: '< 6 meses' },
  { value: 'Intermediario', label: 'Intermediario', desc: '6m - 2 anos' },
  { value: 'Avancado', label: 'Avancado', desc: '2+ anos' },
];

const LOCATIONS: { value: Location; label: string }[] = [
  { value: 'Academia', label: 'Academia' },
  { value: 'Casa', label: 'Casa' },
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

  const canGenerate = form.goal && form.level && form.location && form.muscleGroups.length > 0;

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
    <Card className="max-w-2xl mx-auto">
      <CardHeader className="text-center">
        <CardTitle className="flex items-center justify-center gap-2">
          <Sparkles className="h-5 w-5 text-primary" />
          Gerar Treino com IA
        </CardTitle>
        <CardDescription>Responda 4 perguntas e tenha seu treino em segundos</CardDescription>
      </CardHeader>
      <CardContent className="space-y-8">
        {/* 1. Objetivo */}
        <div className="space-y-3">
          <label className="block font-semibold">1. Qual seu objetivo?</label>
          <div className="grid grid-cols-2 gap-3">
            {GOALS.map(({ value, label, description }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, goal: value })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-left',
                  form.goal === value
                    ? 'border-primary bg-primary/10 scale-[1.02]'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <span className="block font-medium">{label}</span>
                <span className="text-xs text-muted-foreground">{description}</span>
              </button>
            ))}
          </div>
        </div>

        {/* 2. Nivel */}
        <div className="space-y-3">
          <label className="block font-semibold">2. Qual seu nivel?</label>
          <div className="grid grid-cols-3 gap-3">
            {LEVELS.map(({ value, label, desc }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, level: value })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-center',
                  form.level === value
                    ? 'border-primary bg-primary/10'
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
          <label className="block font-semibold">3. Onde vai treinar?</label>
          <div className="grid grid-cols-2 gap-3">
            {LOCATIONS.map(({ value, label }) => (
              <button
                key={value}
                onClick={() => setForm({ ...form, location: value })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-center',
                  form.location === value
                    ? 'border-primary bg-primary/10'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <span className="text-2xl">{value === 'Academia' ? '🏋️' : '🏠'}</span>
                <span className="block mt-1 font-medium">{label}</span>
              </button>
            ))}
          </div>
        </div>

        {/* 4. Musculos */}
        <div className="space-y-3">
          <label className="block font-semibold">4. Que musculos quer treinar?</label>
          <div className="flex flex-wrap gap-2">
            {MUSCLES.map(({ value, label }) => (
              <button
                key={value}
                onClick={() => toggleMuscle(value)}
                className={cn(
                  'px-4 py-2 rounded-full border-2 transition-all',
                  form.muscleGroups.includes(value)
                    ? 'border-primary bg-primary text-primary-foreground'
                    : 'border-border hover:border-primary/50'
                )}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* Opcoes Avancadas */}
        <div>
          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="text-primary text-sm flex items-center gap-1 hover:underline"
          >
            {showAdvanced ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            {showAdvanced ? 'Menos opcoes' : 'Personalizar mais'}
          </button>

          {showAdvanced && (
            <div className="mt-4 space-y-4 p-4 bg-muted/50 rounded-xl">
              <div>
                <label className="block text-sm font-medium mb-2">Tempo disponivel</label>
                <div className="flex gap-2">
                  {DURATIONS.map(min => (
                    <button
                      key={min}
                      onClick={() => setForm({ ...form, duration: min })}
                      className={cn(
                        'px-4 py-2 rounded-lg border',
                        form.duration === min ? 'border-primary bg-primary/10' : 'border-border'
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
                  className="w-full p-3 border rounded-lg bg-background"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Observacoes</label>
                <textarea
                  value={form.notes || ''}
                  onChange={e => setForm({ ...form, notes: e.target.value })}
                  placeholder="Algo mais que a IA deva saber?"
                  className="w-full p-3 border rounded-lg bg-background h-20 resize-none"
                />
              </div>
            </div>
          )}
        </div>

        {/* Error message */}
        {generateMutation.isError && (
          <div className="p-3 bg-destructive/10 text-destructive rounded-lg text-sm">
            {(generateMutation.error as any)?.message || 'Erro ao gerar treino. Tente novamente.'}
          </div>
        )}

        {/* Botao Gerar */}
        <Button
          onClick={handleGenerate}
          disabled={!canGenerate || generateMutation.isPending}
          className="w-full py-6 text-lg font-bold"
          size="lg"
        >
          {generateMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-5 w-5 animate-spin" />
              Gerando treino...
            </>
          ) : (
            <>
              <Sparkles className="mr-2 h-5 w-5" />
              Gerar Treino
            </>
          )}
        </Button>
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
  const allExercises = [
    ...workout.warmup.map(e => ({ ...e, section: 'Aquecimento' })),
    ...workout.main.map(e => ({ ...e, section: 'Principal' })),
    ...workout.cooldown.map(e => ({ ...e, section: 'Alongamento' })),
  ];

  return (
    <Card className="max-w-2xl mx-auto">
      <CardHeader>
        <CardTitle>{workout.name}</CardTitle>
        <CardDescription>
          {workout.description} • {workout.estimatedDuration} minutos
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Warmup */}
        {workout.warmup.length > 0 && (
          <div>
            <h3 className="font-semibold text-sm text-muted-foreground mb-3">AQUECIMENTO</h3>
            <div className="space-y-2">
              {workout.warmup.map((ex, idx) => (
                <ExerciseCard key={idx} exercise={ex} />
              ))}
            </div>
          </div>
        )}

        {/* Main */}
        <div>
          <h3 className="font-semibold text-sm text-muted-foreground mb-3">EXERCICIOS PRINCIPAIS</h3>
          <div className="space-y-2">
            {workout.main.map((ex, idx) => (
              <ExerciseCard key={idx} exercise={ex} />
            ))}
          </div>
        </div>

        {/* Cooldown */}
        {workout.cooldown.length > 0 && (
          <div>
            <h3 className="font-semibold text-sm text-muted-foreground mb-3">ALONGAMENTO</h3>
            <div className="space-y-2">
              {workout.cooldown.map((ex, idx) => (
                <ExerciseCard key={idx} exercise={ex} />
              ))}
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 pt-4 border-t">
          <Button variant="outline" onClick={onBack} className="flex-1">
            Voltar
          </Button>
          <Button variant="outline" onClick={onRegenerate} disabled={isLoading} className="flex-1">
            {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Regenerar'}
          </Button>
          <Button className="flex-1">
            Iniciar Treino
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// Exercise Card Component
function ExerciseCard({ exercise }: { exercise: GeneratedExercise }) {
  const [showVideo, setShowVideo] = useState(false);

  return (
    <div className="p-3 border rounded-lg">
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <h4 className="font-medium">{exercise.exerciseName}</h4>
          <p className="text-sm text-muted-foreground">
            {exercise.sets} x {exercise.reps} • {exercise.restSeconds}s descanso
          </p>
        </div>
        {exercise.videoUrl && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowVideo(!showVideo)}
          >
            {showVideo ? 'Ocultar' : 'Ver Video'}
          </Button>
        )}
      </div>
      {showVideo && exercise.videoUrl && (
        <div className="mt-3">
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
