'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import {
  FileText,
  Dumbbell,
  TrendingUp,
  Sparkles,
  ExternalLink,
  Calendar,
  Activity,
} from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { apiClient } from '@/lib/api';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { cn } from '@/lib/utils';

interface ClientSlideOverProps {
  client: any;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ClientSlideOver({ client, open, onOpenChange }: ClientSlideOverProps) {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [notes, setNotes] = useState(client?.ptNotes || '');
  const [saveTimeout, setSaveTimeout] = useState<NodeJS.Timeout | null>(null);

  // Auto-save mutation
  const saveNotesMutation = useMutation({
    mutationFn: async (newNotes: string) => {
      await apiClient.put(`/personal/clients/${client.id}/notes`, {
        notes: newNotes,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
      toast({
        title: 'Anotações salvas',
        description: 'Suas anotações foram salvas automaticamente.',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao salvar',
        description: 'Não foi possível salvar as anotações.',
        variant: 'destructive',
      });
    },
  });

  // Auto-save with debounce
  const handleNotesChange = (value: string) => {
    setNotes(value);

    // Clear existing timeout
    if (saveTimeout) {
      clearTimeout(saveTimeout);
    }

    // Set new timeout
    const timeout = setTimeout(() => {
      saveNotesMutation.mutate(value);
    }, 500);

    setSaveTimeout(timeout);
  };

  const getInitials = (name: string) => {
    return name
      ?.split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const getStatusColor = (planCount: number) => {
    if (planCount === 0) return 'text-red-600 bg-red-50';
    if (planCount === 1) return 'text-yellow-600 bg-yellow-50';
    return 'text-green-600 bg-green-50';
  };

  if (!client) return null;

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="sm:max-w-md overflow-y-auto">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarImage src={client.profilePicture} alt={client.name} />
              <AvatarFallback>{getInitials(client.name)}</AvatarFallback>
            </Avatar>
            <div className="text-left">
              <div className="font-semibold text-lg">{client.name}</div>
              <div className="text-sm text-muted-foreground font-normal">
                {client.email}
              </div>
            </div>
          </SheetTitle>
          <SheetDescription className="text-left">
            Cliente desde: {new Date(client.createdAt).toLocaleDateString('pt-BR')}
            <br />
            Último treino: {client.lastWorkout || 'Nenhum treino registrado'}
          </SheetDescription>
        </SheetHeader>

        <div className="space-y-6 mt-6">
          {/* Quick Stats */}
          <div className="grid grid-cols-3 gap-3">
            <div className="text-center p-3 bg-gray-50 rounded-lg">
              <div className="text-2xl font-bold">{client.planCount || 0}</div>
              <div className="text-xs text-muted-foreground">Planos</div>
            </div>
            <div className="text-center p-3 bg-gray-50 rounded-lg">
              <div className="text-2xl font-bold">{client.workoutCount || 0}</div>
              <div className="text-xs text-muted-foreground">Treinos</div>
            </div>
            <div className="text-center p-3 bg-gray-50 rounded-lg">
              <div className="text-2xl font-bold">{client.frequency || 0}%</div>
              <div className="text-xs text-muted-foreground">Freq.</div>
            </div>
          </div>

          {/* Quick Actions */}
          <div>
            <h3 className="text-sm font-semibold mb-3">Ações Rápidas</h3>
            <div className="grid grid-cols-2 gap-2">
              <Button
                variant="outline"
                size="sm"
                className="justify-start"
                onClick={() => {
                  router.push(`/instructor/clients/${client.id}/assessments/new`);
                  onOpenChange(false);
                }}
              >
                <FileText className="h-4 w-4 mr-2" />
                Nova Avaliação
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="justify-start"
                onClick={() => {
                  router.push(`/plans/new?clientId=${client.id}`);
                  onOpenChange(false);
                }}
              >
                <Dumbbell className="h-4 w-4 mr-2" />
                Criar Plano
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="justify-start bg-gradient-to-r from-purple-50 to-blue-50"
                onClick={() => {
                  router.push(`/instructor/clients/${client.id}/ai-assessment`);
                  onOpenChange(false);
                }}
              >
                <Sparkles className="h-4 w-4 mr-2" />
                Avaliação IA
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="justify-start"
                onClick={() => {
                  router.push(`/instructor/clients/${client.id}/evolution`);
                  onOpenChange(false);
                }}
              >
                <TrendingUp className="h-4 w-4 mr-2" />
                Ver Evolução
              </Button>
            </div>
          </div>

          <Separator />

          {/* PT Notes */}
          <div>
            <Label htmlFor="pt-notes" className="text-sm font-semibold">
              📋 Anotações do PT (privado)
            </Label>
            <p className="text-xs text-muted-foreground mb-2">
              Suas anotações são salvas automaticamente
            </p>
            <Textarea
              id="pt-notes"
              placeholder="Digite suas observações sobre o cliente..."
              value={notes}
              onChange={(e) => handleNotesChange(e.target.value)}
              rows={4}
              className="resize-none"
            />
            {saveNotesMutation.isPending && (
              <p className="text-xs text-muted-foreground mt-1">Salvando...</p>
            )}
          </div>

          <Separator />

          {/* Latest Assessment */}
          <div>
            <h3 className="text-sm font-semibold mb-3 flex items-center gap-2">
              <Activity className="h-4 w-4" />
              Última Avaliação
            </h3>
            {client.latestAssessment ? (
              <div className="bg-gray-50 rounded-lg p-3 space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-medium text-sm">
                    {client.latestAssessment.type}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {new Date(client.latestAssessment.date).toLocaleDateString('pt-BR')}
                  </span>
                </div>
                <p className="text-sm text-muted-foreground line-clamp-2">
                  {client.latestAssessment.summary}
                </p>
                <Button
                  variant="link"
                  size="sm"
                  className="p-0 h-auto text-xs"
                  onClick={() => {
                    router.push(`/instructor/clients/${client.id}/assessments`);
                    onOpenChange(false);
                  }}
                >
                  Ver Todas as Avaliações →
                </Button>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                Nenhuma avaliação registrada
              </p>
            )}
          </div>

          <Separator />

          {/* Active Plans */}
          <div>
            <h3 className="text-sm font-semibold mb-3 flex items-center gap-2">
              <Dumbbell className="h-4 w-4" />
              Planos Ativos ({client.planCount || 0})
            </h3>
            {client.activePlans && client.activePlans.length > 0 ? (
              <div className="space-y-2">
                {client.activePlans.slice(0, 3).map((plan: any) => (
                  <div
                    key={plan.id}
                    className="bg-gray-50 rounded-lg p-3 flex items-center justify-between"
                  >
                    <div>
                      <div className="font-medium text-sm">{plan.name}</div>
                      <div className="text-xs text-muted-foreground">
                        {plan.workoutCount || 0} treinos
                      </div>
                    </div>
                    <Badge variant="secondary" className={cn(getStatusColor(1))}>
                      Ativo
                    </Badge>
                  </div>
                ))}
                <Button
                  variant="link"
                  size="sm"
                  className="p-0 h-auto text-xs"
                  onClick={() => {
                    router.push(`/instructor/clients/${client.id}`);
                    onOpenChange(false);
                  }}
                >
                  Gerenciar Planos →
                </Button>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                Nenhum plano ativo
              </p>
            )}
          </div>

          <Separator />

          {/* View Full Profile */}
          <Button
            className="w-full"
            onClick={() => {
              router.push(`/instructor/clients/${client.id}`);
              onOpenChange(false);
            }}
          >
            Ver Perfil Completo
            <ExternalLink className="h-4 w-4 ml-2" />
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
