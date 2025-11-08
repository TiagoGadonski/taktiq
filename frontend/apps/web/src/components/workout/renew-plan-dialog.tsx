'use client';

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { RefreshCw, Copy, Calendar } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import { api } from '@/lib/api';

interface RenewPlanDialogProps {
  planId: string;
  planName: string;
  isExpired: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

type RenewalAction = 'extend' | 'duplicate';

export function RenewPlanDialog({
  planId,
  planName,
  isExpired,
  open,
  onOpenChange,
}: RenewPlanDialogProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [action, setAction] = useState<RenewalAction>('extend');
  const [duration, setDuration] = useState(4);

  const renewMutation = useMutation({
    mutationFn: async () => {
      if (action === 'extend') {
        // Call API to extend the plan
        return api.workoutPlans.renew(planId, { additionalWeeks: duration });
      } else {
        // Call API to duplicate the plan
        return api.workoutPlans.duplicate(planId, { duration });
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      queryClient.invalidateQueries({ queryKey: ['workout-plan', planId] });
      toast({
        title: action === 'extend' ? 'Plano renovado!' : 'Novo plano criado!',
        description:
          action === 'extend'
            ? `O plano foi estendido por mais ${duration} semanas.`
            : `Um novo plano de ${duration} semanas foi criado baseado no anterior.`,
      });
      onOpenChange(false);
      // Reset form
      setAction('extend');
      setDuration(4);
    },
    onError: () => {
      toast({
        title: 'Erro',
        description: 'Não foi possível renovar o plano. Tente novamente.',
        variant: 'destructive',
      });
    },
  });

  const handleRenew = () => {
    renewMutation.mutate();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {isExpired ? 'Renovar Plano Expirado' : 'Continuar Plano'}
          </DialogTitle>
          <DialogDescription>
            {isExpired
              ? `O plano "${planName}" expirou. Escolha como deseja continuar.`
              : `Continue seu progresso com o plano "${planName}".`}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Action Selection */}
          <div className="space-y-3">
            <Label>Como deseja continuar?</Label>
            <div className="grid gap-3">
              <button
                onClick={() => setAction('extend')}
                className={`flex items-start gap-3 rounded-lg border-2 p-4 text-left transition-all ${
                  action === 'extend'
                    ? 'border-primary bg-primary/5'
                    : 'border-border hover:border-primary/50'
                }`}
              >
                <RefreshCw className="h-5 w-5 mt-0.5 flex-shrink-0" />
                <div className="flex-1">
                  <div className="font-medium">Estender Plano Atual</div>
                  <div className="text-sm text-muted-foreground mt-1">
                    Adicione mais semanas ao plano existente mantendo o histórico
                  </div>
                </div>
              </button>

              <button
                onClick={() => setAction('duplicate')}
                className={`flex items-start gap-3 rounded-lg border-2 p-4 text-left transition-all ${
                  action === 'duplicate'
                    ? 'border-primary bg-primary/5'
                    : 'border-border hover:border-primary/50'
                }`}
              >
                <Copy className="h-5 w-5 mt-0.5 flex-shrink-0" />
                <div className="flex-1">
                  <div className="font-medium">Criar Novo Plano</div>
                  <div className="text-sm text-muted-foreground mt-1">
                    Comece um novo plano baseado nos mesmos exercícios e treinos
                  </div>
                </div>
              </button>
            </div>
          </div>

          {/* Duration Selection */}
          <div className="space-y-3">
            <Label className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              {action === 'extend' ? 'Adicionar Duração' : 'Duração do Novo Plano'}
            </Label>
            <div className="grid grid-cols-4 gap-2">
              {[4, 6, 8, 12].map((weeks) => (
                <button
                  key={weeks}
                  onClick={() => setDuration(weeks)}
                  className={`rounded-lg border-2 px-4 py-3 text-sm font-medium transition-all ${
                    duration === weeks
                      ? 'border-primary bg-primary text-primary-foreground'
                      : 'border-border hover:border-primary/50'
                  }`}
                >
                  {weeks} sem
                </button>
              ))}
            </div>
          </div>

          {/* Summary */}
          <div className="rounded-lg bg-muted/50 p-4 space-y-2">
            <div className="text-sm font-medium">Resumo</div>
            <div className="text-sm text-muted-foreground">
              {action === 'extend' ? (
                <>
                  O plano <strong>{planName}</strong> será estendido por mais{' '}
                  <strong>{duration} semanas</strong>.
                </>
              ) : (
                <>
                  Um novo plano de <strong>{duration} semanas</strong> será criado baseado em{' '}
                  <strong>{planName}</strong>.
                </>
              )}
            </div>
          </div>
        </div>

        <div className="flex gap-3">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={renewMutation.isPending}
            className="flex-1"
          >
            Cancelar
          </Button>
          <Button
            onClick={handleRenew}
            disabled={renewMutation.isPending}
            className="flex-1"
          >
            {renewMutation.isPending
              ? 'Processando...'
              : action === 'extend'
                ? 'Estender Plano'
                : 'Criar Novo Plano'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
