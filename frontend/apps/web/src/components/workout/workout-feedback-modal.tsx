'use client';

import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { AlertCircle, Heart, Zap, ThumbsUp, Smile, Meh, Frown } from 'lucide-react';
import { cn } from '@/lib/utils';

interface WorkoutFeedbackModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (feedback: FeedbackData) => Promise<void>;
  sessionId: string;
}

export interface FeedbackData {
  difficultyRating: number;
  energyLevel: number;
  overallSatisfaction: number;
  painAreas?: string[];
  favoriteExercises?: string[];
  dislikedExercises?: string[];
  comments?: string;
}

const PAIN_AREAS = [
  'Lombar',
  'Joelho',
  'Ombro',
  'Pescoço',
  'Quadril',
  'Cotovelo',
  'Punho',
  'Tornozelo'
];

export function WorkoutFeedbackModal({ open, onClose, onSubmit, sessionId }: WorkoutFeedbackModalProps) {
  const [difficulty, setDifficulty] = useState(3);
  const [energy, setEnergy] = useState(3);
  const [satisfaction, setSatisfaction] = useState(4);
  const [painAreas, setPainAreas] = useState<string[]>([]);
  const [comments, setComments] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      await onSubmit({
        difficultyRating: difficulty,
        energyLevel: energy,
        overallSatisfaction: satisfaction,
        painAreas: painAreas.length > 0 ? painAreas : undefined,
        comments: comments.trim() || undefined
      });
      onClose();
    } catch (error) {
    } finally{
      setIsSubmitting(false);
    }
  };

  const handleSkip = () => {
    onClose();
  };

  const togglePainArea = (area: string) => {
    setPainAreas(prev =>
      prev.includes(area)
        ? prev.filter(a => a !== area)
        : [...prev, area]
    );
  };

  const RatingButton = ({
    value,
    currentValue,
    onClick,
    label
  }: {
    value: number;
    currentValue: number;
    onClick: () => void;
    label: string;
  }) => (
    <Button
      type="button"
      variant={currentValue === value ? 'default' : 'outline'}
      size="lg"
      onClick={onClick}
      className={cn(
        'w-full sm:w-auto min-w-[60px] font-semibold text-lg',
        currentValue === value && 'ring-2 ring-primary ring-offset-2'
      )}
    >
      {value}
    </Button>
  );

  const getDifficultyIcon = (rating: number) => {
    if (rating <= 2) return <Smile className="h-5 w-5" />;
    if (rating <= 3) return <Meh className="h-5 w-5" />;
    return <Frown className="h-5 w-5" />;
  };

  const getEnergyIcon = (rating: number) => {
    return <Zap className={cn('h-5 w-5', rating >= 4 ? 'text-yellow-500' : 'text-gray-400')} />;
  };

  const getSatisfactionIcon = (rating: number) => {
    if (rating >= 4) return <Heart className="h-5 w-5 text-red-500" />;
    if (rating >= 3) return <ThumbsUp className="h-5 w-5 text-blue-500" />;
    return <Meh className="h-5 w-5 text-gray-400" />;
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleSkip()}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl flex items-center gap-2">
            <Heart className="h-6 w-6 text-red-500" />
            Como foi o treino?
          </DialogTitle>
          <p className="text-sm text-muted-foreground">
            Seu feedback nos ajuda a personalizar melhor seus próximos treinos!
          </p>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Dificuldade */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label className="text-base font-semibold flex items-center gap-2">
                {getDifficultyIcon(difficulty)}
                Dificuldade do Treino
              </Label>
              <span className="text-sm text-muted-foreground">
                {difficulty <= 2 && 'Muito fácil'}
                {difficulty === 3 && 'Adequado'}
                {difficulty >= 4 && 'Desafiador'}
              </span>
            </div>
            <div className="flex flex-wrap gap-2">
              {[1, 2, 3, 4, 5].map(val => (
                <RatingButton
                  key={val}
                  value={val}
                  currentValue={difficulty}
                  onClick={() => setDifficulty(val)}
                  label={val.toString()}
                />
              ))}
            </div>
            <p className="text-xs text-muted-foreground">
              1 = Muito fácil • 5 = Muito difícil
            </p>
          </div>

          {/* Nível de Energia */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label className="text-base font-semibold flex items-center gap-2">
                {getEnergyIcon(energy)}
                Nível de Energia Após o Treino
              </Label>
              <span className="text-sm text-muted-foreground">
                {energy <= 2 && 'Exausto'}
                {energy === 3 && 'Normal'}
                {energy >= 4 && 'Energizado'}
              </span>
            </div>
            <div className="flex flex-wrap gap-2">
              {[1, 2, 3, 4, 5].map(val => (
                <RatingButton
                  key={val}
                  value={val}
                  currentValue={energy}
                  onClick={() => setEnergy(val)}
                  label={val.toString()}
                />
              ))}
            </div>
            <p className="text-xs text-muted-foreground">
              1 = Exausto • 5 = Muito energizado
            </p>
          </div>

          {/* Satisfação */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label className="text-base font-semibold flex items-center gap-2">
                {getSatisfactionIcon(satisfaction)}
                Satisfação Geral
              </Label>
              <span className="text-sm text-muted-foreground">
                {satisfaction <= 2 && 'Ruim'}
                {satisfaction === 3 && 'Ok'}
                {satisfaction >= 4 && 'Excelente'}
              </span>
            </div>
            <div className="flex flex-wrap gap-2">
              {[1, 2, 3, 4, 5].map(val => (
                <RatingButton
                  key={val}
                  value={val}
                  currentValue={satisfaction}
                  onClick={() => setSatisfaction(val)}
                  label={val.toString()}
                />
              ))}
            </div>
            <p className="text-xs text-muted-foreground">
              1 = Péssimo • 5 = Excelente
            </p>
          </div>

          {/* Áreas com Dor */}
          <div className="space-y-3">
            <Label className="text-base font-semibold flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-orange-500" />
              Sentiu dor ou desconforto em alguma área?
              <span className="text-xs font-normal text-muted-foreground ml-2">(opcional)</span>
            </Label>
            <div className="flex flex-wrap gap-2">
              {PAIN_AREAS.map(area => (
                <Badge
                  key={area}
                  variant={painAreas.includes(area) ? 'destructive' : 'outline'}
                  className="cursor-pointer transition-all hover:scale-105"
                  onClick={() => togglePainArea(area)}
                >
                  {area}
                </Badge>
              ))}
            </div>
            {painAreas.length > 0 && (
              <p className="text-sm text-orange-600 dark:text-orange-400">
                ⚠️ Selecionadas: {painAreas.join(', ')}
              </p>
            )}
          </div>

          {/* Comentários */}
          <div className="space-y-3">
            <Label htmlFor="comments" className="text-base font-semibold">
              Comentários Adicionais
              <span className="text-xs font-normal text-muted-foreground ml-2">(opcional)</span>
            </Label>
            <Textarea
              id="comments"
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              placeholder="Como se sentiu? Algum exercício específico que gostou ou não gostou?"
              className="min-h-[100px] resize-none"
              maxLength={500}
            />
            <p className="text-xs text-muted-foreground text-right">
              {comments.length}/500 caracteres
            </p>
          </div>
        </div>

        <DialogFooter className="gap-2 sm:gap-0">
          <Button
            variant="ghost"
            onClick={handleSkip}
            disabled={isSubmitting}
          >
            Pular
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="min-w-[120px]"
          >
            {isSubmitting ? 'Enviando...' : 'Enviar Feedback'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
