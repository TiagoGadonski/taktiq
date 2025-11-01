'use client';

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Share2, Lock, Users, Globe, Check } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import { Checkbox } from '@/components/ui/checkbox';

enum VisibilityLevel {
  Private = 0,
  FriendsOnly = 1,
  Public = 2,
}

interface ShareSettingsDialogProps {
  planId: string;
  planName: string;
  currentVisibility?: number;
  currentAllowCopying?: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ShareSettingsDialog({
  planId,
  planName,
  currentVisibility = VisibilityLevel.Private,
  currentAllowCopying = true,
  open,
  onOpenChange,
}: ShareSettingsDialogProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [visibility, setVisibility] = useState<VisibilityLevel>(currentVisibility);
  const [allowCopying, setAllowCopying] = useState(currentAllowCopying);

  const updateVisibilityMutation = useMutation({
    mutationFn: (data: { visibilityLevel: number; allowCopying: boolean }) =>
      api.workoutPlans.updateVisibility(planId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Configurações atualizadas!',
        description: 'As configurações de compartilhamento foram atualizadas com sucesso.',
      });
      onOpenChange(false);
    },
    onError: () => {
      toast({
        title: 'Erro',
        description: 'Não foi possível atualizar as configurações.',
        variant: 'destructive',
      });
    },
  });

  const handleSave = () => {
    updateVisibilityMutation.mutate({
      visibilityLevel: visibility,
      allowCopying,
    });
  };

  const visibilityOptions = [
    {
      value: VisibilityLevel.Private,
      label: 'Privado',
      description: 'Apenas você pode ver este plano',
      icon: Lock,
    },
    {
      value: VisibilityLevel.FriendsOnly,
      label: 'Amigos',
      description: 'Apenas seus amigos confirmados podem ver',
      icon: Users,
    },
    {
      value: VisibilityLevel.Public,
      label: 'Público',
      description: 'Qualquer pessoa pode visualizar e pesquisar',
      icon: Globe,
    },
  ];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-lg sm:text-xl">
            <Share2 className="h-4 w-4 sm:h-5 sm:w-5" />
            <span className="line-clamp-1">Configurações de Compartilhamento</span>
          </DialogTitle>
          <DialogDescription className="text-sm">
            Configure quem pode visualizar "<span className="line-clamp-1 inline">{planName}</span>"
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          <div className="space-y-3">
            <Label>Visibilidade</Label>
            <div className="space-y-2">
              {visibilityOptions.map((option) => {
                const Icon = option.icon;
                const isSelected = visibility === option.value;

                return (
                  <button
                    key={option.value}
                    onClick={() => setVisibility(option.value)}
                    className={`w-full flex items-start gap-3 p-3 sm:p-4 rounded-lg border-2 transition-all text-left touch-manipulation ${
                      isSelected
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:border-primary/50 active:border-primary/30'
                    }`}
                  >
                    <div className={`mt-0.5 flex-shrink-0 ${isSelected ? 'text-primary' : 'text-muted-foreground'}`}>
                      <Icon className="h-5 w-5 sm:h-6 sm:w-6" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-sm sm:text-base">{option.label}</span>
                        {isSelected && <Check className="h-4 w-4 text-primary flex-shrink-0" />}
                      </div>
                      <p className="text-xs sm:text-sm text-muted-foreground mt-1">
                        {option.description}
                      </p>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {visibility === VisibilityLevel.Public && (
            <div className="flex items-start space-x-3 p-4 rounded-lg bg-muted/50">
              <Checkbox
                id="allowCopying"
                checked={allowCopying}
                onCheckedChange={(checked) => setAllowCopying(checked as boolean)}
              />
              <div className="flex-1 space-y-1">
                <Label
                  htmlFor="allowCopying"
                  className="text-sm font-medium leading-none cursor-pointer"
                >
                  Permitir cópias
                </Label>
                <p className="text-sm text-muted-foreground">
                  Outros usuários poderão clonar este plano para suas próprias contas
                </p>
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="flex-col sm:flex-row gap-2">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={updateVisibilityMutation.isPending}
            className="w-full sm:w-auto"
          >
            Cancelar
          </Button>
          <Button
            onClick={handleSave}
            disabled={updateVisibilityMutation.isPending}
            className="w-full sm:w-auto"
          >
            {updateVisibilityMutation.isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
