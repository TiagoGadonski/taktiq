'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import {
  Sparkles,
  Loader2,
  ChevronLeft,
  ChevronRight,
  Target,
  Dumbbell,
  AlertTriangle,
  Settings,
  Home,
  Building2,
} from 'lucide-react';
import { cn } from '@/lib/utils';

// Types
export interface StructuredWorkoutRequest {
  prompt: string;
  fitnessLevel?: string;
  duration?: number;
  equipment?: string[];
  workoutLocation?: string;
  includeWarmup?: boolean;
  includeCooldown?: boolean;
  includeMobility?: boolean;
  goal?: string;
  secondaryGoal?: string;
  targetMuscles?: string[];
  priorityMuscles?: string[];
  avoidMuscles?: string[];
  injuries?: string[];
  restrictedExercises?: string[];
  favoriteExercises?: string[];
  trainingSplit?: string;
  intensityPreference?: string;
  setsPerMuscle?: number;
  restPreference?: string;
  additionalNotes?: string;
}

interface GenerateWorkoutFormProps {
  onGenerate: (request: StructuredWorkoutRequest) => void;
  isLoading: boolean;
  initialPrompt?: string;
}

const GOALS = [
  { value: 'hipertrofia', label: 'Hipertrofia', description: 'Ganho de massa muscular' },
  { value: 'forca', label: 'Força', description: 'Aumento de força máxima' },
  { value: 'emagrecimento', label: 'Emagrecimento', description: 'Perda de gordura corporal' },
  { value: 'resistencia', label: 'Resistência', description: 'Condicionamento e resistência muscular' },
  { value: 'definicao', label: 'Definição', description: 'Tonificação e definição muscular' },
  { value: 'funcional', label: 'Funcional', description: 'Movimentos funcionais do dia a dia' },
];

const MUSCLE_GROUPS = [
  { value: 'peito', label: 'Peito' },
  { value: 'costas', label: 'Costas' },
  { value: 'ombros', label: 'Ombros' },
  { value: 'biceps', label: 'Bíceps' },
  { value: 'triceps', label: 'Tríceps' },
  { value: 'antebraco', label: 'Antebraço' },
  { value: 'quadriceps', label: 'Quadríceps' },
  { value: 'posterior', label: 'Posterior de Coxa' },
  { value: 'gluteos', label: 'Glúteos' },
  { value: 'panturrilha', label: 'Panturrilha' },
  { value: 'abdomen', label: 'Abdômen' },
  { value: 'lombar', label: 'Lombar' },
  { value: 'trapezio', label: 'Trapézio' },
];

const INJURIES = [
  { value: 'ombro', label: 'Ombro' },
  { value: 'joelho', label: 'Joelho' },
  { value: 'lombar', label: 'Lombar' },
  { value: 'cotovelo', label: 'Cotovelo' },
  { value: 'punho', label: 'Punho' },
  { value: 'quadril', label: 'Quadril' },
  { value: 'tornozelo', label: 'Tornozelo' },
  { value: 'cervical', label: 'Cervical' },
];

const FITNESS_LEVELS = [
  { value: 'beginner', label: 'Iniciante', description: '0-6 meses de treino' },
  { value: 'intermediate', label: 'Intermediário', description: '6 meses - 2 anos' },
  { value: 'advanced', label: 'Avançado', description: '2+ anos de treino' },
];

const INTENSITY_PREFERENCES = [
  { value: 'leve', label: 'Leve', description: 'Ritmo moderado, foco em técnica' },
  { value: 'moderada', label: 'Moderada', description: 'Equilíbrio entre esforço e recuperação' },
  { value: 'intensa', label: 'Intensa', description: 'Alta intensidade, descansos curtos' },
  { value: 'muito_intensa', label: 'Muito Intensa', description: 'Máximo esforço, técnicas avançadas' },
];

const REST_PREFERENCES = [
  { value: 'curto', label: '30-60s', description: 'Descansos curtos' },
  { value: 'medio', label: '60-90s', description: 'Descansos moderados' },
  { value: 'longo', label: '90-180s', description: 'Descansos longos' },
];

export function GenerateWorkoutForm({ onGenerate, isLoading, initialPrompt = '' }: GenerateWorkoutFormProps) {
  const [currentStep, setCurrentStep] = useState(0);

  // Step 1: Goal and Level
  const [goal, setGoal] = useState<string>('');
  const [secondaryGoal, setSecondaryGoal] = useState<string>('');
  const [fitnessLevel, setFitnessLevel] = useState<string>('intermediate');

  // Step 2: Availability
  const [duration, setDuration] = useState<number>(45);
  const [workoutLocation, setWorkoutLocation] = useState<'gym' | 'home' | 'both'>('gym');
  const [includeWarmup, setIncludeWarmup] = useState(false);
  const [includeCooldown, setIncludeCooldown] = useState(false);
  const [includeMobility, setIncludeMobility] = useState(false);

  // Step 3: Muscles
  const [targetMuscles, setTargetMuscles] = useState<string[]>([]);
  const [priorityMuscles, setPriorityMuscles] = useState<string[]>([]);
  const [avoidMuscles, setAvoidMuscles] = useState<string[]>([]);

  // Step 4: Restrictions
  const [injuries, setInjuries] = useState<string[]>([]);
  const [restrictedExercises, setRestrictedExercises] = useState<string>('');
  const [favoriteExercises, setFavoriteExercises] = useState<string>('');

  // Step 5: Preferences
  const [intensityPreference, setIntensityPreference] = useState<string>('moderada');
  const [setsPerMuscle, setSetsPerMuscle] = useState<number>(4);
  const [restPreference, setRestPreference] = useState<string>('medio');
  const [additionalNotes, setAdditionalNotes] = useState<string>(initialPrompt);

  const steps = [
    { title: 'Objetivo', icon: Target, description: 'Defina seu objetivo principal' },
    { title: 'Disponibilidade', icon: Dumbbell, description: 'Configure tempo e local' },
    { title: 'Músculos', icon: Dumbbell, description: 'Selecione os grupos musculares' },
    { title: 'Restrições', icon: AlertTriangle, description: 'Informe lesões e limitações' },
    { title: 'Preferências', icon: Settings, description: 'Ajuste intensidade e detalhes' },
  ];

  const toggleArrayItem = (
    array: string[],
    setArray: React.Dispatch<React.SetStateAction<string[]>>,
    item: string
  ) => {
    if (array.includes(item)) {
      setArray(array.filter((i) => i !== item));
    } else {
      setArray([...array, item]);
    }
  };

  const handleGenerate = () => {
    // Build the prompt from structured data
    let prompt = '';

    if (goal) {
      const goalLabel = GOALS.find(g => g.value === goal)?.label || goal;
      prompt = `Treino focado em ${goalLabel}`;
    }

    if (targetMuscles.length > 0) {
      const muscleLabels = targetMuscles.map(m => MUSCLE_GROUPS.find(mg => mg.value === m)?.label || m);
      prompt += ` trabalhando ${muscleLabels.join(', ')}`;
    }

    if (additionalNotes) {
      prompt += `. ${additionalNotes}`;
    }

    const request: StructuredWorkoutRequest = {
      prompt: prompt || 'Treino personalizado',
      fitnessLevel,
      duration,
      workoutLocation,
      includeWarmup,
      includeCooldown,
      includeMobility,
      goal: goal || undefined,
      secondaryGoal: secondaryGoal || undefined,
      targetMuscles: targetMuscles.length > 0 ? targetMuscles : undefined,
      priorityMuscles: priorityMuscles.length > 0 ? priorityMuscles : undefined,
      avoidMuscles: avoidMuscles.length > 0 ? avoidMuscles : undefined,
      injuries: injuries.length > 0 ? injuries : undefined,
      restrictedExercises: restrictedExercises ? restrictedExercises.split(',').map(s => s.trim()) : undefined,
      favoriteExercises: favoriteExercises ? favoriteExercises.split(',').map(s => s.trim()) : undefined,
      intensityPreference: intensityPreference || undefined,
      setsPerMuscle: setsPerMuscle || undefined,
      restPreference: restPreference || undefined,
      additionalNotes: additionalNotes || undefined,
    };

    onGenerate(request);
  };

  const canProceed = () => {
    switch (currentStep) {
      case 0:
        return goal !== '';
      case 1:
        return duration > 0;
      case 2:
        return true; // Muscles are optional
      case 3:
        return true; // Restrictions are optional
      case 4:
        return true;
      default:
        return true;
    }
  };

  const renderStepIndicator = () => (
    <div className="flex items-center justify-center gap-2 mb-6">
      {steps.map((step, index) => (
        <div key={index} className="flex items-center">
          <button
            onClick={() => setCurrentStep(index)}
            className={cn(
              'w-8 h-8 rounded-full flex items-center justify-center text-xs font-medium transition-all',
              currentStep === index
                ? 'bg-primary text-primary-foreground'
                : index < currentStep
                ? 'bg-primary/20 text-primary'
                : 'bg-muted text-muted-foreground'
            )}
          >
            {index + 1}
          </button>
          {index < steps.length - 1 && (
            <div
              className={cn(
                'w-8 h-0.5 mx-1',
                index < currentStep ? 'bg-primary/50' : 'bg-muted'
              )}
            />
          )}
        </div>
      ))}
    </div>
  );

  const renderStep = () => {
    switch (currentStep) {
      case 0:
        return (
          <div className="space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-medium">Objetivo Principal *</Label>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                {GOALS.map((g) => (
                  <button
                    key={g.value}
                    onClick={() => setGoal(g.value)}
                    className={cn(
                      'p-3 rounded-lg border text-left transition-all',
                      goal === g.value
                        ? 'border-primary bg-primary/10 ring-2 ring-primary'
                        : 'border-border hover:border-primary/50'
                    )}
                  >
                    <div className="font-medium text-sm">{g.label}</div>
                    <div className="text-xs text-muted-foreground mt-1">{g.description}</div>
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">Objetivo Secundário (opcional)</Label>
              <div className="flex flex-wrap gap-2">
                {GOALS.filter((g) => g.value !== goal).map((g) => (
                  <Badge
                    key={g.value}
                    variant={secondaryGoal === g.value ? 'default' : 'outline'}
                    className="cursor-pointer"
                    onClick={() => setSecondaryGoal(secondaryGoal === g.value ? '' : g.value)}
                  >
                    {g.label}
                  </Badge>
                ))}
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">Nível de Condicionamento</Label>
              <div className="grid grid-cols-3 gap-3">
                {FITNESS_LEVELS.map((level) => (
                  <button
                    key={level.value}
                    onClick={() => setFitnessLevel(level.value)}
                    className={cn(
                      'p-3 rounded-lg border text-center transition-all',
                      fitnessLevel === level.value
                        ? 'border-primary bg-primary/10 ring-2 ring-primary'
                        : 'border-border hover:border-primary/50'
                    )}
                  >
                    <div className="font-medium text-sm">{level.label}</div>
                    <div className="text-xs text-muted-foreground mt-1">{level.description}</div>
                  </button>
                ))}
              </div>
            </div>
          </div>
        );

      case 1:
        return (
          <div className="space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-medium">Duração do Treino</Label>
              <div className="grid grid-cols-4 gap-3">
                {[30, 45, 60, 90].map((mins) => (
                  <button
                    key={mins}
                    onClick={() => setDuration(mins)}
                    className={cn(
                      'p-3 rounded-lg border text-center transition-all',
                      duration === mins
                        ? 'border-primary bg-primary/10 ring-2 ring-primary'
                        : 'border-border hover:border-primary/50'
                    )}
                  >
                    <div className="font-medium">{mins}</div>
                    <div className="text-xs text-muted-foreground">minutos</div>
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">Local do Treino</Label>
              <div className="grid grid-cols-3 gap-3">
                <button
                  onClick={() => setWorkoutLocation('gym')}
                  className={cn(
                    'p-4 rounded-lg border text-center transition-all flex flex-col items-center gap-2',
                    workoutLocation === 'gym'
                      ? 'border-primary bg-primary/10 ring-2 ring-primary'
                      : 'border-border hover:border-primary/50'
                  )}
                >
                  <Building2 className="h-6 w-6" />
                  <div className="font-medium text-sm">Academia</div>
                </button>
                <button
                  onClick={() => setWorkoutLocation('home')}
                  className={cn(
                    'p-4 rounded-lg border text-center transition-all flex flex-col items-center gap-2',
                    workoutLocation === 'home'
                      ? 'border-primary bg-primary/10 ring-2 ring-primary'
                      : 'border-border hover:border-primary/50'
                  )}
                >
                  <Home className="h-6 w-6" />
                  <div className="font-medium text-sm">Casa</div>
                </button>
                <button
                  onClick={() => setWorkoutLocation('both')}
                  className={cn(
                    'p-4 rounded-lg border text-center transition-all flex flex-col items-center gap-2',
                    workoutLocation === 'both'
                      ? 'border-primary bg-primary/10 ring-2 ring-primary'
                      : 'border-border hover:border-primary/50'
                  )}
                >
                  <div className="flex gap-1">
                    <Home className="h-5 w-5" />
                    <Building2 className="h-5 w-5" />
                  </div>
                  <div className="font-medium text-sm">Ambos</div>
                </button>
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">Opções Adicionais</Label>
              <div className="space-y-3">
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="warmup"
                    checked={includeWarmup}
                    onCheckedChange={(checked) => setIncludeWarmup(checked as boolean)}
                  />
                  <Label htmlFor="warmup" className="font-normal cursor-pointer">
                    Incluir aquecimento (5-10 min)
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="mobility"
                    checked={includeMobility}
                    onCheckedChange={(checked) => setIncludeMobility(checked as boolean)}
                  />
                  <Label htmlFor="mobility" className="font-normal cursor-pointer">
                    Incluir mobilidade articular
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="cooldown"
                    checked={includeCooldown}
                    onCheckedChange={(checked) => setIncludeCooldown(checked as boolean)}
                  />
                  <Label htmlFor="cooldown" className="font-normal cursor-pointer">
                    Incluir alongamento final (5-10 min)
                  </Label>
                </div>
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-medium">
                Músculos Alvo
                <span className="text-muted-foreground font-normal ml-2">
                  (selecione os grupos que deseja trabalhar)
                </span>
              </Label>
              <div className="flex flex-wrap gap-2">
                {MUSCLE_GROUPS.map((muscle) => (
                  <Badge
                    key={muscle.value}
                    variant={targetMuscles.includes(muscle.value) ? 'default' : 'outline'}
                    className="cursor-pointer"
                    onClick={() => toggleArrayItem(targetMuscles, setTargetMuscles, muscle.value)}
                  >
                    {muscle.label}
                  </Badge>
                ))}
              </div>
            </div>

            {targetMuscles.length > 0 && (
              <div className="space-y-3">
                <Label className="text-base font-medium">
                  Músculos Prioritários
                  <span className="text-muted-foreground font-normal ml-2">
                    (quais devem receber mais volume?)
                  </span>
                </Label>
                <div className="flex flex-wrap gap-2">
                  {targetMuscles.map((muscleValue) => {
                    const muscle = MUSCLE_GROUPS.find((m) => m.value === muscleValue);
                    return (
                      <Badge
                        key={muscleValue}
                        variant={priorityMuscles.includes(muscleValue) ? 'default' : 'secondary'}
                        className="cursor-pointer"
                        onClick={() => toggleArrayItem(priorityMuscles, setPriorityMuscles, muscleValue)}
                      >
                        {muscle?.label}
                        {priorityMuscles.includes(muscleValue) && ' ★'}
                      </Badge>
                    );
                  })}
                </div>
              </div>
            )}

            <div className="space-y-3">
              <Label className="text-base font-medium">
                Músculos a Evitar
                <span className="text-muted-foreground font-normal ml-2">
                  (grupos que não devem ser trabalhados)
                </span>
              </Label>
              <div className="flex flex-wrap gap-2">
                {MUSCLE_GROUPS.filter((m) => !targetMuscles.includes(m.value)).map((muscle) => (
                  <Badge
                    key={muscle.value}
                    variant={avoidMuscles.includes(muscle.value) ? 'destructive' : 'outline'}
                    className="cursor-pointer"
                    onClick={() => toggleArrayItem(avoidMuscles, setAvoidMuscles, muscle.value)}
                  >
                    {muscle.label}
                  </Badge>
                ))}
              </div>
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-medium">
                Lesões ou Áreas Sensíveis
                <span className="text-muted-foreground font-normal ml-2">
                  (a IA evitará exercícios problemáticos)
                </span>
              </Label>
              <div className="flex flex-wrap gap-2">
                {INJURIES.map((injury) => (
                  <Badge
                    key={injury.value}
                    variant={injuries.includes(injury.value) ? 'destructive' : 'outline'}
                    className="cursor-pointer"
                    onClick={() => toggleArrayItem(injuries, setInjuries, injury.value)}
                  >
                    {injury.label}
                  </Badge>
                ))}
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">
                Exercícios a Evitar
                <span className="text-muted-foreground font-normal ml-2">(opcional)</span>
              </Label>
              <textarea
                value={restrictedExercises}
                onChange={(e) => setRestrictedExercises(e.target.value)}
                placeholder="Ex: supino inclinado, agachamento smith (separados por vírgula)"
                className="w-full min-h-[80px] p-3 rounded-md border border-input bg-background text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">
                Exercícios Favoritos
                <span className="text-muted-foreground font-normal ml-2">
                  (a IA tentará incluir se possível)
                </span>
              </Label>
              <textarea
                value={favoriteExercises}
                onChange={(e) => setFavoriteExercises(e.target.value)}
                placeholder="Ex: crucifixo, leg press, rosca direta (separados por vírgula)"
                className="w-full min-h-[80px] p-3 rounded-md border border-input bg-background text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
          </div>
        );

      case 4:
        return (
          <div className="space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-medium">Intensidade Preferida</Label>
              <div className="grid grid-cols-2 gap-3">
                {INTENSITY_PREFERENCES.map((intensity) => (
                  <button
                    key={intensity.value}
                    onClick={() => setIntensityPreference(intensity.value)}
                    className={cn(
                      'p-3 rounded-lg border text-left transition-all',
                      intensityPreference === intensity.value
                        ? 'border-primary bg-primary/10 ring-2 ring-primary'
                        : 'border-border hover:border-primary/50'
                    )}
                  >
                    <div className="font-medium text-sm">{intensity.label}</div>
                    <div className="text-xs text-muted-foreground mt-1">{intensity.description}</div>
                  </button>
                ))}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-3">
                <Label className="text-base font-medium">Séries por Músculo</Label>
                <div className="grid grid-cols-3 gap-2">
                  {[3, 4, 5].map((sets) => (
                    <button
                      key={sets}
                      onClick={() => setSetsPerMuscle(sets)}
                      className={cn(
                        'p-2 rounded-lg border text-center transition-all',
                        setsPerMuscle === sets
                          ? 'border-primary bg-primary/10 ring-2 ring-primary'
                          : 'border-border hover:border-primary/50'
                      )}
                    >
                      {sets}
                    </button>
                  ))}
                </div>
              </div>

              <div className="space-y-3">
                <Label className="text-base font-medium">Tempo de Descanso</Label>
                <div className="grid grid-cols-3 gap-2">
                  {REST_PREFERENCES.map((rest) => (
                    <button
                      key={rest.value}
                      onClick={() => setRestPreference(rest.value)}
                      className={cn(
                        'p-2 rounded-lg border text-center transition-all text-xs',
                        restPreference === rest.value
                          ? 'border-primary bg-primary/10 ring-2 ring-primary'
                          : 'border-border hover:border-primary/50'
                      )}
                    >
                      {rest.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <Label className="text-base font-medium">
                Observações Adicionais
                <span className="text-muted-foreground font-normal ml-2">(opcional)</span>
              </Label>
              <textarea
                value={additionalNotes}
                onChange={(e) => setAdditionalNotes(e.target.value)}
                placeholder="Ex: Prefiro começar com exercícios compostos. Tenho apenas 2 pares de halteres em casa..."
                className="w-full min-h-[100px] p-3 rounded-md border border-input bg-background text-sm resize-none focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  const StepIcon = steps[currentStep].icon;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          {StepIcon && <StepIcon className="h-5 w-5" />}
          {steps[currentStep].title}
        </CardTitle>
        <CardDescription>{steps[currentStep].description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {renderStepIndicator()}
        {renderStep()}

        <div className="flex justify-between pt-4 border-t">
          <Button
            variant="outline"
            onClick={() => setCurrentStep((prev) => Math.max(0, prev - 1))}
            disabled={currentStep === 0 || isLoading}
          >
            <ChevronLeft className="h-4 w-4 mr-1" />
            Voltar
          </Button>

          {currentStep < steps.length - 1 ? (
            <Button
              onClick={() => setCurrentStep((prev) => prev + 1)}
              disabled={!canProceed() || isLoading}
            >
              Próximo
              <ChevronRight className="h-4 w-4 ml-1" />
            </Button>
          ) : (
            <Button onClick={handleGenerate} disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Gerando...
                </>
              ) : (
                <>
                  <Sparkles className="mr-2 h-4 w-4" />
                  Gerar Treino
                </>
              )}
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
