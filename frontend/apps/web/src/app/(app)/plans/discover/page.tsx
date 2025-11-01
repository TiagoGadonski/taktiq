'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Search, Eye, Copy, ChevronLeft, ChevronRight } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export default function DiscoverPlansPage() {
  const router = useRouter();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['public-workout-plans', page, search],
    queryFn: () =>
      api.workoutPlans.getPublicPlans({
        page,
        pageSize: 12,
        search: search || undefined,
      }),
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearch(searchInput);
    setPage(1);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold">Descobrir Planos</h1>
          <p className="text-sm sm:text-base text-muted-foreground">
            Explore planos de treino públicos criados pela comunidade
          </p>
        </div>
        <Button variant="outline" onClick={() => router.push('/plans')} className="w-full sm:w-auto">
          Meus Planos
        </Button>
      </div>

      <form onSubmit={handleSearch} className="flex flex-col gap-2 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar planos..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="pl-10"
          />
        </div>
        <Button type="submit" className="w-full sm:w-auto">Buscar</Button>
      </form>

      {isLoading ? (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">Carregando planos...</p>
          </CardContent>
        </Card>
      ) : data && data.plans.length > 0 ? (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {data.plans.map((plan: any) => (
              <Card key={plan.id} className="hover:border-primary/50 transition-colors">
                <CardHeader>
                  <CardTitle>{plan.name}</CardTitle>
                  <CardDescription className="line-clamp-2">
                    {plan.description || 'Sem descrição'}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Criador:</span>
                      <span className="font-medium">{plan.creatorName}</span>
                    </div>
                    {plan.goal && (
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">Objetivo:</span>
                        <span className="font-medium">{plan.goal}</span>
                      </div>
                    )}
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Treinos:</span>
                      <span className="font-medium">{plan.workoutCount}</span>
                    </div>
                    {plan.duration && (
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">Duração:</span>
                        <span className="font-medium">{plan.duration} semanas</span>
                      </div>
                    )}
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Visualizações:</span>
                      <span className="font-medium">{plan.viewCount}</span>
                    </div>

                    <div className="grid grid-cols-2 gap-2 pt-4">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => router.push(`/plans/public/${plan.id}`)}
                      >
                        <Eye className="mr-1 h-3 w-3" />
                        <span className="hidden xs:inline">Ver</span>
                      </Button>
                      {plan.allowCopying && (
                        <Button
                          size="sm"
                          onClick={() => {
                            // TODO: Implement clone functionality
                          }}
                        >
                          <Copy className="mr-1 h-3 w-3" />
                          <span className="hidden xs:inline">Copiar</span>
                        </Button>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {data.totalPages > 1 && (
            <div className="flex flex-col gap-3 items-center sm:flex-row sm:justify-center">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="w-full sm:w-auto"
              >
                <ChevronLeft className="h-4 w-4" />
                <span className="ml-1">Anterior</span>
              </Button>
              <span className="text-sm text-muted-foreground whitespace-nowrap">
                Página {page} de {data.totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="w-full sm:w-auto"
              >
                <span className="mr-1">Próxima</span>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          )}
        </>
      ) : (
        <Card>
          <CardContent className="py-12 text-center">
            <div className="mx-auto max-w-md space-y-4">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-muted">
                <Search className="h-10 w-10 text-muted-foreground" />
              </div>
              <h3 className="text-xl font-semibold">Nenhum plano encontrado</h3>
              <p className="text-muted-foreground">
                {search
                  ? 'Tente buscar com palavras diferentes'
                  : 'Ainda não há planos públicos disponíveis'}
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
