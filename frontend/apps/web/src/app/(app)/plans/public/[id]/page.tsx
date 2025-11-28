'use client';

import { useRouter, useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, Copy, Eye, User, Calendar, Target, Dumbbell } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/hooks/use-auth';
import { PlanComments } from '@/components/plans/plan-comments';

export default function PublicPlanDetailPage() {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const { user } = useAuth();
  const planId = params?.id as string;

  const { data: plan, isLoading } = useQuery({
    queryKey: ['public-workout-plan', planId],
    queryFn: async () => {
      return await api.workoutPlans.getPublicPlanById(planId);
    },
    enabled: !!planId,
  });

  const handleCopyPlan = async () => {
    try {
      await api.workoutPlans.clone(planId);
      toast({
        title: 'Plano copiado!',
        description: 'O plano foi adicionado aos seus planos de treino.',
      });
      router.push('/plans');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao copiar plano',
        description: error.message || 'Não foi possível copiar o plano.',
      });
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
          <p className="text-muted-foreground">Carregando plano...</p>
        </div>
      </div>
    );
  }

  if (!plan) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <h3 className="text-xl font-semibold mb-2">Plano não encontrado</h3>
          <p className="text-muted-foreground mb-4">
            Este plano não existe ou não está mais disponível publicamente.
          </p>
          <Button onClick={() => router.push('/plans/discover')}>
            Voltar para descobrir planos
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="w-fit"
        >
          <ChevronLeft className="h-4 w-4 mr-1" />
          Voltar
        </Button>
        {plan.allowCopying && (
          <Button onClick={handleCopyPlan} className="w-full sm:w-auto">
            <Copy className="mr-2 h-4 w-4" />
            Copiar para Meus Planos
          </Button>
        )}
      </div>

      {/* Plan Info Card */}
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between">
            <div className="space-y-2 flex-1">
              <CardTitle className="text-2xl sm:text-3xl">{plan.name}</CardTitle>
              {plan.description && (
                <CardDescription className="text-base">
                  {plan.description}
                </CardDescription>
              )}
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Metadata */}
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg">
              <User className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Criador</p>
                <p className="font-medium">{plan.creatorName}</p>
              </div>
            </div>
            {plan.goal && (
              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg">
                <Target className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm text-muted-foreground">Objetivo</p>
                  <p className="font-medium">{plan.goal}</p>
                </div>
              </div>
            )}
            {plan.duration && (
              <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg">
                <Calendar className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="text-sm text-muted-foreground">Duração</p>
                  <p className="font-medium">{plan.duration} semanas</p>
                </div>
              </div>
            )}
            <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg">
              <Eye className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Visualizações</p>
                <p className="font-medium">{plan.viewCount}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Workouts */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold">
          Treinos ({plan.workouts?.length || 0})
        </h2>
        {plan.workouts && plan.workouts.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2">
            {plan.workouts.map((workout: any) => (
              <Card key={workout.id}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <CardTitle className="text-lg">{workout.name}</CardTitle>
                      {workout.dayOfWeek !== null && workout.dayOfWeek !== undefined && (
                        <CardDescription>
                          {['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'][workout.dayOfWeek]}
                        </CardDescription>
                      )}
                    </div>
                    <Badge variant="secondary">
                      <Dumbbell className="h-3 w-3 mr-1" />
                      {workout.exercises?.length || 0} exercícios
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  {workout.exercises && workout.exercises.length > 0 ? (
                    <div className="space-y-2">
                      {workout.exercises.map((exercise: any, index: number) => (
                        <div
                          key={exercise.id || index}
                          className="flex items-center justify-between py-2 px-3 bg-muted/50 rounded-md text-sm"
                        >
                          <div className="flex items-center gap-2">
                            <span className="font-medium text-muted-foreground">
                              {index + 1}.
                            </span>
                            <span>{exercise.exerciseName || exercise.name}</span>
                          </div>
                          <span className="text-muted-foreground">
                            {exercise.targetSets}x{exercise.targetReps}
                          </span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground text-center py-4">
                      Nenhum exercício configurado
                    </p>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-muted-foreground">Este plano ainda não possui treinos configurados.</p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Comments Section */}
      <div className="mt-8">
        <PlanComments planId={planId} currentUserId={user?.id} />
      </div>
    </div>
  );
}
