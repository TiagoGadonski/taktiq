'use client';

import { useRouter, useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, Copy, Eye, User, Calendar, Target, Dumbbell, Edit } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/hooks/use-auth';

export default function PlanDetailPage() {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const { user } = useAuth();
  const planId = params?.id as string;

  const { data: plan, isLoading } = useQuery({
    queryKey: ['workout-plan', planId],
    queryFn: async () => {
      return await api.workoutPlans.getById(planId);
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
            Este plano não existe ou você não tem permissão para visualizá-lo.
          </p>
          <Button onClick={() => router.push('/plans')}>
            Voltar para meus planos
          </Button>
        </CardContent>
      </Card>
    );
  }

  const isOwnPlan = (plan as any).ownerId === user?.id || (plan as any).creatorId === user?.id;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.back()}
          className="w-fit hover-lift tap-scale"
        >
          <ChevronLeft className="h-4 w-4 mr-1" />
          Voltar
        </Button>
        <div className="flex gap-2">
          {isOwnPlan && (
            <Button onClick={() => router.push(`/plans/${planId}/edit`)} className="w-full sm:w-auto hover-lift tap-scale">
              <Edit className="mr-2 h-4 w-4" />
              Editar Plano
            </Button>
          )}
          {!isOwnPlan && plan.allowCopying && (
            <Button onClick={handleCopyPlan} className="w-full sm:w-auto hover-lift tap-scale">
              <Copy className="mr-2 h-4 w-4" />
              Copiar para Meus Planos
            </Button>
          )}
        </div>
      </div>

      {/* Plan Info Card */}
      <Card className="glass border-primary/20 hover-lift">
        <CardHeader>
          <div className="flex items-start justify-between">
            <div className="space-y-2 flex-1">
              <div className="flex items-center gap-2 flex-wrap">
                <CardTitle className="text-2xl sm:text-3xl bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
                  {plan.name}
                </CardTitle>
                {isOwnPlan && (
                  <Badge variant="outline" className="bg-primary/10 text-primary border-primary/30">
                    Meu Plano
                  </Badge>
                )}
                {!isOwnPlan && (
                  <Badge variant="outline" className="bg-blue-500/10 text-blue-500 border-blue-500/30">
                    Adquirido
                  </Badge>
                )}
              </div>
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
            <div className="flex items-center gap-3 p-3 glass rounded-lg border border-primary/10">
              <User className="h-5 w-5 text-primary" />
              <div>
                <p className="text-sm text-muted-foreground">Criador</p>
                <p className="font-medium">{plan.creatorName || (plan as any).ownerName}</p>
              </div>
            </div>
            {plan.goal && (
              <div className="flex items-center gap-3 p-3 glass rounded-lg border border-primary/10">
                <Target className="h-5 w-5 text-primary" />
                <div>
                  <p className="text-sm text-muted-foreground">Objetivo</p>
                  <p className="font-medium">{plan.goal}</p>
                </div>
              </div>
            )}
            {plan.duration && (
              <div className="flex items-center gap-3 p-3 glass rounded-lg border border-primary/10">
                <Calendar className="h-5 w-5 text-primary" />
                <div>
                  <p className="text-sm text-muted-foreground">Duração</p>
                  <p className="font-medium">{plan.duration} semanas</p>
                </div>
              </div>
            )}
            {plan.viewCount !== undefined && (
              <div className="flex items-center gap-3 p-3 glass rounded-lg border border-primary/10">
                <Eye className="h-5 w-5 text-primary" />
                <div>
                  <p className="text-sm text-muted-foreground">Visualizações</p>
                  <p className="font-medium">{plan.viewCount}</p>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Workouts */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold flex items-center gap-2">
          <Dumbbell className="h-6 w-6 text-primary" />
          Treinos ({plan.workouts?.length || 0})
        </h2>

        {plan.workouts && plan.workouts.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {plan.workouts.map((workout, index) => (
              <Card key={workout.id} className="glass border-primary/20 hover-lift tap-scale animate-scale-in" style={{ animationDelay: `${index * 50}ms` }}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <CardTitle className="text-lg">{workout.name}</CardTitle>
                      {(workout as any).description && (
                        <CardDescription className="mt-1 line-clamp-2">
                          {(workout as any).description}
                        </CardDescription>
                      )}
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {/* Workout metadata */}
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <Dumbbell className="h-4 w-4" />
                      <span>{workout.exercises?.length || 0} exercícios</span>
                    </div>

                    {/* Exercises list */}
                    {workout.exercises && workout.exercises.length > 0 && (
                      <div className="space-y-1 pt-2 border-t border-border/50">
                        <p className="text-xs font-medium text-muted-foreground mb-2">Exercícios:</p>
                        {workout.exercises.map((exercise, idx) => (
                          <div key={exercise.id} className="text-sm pl-2 border-l-2 border-primary/30">
                            <p className="font-medium">{idx + 1}. {exercise.exercise?.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {exercise.targetSets} séries × {exercise.targetReps} reps
                              {exercise.targetLoad && ` @ ${exercise.targetLoad}kg`}
                            </p>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <Card className="glass border-primary/20">
            <CardContent className="py-12 text-center">
              <Dumbbell className="h-12 w-12 text-muted-foreground mx-auto mb-4 opacity-50" />
              <p className="text-muted-foreground">
                Este plano ainda não possui treinos.
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
