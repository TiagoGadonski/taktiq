'use client';

import { useEffect, useState } from 'react';
import { CheckCircle2, Trophy, Zap } from 'lucide-react';
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface WorkoutCompletionModalProps {
  open: boolean;
  onComplete: () => void;
  autoCompleteDelay?: number; // milliseconds
}

export function WorkoutCompletionModal({
  open,
  onComplete,
  autoCompleteDelay = 5000, // 5 seconds default
}: WorkoutCompletionModalProps) {
  const [countdown, setCountdown] = useState(Math.floor(autoCompleteDelay / 1000));

  useEffect(() => {
    if (!open) {
      setCountdown(Math.floor(autoCompleteDelay / 1000));
      return;
    }

    // Auto-complete after delay
    const completeTimer = setTimeout(() => {
      onComplete();
    }, autoCompleteDelay);

    // Countdown timer for display
    const countdownInterval = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(countdownInterval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => {
      clearTimeout(completeTimer);
      clearInterval(countdownInterval);
    };
  }, [open, autoCompleteDelay, onComplete]);

  return (
    <Dialog open={open} onOpenChange={() => {}}>
      <DialogContent className="sm:max-w-md" hideCloseButton>
        <div className="flex flex-col items-center justify-center py-8 space-y-6">
          {/* Animated Trophy Icon */}
          <div className="relative">
            <div className="absolute inset-0 animate-ping opacity-20">
              <Trophy className="h-24 w-24 text-yellow-500" />
            </div>
            <Trophy className="h-24 w-24 text-yellow-500 relative z-10" />
          </div>

          {/* Congratulations Message */}
          <div className="text-center space-y-2">
            <h2 className="text-3xl font-bold">Parabéns! 🎉</h2>
            <p className="text-lg text-muted-foreground">
              Você completou todos os exercícios!
            </p>
          </div>

          {/* Stats/Achievements */}
          <div className="flex gap-4 items-center justify-center">
            <div className="flex items-center gap-2 px-4 py-2 bg-primary/10 rounded-full">
              <CheckCircle2 className="h-5 w-5 text-green-500" />
              <span className="font-semibold">Treino Completo</span>
            </div>
            <div className="flex items-center gap-2 px-4 py-2 bg-primary/10 rounded-full">
              <Zap className="h-5 w-5 text-yellow-500" />
              <span className="font-semibold">Excelente!</span>
            </div>
          </div>

          {/* Motivational Quote */}
          <div className="text-center max-w-sm">
            <p className="text-sm text-muted-foreground italic">
              "O sucesso é a soma de pequenos esforços repetidos dia após dia."
            </p>
          </div>

          {/* Countdown and Action */}
          <div className="flex flex-col items-center gap-3 w-full">
            <Button
              onClick={onComplete}
              className="w-full max-w-xs"
              size="lg"
            >
              Finalizar Treino
            </Button>
            <p className="text-xs text-muted-foreground">
              Finalizando automaticamente em {countdown} segundo{countdown !== 1 ? 's' : ''}...
            </p>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
