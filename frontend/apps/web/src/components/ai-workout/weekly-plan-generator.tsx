'use client';

import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Loader2, Sparkles, ChevronDown, ChevronUp, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { cn } from '@/lib/utils';
import { apiClient } from '@/lib/api';

// Types
type Goal = 'Hipertrofia' | 'Emagrecimento' | 'Forca' | 'Condicionamento';
type Level = 'Iniciante' | 'Intermediario' | 'Avancado';
type Location = 'Academia' | 'Casa';
type SplitType = 'Full Body' | 'Upper/Lower' | 'Push/Pull/Legs' | 'ABC';

interface WeeklyPlanForm {
  goal: Goal | null;
  level: Level | null;
  location: Location | null;
  daysPerWeek: number;
  weeks: number;
  splitType?: SplitType;
  priorityMuscles?: string[];
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

interface WeeklyPlanGeneratorProps {
  onPlanGenerated?: (plan: WeeklyPlanResponse) => void;
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

const DAYS_OPTIONS = [2, 3, 4, 5, 6];
const WEEKS_OPTIONS = [4, 8, 12];

const SPLIT_TYPES: { value: SplitType; label: string; desc: string }[] = [
  { value: 'Full Body', label: 'Full Body', desc: 'Corpo inteiro cada treino' },
  { value: 'Upper/Lower', label: 'Upper/Lower', desc: 'Superior e inferior' },
  { value: 'Push/Pull/Legs', label: 'Push/Pull/Legs', desc: 'Empurrar, puxar, pernas' },
  { value: 'ABC', label: 'ABC', desc: 'Divisao tradicional' },
];

const PRIORITY_MUSCLES = [
  'Peito', 'Costas', 'Ombros', 'Biceps', 'Triceps', 'Pernas', 'Gluteos', 'Abdomen'
];

export function WeeklyPlanGenerator({ onPlanGenerated }: WeeklyPlanGeneratorProps) {
  const [form, setForm] = useState<WeeklyPlanForm>({
    goal: null,
    level: null,
    location: null,
    daysPerWeek: 4,
    weeks: 4,
  });
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [generatedPlan, setGeneratedPlan] = useState<WeeklyPlanResponse | null>(null);

  const generateMutation = useMutation({
    mutationFn: async (data: WeeklyPlanForm) => {
      const response = await apiClient.post<WeeklyPlanResponse>('/ai/v2/weekly-plan', {
        goal: data.goal,
        level: data.level,
        location: data.location,
        daysPerWeek: data.daysPerWeek,
        weeks: data.weeks,
        splitType: data.splitType,
        priorityMuscles: data.priorityMuscles,
        injuries: data.injuries,
        notes: data.notes,
      });
      return response;
    },
    onSuccess: (data) => {
      setGeneratedPlan(data);
      onPlanGenerated?.(data);
    },
  });

  const togglePriorityMuscle = (muscle: string) => {
    const current = form.priorityMuscles || [];
    if (current.includes(muscle)) {
      setForm({ ...form, priorityMuscles: current.filter(m => m !== muscle) });
    } else if (current.length < 3) {
      setForm({ ...form, priorityMuscles: [...current, muscle] });
    }
  };

  const canGenerate = form.goal && form.level && form.location;

  const handleGenerate = () => {
    if (!canGenerate) return;
    generateMutation.mutate(form);
  };

  // If we have a generated plan, show it
  if (generatedPlan) {
    return (
      <PlanDisplay
        plan={generatedPlan}
        onBack={() => setGeneratedPlan(null)}
        onRegenerate={handleGenerate}
        isLoading={generateMutation.isPending}
      />
    );
  }

  return (
    <Card className="max-w-2xl mx-auto">
      <CardHeader className="text-center">
        <CardTitle className="flex items-center justify-center gap-2">
          <Calendar className="h-5 w-5 text-primary" />
          Gerar Plano Semanal
        </CardTitle>
        <CardDescription>Crie um plano de treino completo para varias semanas</CardDescription>
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

        {/* 4. Dias por semana */}
        <div className="space-y-3">
          <label className="block font-semibold">4. Quantos dias por semana?</label>
          <div className="flex gap-2">
            {DAYS_OPTIONS.map(days => (
              <button
                key={days}
                onClick={() => setForm({ ...form, daysPerWeek: days })}
                className={cn(
                  'w-14 h-14 rounded-xl border-2 text-xl font-bold transition-all',
                  form.daysPerWeek === days
                    ? 'border-primary bg-primary text-primary-foreground'
                    : 'border-border hover:border-primary/50'
                )}
              >
                {days}x
              </button>
            ))}
          </div>
        </div>

        {/* 5. Duracao do plano */}
        <div className="space-y-3">
          <label className="block font-semibold">5. Duracao do plano</label>
          <div className="grid grid-cols-3 gap-3">
            {WEEKS_OPTIONS.map(w => (
              <button
                key={w}
                onClick={() => setForm({ ...form, weeks: w })}
                className={cn(
                  'p-4 rounded-xl border-2 transition-all text-center',
                  form.weeks === w
                    ? 'border-primary bg-primary/10'
                    : 'border-border hover:border-primary/50'
                )}
              >
                <span className="block text-2xl font-bold">{w}</span>
                <span className="text-sm text-muted-foreground">semanas</span>
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
                <label className="block text-sm font-medium mb-2">Divisao de treino</label>
                <div className="grid grid-cols-2 gap-2">
                  <button
                    onClick={() => setForm({ ...form, splitType: undefined })}
                    className={cn(
                      'p-3 rounded-lg border text-sm text-left',
                      !form.splitType ? 'border-primary bg-primary/10' : 'border-border'
                    )}
                  >
                    <span className="block font-medium">Deixar IA escolher</span>
                    <span className="text-xs text-muted-foreground">Recomendado</span>
                  </button>
                  {SPLIT_TYPES.map(({ value, label, desc }) => (
                    <button
                      key={value}
                      onClick={() => setForm({ ...form, splitType: value })}
                      className={cn(
                        'p-3 rounded-lg border text-sm text-left',
                        form.splitType === value ? 'border-primary bg-primary/10' : 'border-border'
                      )}
                    >
                      <span className="block font-medium">{label}</span>
                      <span className="text-xs text-muted-foreground">{desc}</span>
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">
                  Musculos para priorizar (max 3)
                </label>
                <div className="flex flex-wrap gap-2">
                  {PRIORITY_MUSCLES.map(muscle => (
                    <button
                      key={muscle}
                      onClick={() => togglePriorityMuscle(muscle)}
                      className={cn(
                        'px-3 py-1.5 rounded-full border text-sm transition-all',
                        form.priorityMuscles?.includes(muscle)
                          ? 'border-primary bg-primary text-primary-foreground'
                          : 'border-border hover:border-primary/50'
                      )}
                    >
                      {muscle}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Lesoes/restricoes</label>
                <input
                  type="text"
                  value={form.injuries || ''}
                  onChange={e => setForm({ ...form, injuries: e.target.value })}
                  placeholder="Ex: Problema no ombro, evitar peso livre..."
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
            {(generateMutation.error as any)?.message || 'Erro ao gerar plano. Tente novamente.'}
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
              Gerando plano...
            </>
          ) : (
            <>
              <Sparkles className="mr-2 h-5 w-5" />
              Gerar Plano de {form.weeks} Semanas
            </>
          )}
        </Button>
      </CardContent>
    </Card>
  );
}

// Plan Display Component
function PlanDisplay({
  plan,
  onBack,
  onRegenerate,
  isLoading,
}: {
  plan: WeeklyPlanResponse;
  onBack: () => void;
  onRegenerate: () => void;
  isLoading: boolean;
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
              className={cn(
                'px-4 py-2 rounded-lg border whitespace-nowrap transition-all',
                selectedWeek === idx
                  ? 'border-primary bg-primary text-primary-foreground'
                  : 'border-border hover:border-primary/50'
              )}
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
                      <div key={exIdx} className="flex items-center justify-between p-2 bg-background rounded">
                        <div>
                          <span className="font-medium">{ex.exerciseName}</span>
                          <span className="text-sm text-muted-foreground ml-2">
                            ({ex.muscleGroup})
                          </span>
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
          <Button variant="outline" onClick={onRegenerate} disabled={isLoading} className="flex-1">
            {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Regenerar'}
          </Button>
          <Button className="flex-1">
            Salvar Plano
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
