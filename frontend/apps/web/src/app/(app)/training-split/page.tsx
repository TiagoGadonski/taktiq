'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/hooks/use-toast';
import { Dumbbell, Save, Calendar } from 'lucide-react';

interface TrainingSplitData {
  [key: string]: string;
}

const DAYS_OF_WEEK = [
  { key: '0', label: 'Domingo', emoji: '☀️' },
  { key: '1', label: 'Segunda', emoji: '💪' },
  { key: '2', label: 'Terça', emoji: '🔥' },
  { key: '3', label: 'Quarta', emoji: '⚡' },
  { key: '4', label: 'Quinta', emoji: '💯' },
  { key: '5', label: 'Sexta', emoji: '🎯' },
  { key: '6', label: 'Sábado', emoji: '🏆' },
];

const SUGGESTED_SPLITS = [
  {
    name: 'Push Pull Legs',
    description: '3 dias por semana',
    split: {
      '0': 'Descanso',
      '1': 'Push (Peito, Ombros, Tríceps)',
      '2': 'Descanso',
      '3': 'Pull (Costas, Bíceps)',
      '4': 'Descanso',
      '5': 'Legs (Pernas, Glúteos)',
      '6': 'Descanso',
    },
  },
  {
    name: 'Upper Lower',
    description: '4 dias por semana',
    split: {
      '0': 'Descanso',
      '1': 'Membros Superiores',
      '2': 'Membros Inferiores',
      '3': 'Descanso',
      '4': 'Membros Superiores',
      '5': 'Membros Inferiores',
      '6': 'Descanso',
    },
  },
  {
    name: 'Bro Split',
    description: '5 dias por semana',
    split: {
      '0': 'Descanso',
      '1': 'Peito',
      '2': 'Costas',
      '3': 'Pernas',
      '4': 'Ombros',
      '5': 'Braços',
      '6': 'Descanso',
    },
  },
];

export default function TrainingSplitPage() {
  const { user } = useAuth();
  const { toast } = useToast();
  const [trainingSplit, setTrainingSplit] = useState<TrainingSplitData>({
    '0': '',
    '1': '',
    '2': '',
    '3': '',
    '4': '',
    '5': '',
    '6': '',
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  // Load existing training split
  useEffect(() => {
    const loadTrainingSplit = async () => {
      setLoading(true);
      try {
        const response = await fetch('/api/me', {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`,
          },
        });

        if (response.ok) {
          const data = await response.json();
          if (data.trainingSplit) {
            const parsed = JSON.parse(data.trainingSplit);
            setTrainingSplit(parsed);
          }
        }
      } catch (error) {
        console.error('Error loading training split:', error);
      } finally {
        setLoading(false);
      }
    };

    loadTrainingSplit();
  }, []);

  const handleDayChange = (dayKey: string, value: string) => {
    setTrainingSplit((prev) => ({
      ...prev,
      [dayKey]: value,
    }));
  };

  const applySuggestedSplit = (suggestedSplit: TrainingSplitData) => {
    setTrainingSplit(suggestedSplit);
    toast({
      title: 'Divisão aplicada!',
      description: 'Você pode personalizar conforme necessário.',
    });
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      // Get current user profile
      const getResponse = await fetch('/api/me', {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
      });

      if (!getResponse.ok) throw new Error('Failed to get profile');

      const currentProfile = await getResponse.json();

      // Update with training split
      const response = await fetch('/api/me', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({
          ...currentProfile,
          trainingSplit: JSON.stringify(trainingSplit),
        }),
      });

      if (response.ok) {
        toast({
          title: 'Divisão salva!',
          description: 'Sua divisão de treinos foi atualizada com sucesso.',
        });
      } else {
        throw new Error('Failed to save');
      }
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível salvar a divisão. Tente novamente.',
        variant: 'destructive',
      });
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex h-96 items-center justify-center">
        <p>Carregando...</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-4xl space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold flex items-center gap-3">
          <Calendar className="h-8 w-8 text-primary" />
          Divisão de Treinos Semanal
        </h1>
        <p className="text-muted-foreground mt-2">
          Configure quais grupos musculares você treina em cada dia da semana. O sistema irá sugerir treinos
          automaticamente baseado nesta divisão.
        </p>
      </div>

      {/* Suggested Splits */}
      <div className="grid gap-4 md:grid-cols-3">
        {SUGGESTED_SPLITS.map((preset) => (
          <Card
            key={preset.name}
            className="cursor-pointer transition-all hover:border-primary hover-lift tap-scale"
            onClick={() => applySuggestedSplit(preset.split)}
          >
            <CardHeader>
              <CardTitle className="text-base">{preset.name}</CardTitle>
              <CardDescription>{preset.description}</CardDescription>
            </CardHeader>
          </Card>
        ))}
      </div>

      {/* Weekly Split Configuration */}
      <Card className="glass-card">
        <CardHeader>
          <CardTitle>Configure Sua Divisão</CardTitle>
          <CardDescription>
            Defina o foco de cada dia. Exemplos: "Peito e Tríceps", "Pernas", "Descanso", etc.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {DAYS_OF_WEEK.map((day) => (
            <div key={day.key} className="space-y-2">
              <Label htmlFor={`day-${day.key}`} className="flex items-center gap-2">
                <span className="text-xl">{day.emoji}</span>
                <span className="font-semibold">{day.label}</span>
              </Label>
              <Input
                id={`day-${day.key}`}
                value={trainingSplit[day.key] || ''}
                onChange={(e) => handleDayChange(day.key, e.target.value)}
                placeholder="Ex: Peito e Tríceps, Descanso, Cardio..."
                className="w-full"
              />
            </div>
          ))}

          <div className="flex gap-3 pt-4">
            <Button onClick={handleSave} disabled={saving} className="flex-1 hover-lift tap-scale">
              <Save className="mr-2 h-4 w-4" />
              {saving ? 'Salvando...' : 'Salvar Divisão'}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Preview */}
      <Card className="glass-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Dumbbell className="h-5 w-5 text-primary" />
            Prévia da Semana
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-3 sm:grid-cols-2 md:grid-cols-4">
            {DAYS_OF_WEEK.map((day) => (
              <div
                key={day.key}
                className={`rounded-lg border p-3 ${
                  trainingSplit[day.key]?.toLowerCase().includes('descanso')
                    ? 'bg-muted/50'
                    : 'bg-primary/5 border-primary/20'
                }`}
              >
                <div className="font-semibold text-sm flex items-center gap-2">
                  <span>{day.emoji}</span>
                  <span>{day.label}</span>
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  {trainingSplit[day.key] || 'Não configurado'}
                </p>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
