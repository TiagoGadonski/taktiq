'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Star, Trash2, Edit } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import type { WorkoutPlan } from '@gymhero/shared';

export default function PlansPage() {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [selectedPlan, setSelectedPlan] = useState<WorkoutPlan | null>(null);

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

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Planos de Treino</h1>
          <p className="text-muted-foreground">Gerencie seus programas de treinamento</p>
        </div>
        <Button onClick={() => router.push('/plans/new')}>
          <Plus className="mr-2 h-4 w-4" />
          Criar Novo Plano
        </Button>
      </div>

      {isLoading ? (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">Carregando planos...</p>
          </CardContent>
        </Card>
      ) : plans && plans.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan) => (
            <Card
              key={plan.id}
              className={`transition-all ${
                plan.isActive ? 'border-primary ring-2 ring-primary/20' : ''
              }`}
            >
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="flex items-center gap-2">
                      {plan.name}
                      {plan.isActive && <Star className="h-4 w-4 fill-primary text-primary" />}
                    </CardTitle>
                    <CardDescription>{plan.description || 'Sem descrição'}</CardDescription>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Treinos:</span>
                    <span className="font-medium">{plan.workouts?.length || 0}</span>
                  </div>
                  {plan.duration && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Duração:</span>
                      <span className="font-medium">{plan.duration} semanas</span>
                    </div>
                  )}
                  {plan.goal && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Objetivo:</span>
                      <span className="font-medium">{plan.goal}</span>
                    </div>
                  )}

                  <div className="flex gap-2 pt-4">
                    {plan.isActive ? (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleSetInactive(plan.id)}
                        disabled={setInactiveMutation.isPending}
                        className="flex-1"
                      >
                        <Star className="mr-1 h-3 w-3 fill-current" />
                        Desativar
                      </Button>
                    ) : (
                      <Button
                        size="sm"
                        onClick={() => handleSetActive(plan.id)}
                        disabled={setActiveMutation.isPending}
                        className="flex-1"
                      >
                        <Star className="mr-1 h-3 w-3" />
                        Ativar
                      </Button>
                    )}
                    <Button
                      size="sm"
                      variant="outline"
                      className="flex-1"
                      onClick={() => router.push(`/plans/${plan.id}/edit`)}
                    >
                      <Edit className="mr-1 h-3 w-3" />
                      Editar
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleDelete(plan.id)}
                      disabled={plan.isActive || deleteMutation.isPending}
                    >
                      <Trash2 className="h-3 w-3" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
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
    </div>
  );
}
