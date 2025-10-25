'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Calendar, Search, Filter, ChevronRight } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { formatDate, formatDuration } from '@gymhero/shared';

export default function HistoryPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

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
    return (
      session.workout?.name?.toLowerCase().includes(search.toLowerCase()) ||
      session.notes?.toLowerCase().includes(search.toLowerCase())
    );
  });

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
          filteredSessions.map((session) => (
            <Card
              key={session.id}
              className="cursor-pointer transition-colors hover:border-primary"
            >
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3">
                      <Calendar className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h3 className="font-semibold">
                          {session.workout?.name || 'Treino Livre'}
                        </h3>
                        <p className="text-sm text-muted-foreground">
                          {formatDate(session.startedAt, 'pt-BR')}
                          {session.completedAt &&
                            ` • ${formatDuration(session.duration || 0)}`}
                        </p>
                      </div>
                    </div>

                    <div className="mt-4 flex gap-6 text-sm">
                      <div>
                        <span className="text-muted-foreground">Séries: </span>
                        <span className="font-medium">{session.sets?.length || 0}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Volume: </span>
                        <span className="font-medium">
                          {session.sets
                            ?.reduce((acc, set) => acc + (set.weight || 0) * set.reps, 0)
                            .toFixed(0) || 0}{' '}
                          kg
                        </span>
                      </div>
                      {session.notes && (
                        <div className="flex-1">
                          <span className="text-muted-foreground">Notas: </span>
                          <span className="italic">{session.notes}</span>
                        </div>
                      )}
                    </div>
                  </div>
                  <ChevronRight className="h-5 w-5 text-muted-foreground" />
                </div>
              </CardContent>
            </Card>
          ))
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
    </div>
  );
}
