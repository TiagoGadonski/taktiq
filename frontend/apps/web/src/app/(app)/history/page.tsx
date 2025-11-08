'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Calendar, Search, Filter, ChevronRight, X, Dumbbell, Plus } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { formatDate, formatDuration } from '@gymhero/shared';
import type { WorkoutSession, WorkoutSet } from '@gymhero/shared';

export default function HistoryPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedSession, setSelectedSession] = useState<WorkoutSession | null>(null);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);

  const { data: sessions, isLoading } = useQuery({
    queryKey: ['sessions', 'history', page, startDate, endDate],
    queryFn: () =>
      api.sessions.getHistory({
        page,
        pageSize: 10,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      }),
  });

  const filteredSessions = sessions?.data.filter((session) => {
    if (!search) return true;
    const sessionData = session as any;
    return (
      sessionData.workout?.name?.toLowerCase().includes(search.toLowerCase()) ||
      sessionData.notes?.toLowerCase().includes(search.toLowerCase())
    );
  });

  const handleSessionClick = (session: WorkoutSession) => {
    setSelectedSession(session);
    setIsDetailDialogOpen(true);
  };

  // Group sets by exercise for the selected session
  const groupedExercises = selectedSession?.sets?.reduce((acc, set) => {
    const exerciseName = set.exercise?.name || 'Exercício desconhecido';
    if (!acc[exerciseName]) {
      acc[exerciseName] = {
        exercise: set.exercise,
        sets: [],
      };
    }
    acc[exerciseName].sets.push(set);
    return acc;
  }, {} as Record<string, { exercise: any; sets: WorkoutSet[] }>);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Histórico de Treinos</h1>
        <p className="text-muted-foreground">Acompanhe todos os seus treinos anteriores</p>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid gap-4 md:grid-cols-3">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar treinos..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-9"
              />
            </div>
            <div>
              <Input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                placeholder="Data inicial"
              />
            </div>
            <div>
              <Input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                placeholder="Data final"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Session Stats Summary */}
      {sessions && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-medium">Total de Treinos</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">{sessions.totalCount}</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-medium">Séries Totais</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">
                {sessions.data.reduce((acc, s) => acc + (s.sets?.length || 0), 0)}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-sm font-medium">Volume Total</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">
                {sessions.data
                  .reduce((acc, s) => {
                    const volume = s.sets?.reduce(
                      (v, set) => v + (set.weight || 0) * set.reps,
                      0
                    );
                    return acc + (volume || 0);
                  }, 0)
                  .toFixed(0)}{' '}
                kg
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Sessions List */}
      <div className="space-y-4">
        {isLoading ? (
          <Card>
            <CardContent className="py-8 text-center">
              <p className="text-muted-foreground">Carregando...</p>
            </CardContent>
          </Card>
        ) : filteredSessions && filteredSessions.length > 0 ? (
          filteredSessions.map((session) => {
            const sessionData = session as any;
            return (
            <Card
              key={session.id}
              className="cursor-pointer transition-colors hover:border-primary"
              onClick={() => handleSessionClick(session)}
            >
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3">
                      <Calendar className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h3 className="font-semibold">
                          {sessionData.workout?.name || 'Treino Livre'}
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          {formatDate(session.startedAt, 'pt-BR')}
                          {sessionData.completedAt &&
                            ` • ${formatDuration(sessionData.duration || 0)}`}
                        </p>
                      </div>
                    </div>

                    <div className="mt-4 flex gap-6 text-sm">
                      <div>
                        <span className="text-muted-foreground">Séries: </span>
                        <span className="font-medium">{sessionData.sets?.length || 0}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Volume: </span>
                        <span className="font-medium">
                          {sessionData.sets
                            ?.reduce((acc: number, set: any) => acc + (set.weight || 0) * set.reps, 0)
                            .toFixed(0) || 0}{' '}
                          kg
                        </span>
                      </div>
                      {sessionData.notes && (
                        <div className="flex-1">
                          <span className="text-muted-foreground">Notas: </span>
                          <span className="italic">{sessionData.notes}</span>
                        </div>
                      )}
                    </div>
                  </div>
                  <ChevronRight className="h-5 w-5 text-muted-foreground" />
                </div>
              </CardContent>
            </Card>
          );
          })
        ) : (
          <Card>
            <CardContent className="py-8 text-center">
              <p className="text-muted-foreground">
                {search || startDate || endDate
                  ? 'Nenhum treino encontrado com os filtros aplicados'
                  : 'Você ainda não tem treinos registrados'}
              </p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Pagination */}
      {sessions && sessions.totalPages > 1 && (
        <div className="flex justify-center gap-2">
          <Button
            variant="outline"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
          >
            Anterior
          </Button>
          <div className="flex items-center px-4">
            Página {page} de {sessions.totalPages}
          </div>
          <Button
            variant="outline"
            onClick={() => setPage((p) => Math.min(sessions.totalPages, p + 1))}
            disabled={page === sessions.totalPages}
          >
            Próxima
          </Button>
        </div>
      )}

      {/* Session Detail Dialog */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="text-2xl">
              {(selectedSession as any)?.workout?.name || 'Treino Livre'}
            </DialogTitle>
            <DialogDescription>
              {selectedSession && (() => {
                const sessionData = selectedSession as any;
                return (
                <div className="flex items-center gap-4 mt-2 text-sm">
                  <div className="flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    {formatDate(selectedSession.startedAt, 'pt-BR')}
                  </div>
                  {sessionData.completedAt && (
                    <div>Duração: {formatDuration(sessionData.duration || 0)}</div>
                  )}
                </div>
                );
              })()}
            </DialogDescription>
          </DialogHeader>

          {selectedSession && (() => {
            const sessionData = selectedSession as any;
            return (
            <div className="space-y-6 mt-4">
              {/* Session Stats */}
              <div className="grid grid-cols-3 gap-4">
                <Card>
                  <CardContent className="pt-6 text-center">
                    <p className="text-2xl font-bold text-primary">
                      {sessionData.sets?.length || 0}
                    </p>
                    <p className="text-sm text-muted-foreground mt-1">Séries Totais</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="pt-6 text-center">
                    <p className="text-2xl font-bold text-primary">
                      {Object.keys(groupedExercises || {}).length}
                    </p>
                    <p className="text-sm text-muted-foreground mt-1">Exercícios</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="pt-6 text-center">
                    <p className="text-2xl font-bold text-primary">
                      {sessionData.sets
                        ?.reduce((acc: number, set: any) => acc + (set.weight || 0) * set.reps, 0)
                        .toFixed(0) || 0}{' '}
                      kg
                    </p>
                    <p className="text-sm text-muted-foreground mt-1">Volume Total</p>
                  </CardContent>
                </Card>
              </div>

              {/* Exercises and Sets */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold">Exercícios Realizados</h3>
                {groupedExercises &&
                  Object.entries(groupedExercises).map(([exerciseName, { exercise, sets }]) => {
                    // Check if this exercise was added during the session
                    const wasAdded = sets.some(set => set.isAddedDuringSession);

                    return (
                    <Card key={exerciseName}>
                      <CardHeader className="pb-3">
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <Dumbbell className="h-5 w-5 text-primary" />
                            <div>
                              <div className="flex items-center gap-2">
                                <CardTitle className="text-base">{exerciseName}</CardTitle>
                                {wasAdded && (
                                  <Badge variant="secondary" className="bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300">
                                    <Plus className="mr-1 h-3 w-3" />
                                    Adicionado
                                  </Badge>
                                )}
                              </div>
                              {exercise?.muscleGroup && (
                                <p className="text-sm text-muted-foreground capitalize">
                                  {exercise.muscleGroup}
                                </p>
                              )}
                            </div>
                          </div>
                        </div>
                      </CardHeader>
                      <CardContent>
                        <div className="space-y-2">
                          {sets.map((set, index) => (
                            <div
                              key={set.id}
                              className="flex items-center justify-between bg-muted/50 rounded-md px-3 py-2 text-sm"
                            >
                              <div className="flex items-center gap-4">
                                <span className="font-medium text-primary">Série {index + 1}</span>
                                <span>{set.reps} reps</span>
                                {set.weight && (
                                  <span className="font-medium">{set.weight} kg</span>
                                )}
                                {set.weight && (
                                  <span className="text-muted-foreground">
                                    • Volume: {(set.weight * set.reps).toFixed(0)} kg
                                  </span>
                                )}
                              </div>
                              {set.isPr && (
                                <span className="text-xs font-semibold text-yellow-600 bg-yellow-100 dark:bg-yellow-900/30 px-2 py-1 rounded">
                                  PR
                                </span>
                              )}
                            </div>
                          ))}
                          <div className="mt-2 pt-2 border-t">
                            <div className="flex items-center justify-between text-sm font-medium">
                              <span>Total do exercício:</span>
                              <div className="flex items-center gap-4">
                                <span>{sets.length} séries</span>
                                <span>
                                  {sets.reduce((acc, set) => acc + (set.weight || 0) * set.reps, 0).toFixed(0)} kg
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                    );
                  })}
              </div>

              {/* Session Notes */}
              {sessionData.notes && (
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">Notas do Treino</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="text-sm whitespace-pre-wrap">{sessionData.notes}</p>
                  </CardContent>
                </Card>
              )}
            </div>
            );
          })()}
        </DialogContent>
      </Dialog>
    </div>
  );
}
