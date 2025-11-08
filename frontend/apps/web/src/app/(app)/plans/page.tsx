'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Star, Trash2, Edit, Share2, Send, Settings, AlertTriangle, Clock, RefreshCw } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import { ShareSettingsDialog } from '@/components/workout/share-settings-dialog';
import { ShareWithFriendsDialog } from '@/components/workout/share-with-friends-dialog';
import { RenewPlanDialog } from '@/components/workout/renew-plan-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { WorkoutPlan } from '@gymhero/shared';

export default function PlansPage() {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [selectedPlan, setSelectedPlan] = useState<WorkoutPlan | null>(null);
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [shareWithFriendsDialogOpen, setShareWithFriendsDialogOpen] = useState(false);
  const [renewDialogOpen, setRenewDialogOpen] = useState(false);
  const [renewPlanData, setRenewPlanData] = useState<{ id: string; name: string; isExpired: boolean } | null>(null);

  const { data: plans, isLoading } = useQuery({
    queryKey: ['workout-plans'],
    queryFn: () => api.workoutPlans.getAll(),
  });

  const setActiveMutation = useMutation({
    mutationFn: (id: string) => api.workoutPlans.setActive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Plano ativado!',
        description: 'Este plano agora está ativo para seus treinos.',
      });
    },
  });

  const setInactiveMutation = useMutation({
    mutationFn: (id: string) => api.workoutPlans.setInactive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Plano desativado',
        description: 'O plano foi desativado com sucesso.',
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.workoutPlans.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Plano excluído',
      });
    },
  });

  const handleSetActive = (id: string) => {
    setActiveMutation.mutate(id);
  };

  const handleSetInactive = (id: string) => {
    setInactiveMutation.mutate(id);
  };

  const handleDelete = (id: string) => {
    if (confirm('Tem certeza que deseja excluir este plano?')) {
      deleteMutation.mutate(id);
    }
  };

  const getExpirationStatus = (expirationDate?: string | null) => {
    if (!expirationDate) {
      return null;
    }

    const now = new Date();
    const expiry = new Date(expirationDate);
    const daysUntilExpiry = Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

    if (daysUntilExpiry < 0) {
      return {
        status: 'expired' as const,
        label: 'Expirado',
        variant: 'destructive' as const,
        icon: AlertTriangle,
        daysRemaining: Math.abs(daysUntilExpiry),
        message: `Expirou há ${Math.abs(daysUntilExpiry)} dia(s)`
      };
    } else if (daysUntilExpiry <= 7) {
      return {
        status: 'expiring' as const,
        label: `Expira em ${daysUntilExpiry}d`,
        variant: 'secondary' as const,
        icon: Clock,
        daysRemaining: daysUntilExpiry,
        message: `Expira em ${daysUntilExpiry} dia(s)`
      };
    } else {
      return {
        status: 'active' as const,
        label: `${daysUntilExpiry} dias`,
        variant: 'default' as const,
        icon: Clock,
        daysRemaining: daysUntilExpiry,
        message: `${daysUntilExpiry} dias restantes`
      };
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold">Planos de Treino</h1>
          <p className="text-sm sm:text-base text-muted-foreground">Gerencie seus programas de treinamento</p>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row">
          <Button variant="outline" onClick={() => router.push('/plans/discover')} className="w-full sm:w-auto">
            Descobrir Planos
          </Button>
          <Button onClick={() => router.push('/plans/new')} className="w-full sm:w-auto">
            <Plus className="mr-2 h-4 w-4" />
            Criar Novo Plano
          </Button>
        </div>
      </div>

      {isLoading ? (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">Carregando planos...</p>
          </CardContent>
        </Card>
      ) : plans && plans.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan) => {
            const planData = plan as any;
            const expirationInfo = getExpirationStatus(planData.expirationDate);
            return (
            <Card
              key={plan.id}
              className={`transition-all ${
                plan.isActive ? 'border-primary ring-2 ring-primary/20' : ''
              } ${expirationInfo?.status === 'expired' ? 'border-destructive/50' : ''}`}
            >
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="flex items-center gap-2 flex-wrap">
                      {plan.name}
                      {plan.isActive && <Star className="h-4 w-4 fill-primary text-primary" />}
                      {expirationInfo && (
                        <Badge variant={expirationInfo.variant} className="text-xs">
                          <expirationInfo.icon className="mr-1 h-3 w-3" />
                          {expirationInfo.label}
                        </Badge>
                      )}
                    </CardTitle>
                    <CardDescription>{planData.description || 'Sem descrição'}</CardDescription>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Treinos:</span>
                    <span className="font-medium">{plan.workouts?.length || 0}</span>
                  </div>
                  {planData.duration && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Duração:</span>
                      <span className="font-medium">{planData.duration} semanas</span>
                    </div>
                  )}
                  {planData.goal && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Objetivo:</span>
                      <span className="font-medium">{planData.goal}</span>
                    </div>
                  )}
                  {expirationInfo && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Status:</span>
                      <span className={`font-medium ${
                        expirationInfo.status === 'expired' ? 'text-destructive' :
                        expirationInfo.status === 'expiring' ? 'text-yellow-600 dark:text-yellow-500' :
                        'text-green-600 dark:text-green-500'
                      }`}>
                        {expirationInfo.message}
                      </span>
                    </div>
                  )}
                  {expirationInfo?.status === 'expired' && (
                    <div className="rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
                      <div className="flex items-start gap-2">
                        <AlertTriangle className="h-4 w-4 mt-0.5 flex-shrink-0" />
                        <p>Este plano expirou. Considere renovar ou criar um novo plano.</p>
                      </div>
                    </div>
                  )}
                  {expirationInfo?.status === 'expiring' && expirationInfo.daysRemaining <= 3 && (
                    <div className="rounded-lg bg-yellow-500/10 p-3 text-sm text-yellow-600 dark:text-yellow-500">
                      <div className="flex items-start gap-2">
                        <Clock className="h-4 w-4 mt-0.5 flex-shrink-0" />
                        <p>Seu plano está próximo do fim. Planeje sua continuação em breve!</p>
                      </div>
                    </div>
                  )}

                  <div className="space-y-2 pt-4">
                    {/* Renewal Button - Show for expired or expiring plans */}
                    {expirationInfo && (expirationInfo.status === 'expired' || expirationInfo.status === 'expiring') && (
                      <Button
                        size="sm"
                        onClick={() => {
                          setRenewPlanData({
                            id: plan.id,
                            name: plan.name,
                            isExpired: expirationInfo.status === 'expired'
                          });
                          setRenewDialogOpen(true);
                        }}
                        className="w-full"
                        variant={expirationInfo.status === 'expired' ? 'default' : 'secondary'}
                      >
                        <RefreshCw className="mr-2 h-4 w-4" />
                        {expirationInfo.status === 'expired' ? 'Renovar Plano' : 'Continuar Plano'}
                      </Button>
                    )}

                    <div className="flex gap-2">
                      {plan.isActive ? (
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => handleSetInactive(plan.id)}
                          disabled={setInactiveMutation.isPending}
                          className="flex-1"
                          aria-label="Desativar plano"
                        >
                          <Star className="h-4 w-4 xs:mr-2 fill-current" />
                          <span className="hidden xs:inline">Desativar</span>
                        </Button>
                      ) : (
                        <Button
                          size="sm"
                          onClick={() => handleSetActive(plan.id)}
                          disabled={setActiveMutation.isPending}
                          className="flex-1"
                          aria-label="Ativar plano"
                        >
                          <Star className="h-4 w-4 xs:mr-2" />
                          <span className="hidden xs:inline">Ativar</span>
                        </Button>
                      )}
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleDelete(plan.id)}
                        disabled={plan.isActive || deleteMutation.isPending}
                        className="px-3"
                        aria-label="Excluir plano"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => router.push(`/plans/${plan.id}/edit`)}
                        aria-label="Editar plano"
                        className="flex-1 sm:flex-none"
                      >
                        <Edit className="h-4 w-4 md:mr-2" />
                        <span className="hidden md:inline">Editar</span>
                      </Button>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            size="sm"
                            variant="outline"
                            aria-label="Compartilhar plano"
                            className="flex-1 sm:flex-none"
                          >
                            <Share2 className="h-4 w-4 md:mr-2" />
                            <span className="hidden md:inline">Compartilhar</span>
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedPlan(plan);
                              setShareWithFriendsDialogOpen(true);
                            }}
                          >
                            <Send className="mr-2 h-4 w-4" />
                            Enviar para Amigos
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedPlan(plan);
                              setShareDialogOpen(true);
                            }}
                          >
                            <Settings className="mr-2 h-4 w-4" />
                            Configurações de Compartilhamento
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
            );
          })}
        </div>
      ) : (
        <Card>
          <CardContent className="py-12 text-center">
            <div className="mx-auto max-w-md space-y-4">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-muted">
                <Plus className="h-10 w-10 text-muted-foreground" />
              </div>
              <h3 className="text-xl font-semibold">Nenhum plano de treino</h3>
              <p className="text-muted-foreground">
                Crie seu primeiro plano de treino para começar a organizar seus exercícios.
              </p>
              <Button onClick={() => router.push('/plans/new')}>
                <Plus className="mr-2 h-4 w-4" />
                Criar Primeiro Plano
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {selectedPlan && (
        <>
          <ShareSettingsDialog
            planId={selectedPlan.id}
            planName={selectedPlan.name}
            currentVisibility={(selectedPlan as any).visibilityLevel ?? 0}
            currentAllowCopying={(selectedPlan as any).allowCopying ?? true}
            open={shareDialogOpen}
            onOpenChange={setShareDialogOpen}
          />
          <ShareWithFriendsDialog
            planId={selectedPlan.id}
            planName={selectedPlan.name}
            open={shareWithFriendsDialogOpen}
            onOpenChange={setShareWithFriendsDialogOpen}
          />
        </>
      )}

      {renewPlanData && (
        <RenewPlanDialog
          planId={renewPlanData.id}
          planName={renewPlanData.name}
          isExpired={renewPlanData.isExpired}
          open={renewDialogOpen}
          onOpenChange={setRenewDialogOpen}
        />
      )}
    </div>
  );
}
