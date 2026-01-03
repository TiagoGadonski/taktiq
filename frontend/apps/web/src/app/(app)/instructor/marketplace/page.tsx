'use client';

import { useState, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Plus,
  ShoppingBag,
  Eye,
  DollarSign,
  Edit,
  FileText,
  TrendingUp,
} from 'lucide-react';
import { apiClient } from '@/lib/api';
import { cn } from '@/lib/utils';

type PlanFilter = 'all' | 'for-sale' | 'templates' | 'student-plans';

interface WorkoutPlan {
  id: string;
  name: string;
  description?: string;
  price?: number;
  salesCount: number;
  viewsCount: number;
  isForSale: boolean;
  isTemplate: boolean;
  assignedToStudentId?: string;
  createdAt: string;
}

interface BlogPost {
  id: string;
  title: string;
  views: number;
  isDraft: boolean;
  createdAt: string;
}

export default function MarketplacePage() {
  const router = useRouter();
  const [activeFilter, setActiveFilter] = useState<PlanFilter>('all');

  // Fetch workout plans
  const { data: plans, isLoading: plansLoading } = useQuery({
    queryKey: ['marketplace-plans'],
    queryFn: async () => {
      try {
        const response = await apiClient.get<any[]>('/workout-plans/my-plans');
        return (response || []).map((plan: any) => ({
          id: plan.id,
          name: plan.name,
          description: plan.description,
          price: plan.price,
          salesCount: plan.salesCount || 0,
          viewsCount: plan.viewsCount || 0,
          isForSale: plan.isForSale || false,
          isTemplate: plan.isTemplate || false,
          assignedToStudentId: plan.assignedToStudentId,
          createdAt: plan.createdAt,
        })) as WorkoutPlan[];
      } catch (error) {
        console.error('Failed to fetch plans:', error);
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Fetch blog posts (placeholder - implement when API ready)
  const { data: posts } = useQuery({
    queryKey: ['blog-posts'],
    queryFn: async () => {
      // TODO: Replace with real API when available
      return [] as BlogPost[];
    },
    staleTime: 5 * 60 * 1000,
  });

  // Calculate stats
  const stats = useMemo(() => {
    if (!plans) return { total: 0, forSale: 0, totalViews: 0 };

    return {
      total: plans.length,
      forSale: plans.filter((p) => p.isForSale).length,
      totalViews: plans.reduce((sum, p) => sum + p.viewsCount, 0),
    };
  }, [plans]);

  // Filter plans
  const filteredPlans = useMemo(() => {
    if (!plans) return [];

    switch (activeFilter) {
      case 'for-sale':
        return plans.filter((p) => p.isForSale);
      case 'templates':
        return plans.filter((p) => p.isTemplate || !p.assignedToStudentId);
      case 'student-plans':
        return plans.filter((p) => p.assignedToStudentId);
      default:
        return plans;
    }
  }, [plans, activeFilter]);

  // Count plans by filter
  const filterCounts = useMemo(() => {
    if (!plans) return { all: 0, forSale: 0, templates: 0, studentPlans: 0 };

    return {
      all: plans.length,
      forSale: plans.filter((p) => p.isForSale).length,
      templates: plans.filter((p) => p.isTemplate || !p.assignedToStudentId).length,
      studentPlans: plans.filter((p) => p.assignedToStudentId).length,
    };
  }, [plans]);

  return (
    <div className="container mx-auto p-6 max-w-7xl space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Marketplace</h1>
          <p className="text-muted-foreground mt-1">
            Gerencie seus planos de treino e conteúdo educacional
          </p>
        </div>
        <Button onClick={() => router.push('/plans/new')}>
          <Plus className="h-4 w-4 mr-2" />
          Criar Novo Plano
        </Button>
      </div>

      {/* Overview Stats */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Visão Geral</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <ShoppingBag className="h-4 w-4" />
                Total de Planos
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {plansLoading ? '...' : stats.total}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Todos os planos criados
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <DollarSign className="h-4 w-4" />
                À Venda
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {plansLoading ? '...' : stats.forSale}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Planos disponíveis para venda
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <Eye className="h-4 w-4" />
                Visualizações
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {plansLoading ? '...' : stats.totalViews.toLocaleString()}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Total de visualizações
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Plans Section */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Planos de Treino</h2>

        {/* Filter Chips */}
        <div className="flex items-center gap-2 flex-wrap mb-6">
          <button
            onClick={() => setActiveFilter('all')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'all'
                ? 'bg-primary text-primary-foreground'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            )}
          >
            Todos ({filterCounts.all})
          </button>
          <button
            onClick={() => setActiveFilter('for-sale')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'for-sale'
                ? 'bg-green-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            )}
          >
            Para Venda ({filterCounts.forSale})
          </button>
          <button
            onClick={() => setActiveFilter('templates')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'templates'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            )}
          >
            Templates ({filterCounts.templates})
          </button>
          <button
            onClick={() => setActiveFilter('student-plans')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'student-plans'
                ? 'bg-purple-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            )}
          >
            Dos Alunos ({filterCounts.studentPlans})
          </button>
        </div>

        {/* Plans Grid */}
        {plansLoading ? (
          <div className="text-center py-12">
            <p className="text-muted-foreground">Carregando planos...</p>
          </div>
        ) : filteredPlans.length === 0 ? (
          <div className="text-center py-12 border-2 border-dashed rounded-lg">
            <ShoppingBag className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
            <p className="text-muted-foreground">
              Nenhum plano encontrado
            </p>
            <Button
              variant="link"
              onClick={() => router.push('/plans/new')}
              className="mt-2"
            >
              Criar Primeiro Plano →
            </Button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {filteredPlans.map((plan) => (
              <Card key={plan.id} className="hover:shadow-md transition-shadow">
                <CardHeader>
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex-1 min-w-0">
                      <CardTitle className="text-base truncate">
                        {plan.name}
                      </CardTitle>
                      {plan.description && (
                        <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
                          {plan.description}
                        </p>
                      )}
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => router.push(`/plans/${plan.id}/edit`)}
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </CardHeader>
                <CardContent className="space-y-3">
                  {/* Badges */}
                  <div className="flex items-center gap-2 flex-wrap">
                    {plan.isForSale && (
                      <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                        À Venda
                      </Badge>
                    )}
                    {plan.isTemplate && (
                      <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
                        Template
                      </Badge>
                    )}
                    {plan.assignedToStudentId && (
                      <Badge variant="outline" className="bg-purple-50 text-purple-700 border-purple-200">
                        Aluno
                      </Badge>
                    )}
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-3 gap-2 text-center">
                    {plan.price && (
                      <div className="bg-gray-50 rounded p-2">
                        <div className="text-lg font-bold text-green-600">
                          R$ {plan.price.toFixed(2)}
                        </div>
                        <div className="text-xs text-muted-foreground">Preço</div>
                      </div>
                    )}
                    <div className="bg-gray-50 rounded p-2">
                      <div className="text-lg font-bold">
                        {plan.salesCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Vendas</div>
                    </div>
                    <div className="bg-gray-50 rounded p-2">
                      <div className="text-lg font-bold">
                        {plan.viewsCount}
                      </div>
                      <div className="text-xs text-muted-foreground">Views</div>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1"
                      onClick={() => router.push(`/plans/${plan.id}`)}
                    >
                      <Eye className="h-4 w-4 mr-1" />
                      Visualizar
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      className="flex-1"
                      onClick={() => router.push(`/plans/${plan.id}/edit`)}
                    >
                      <Edit className="h-4 w-4 mr-1" />
                      Editar
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      {/* Blog Posts Section */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-lg font-semibold flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Posts do Blog
            </h2>
            <p className="text-sm text-muted-foreground">
              Conteúdo educacional para engajar seus clientes
            </p>
          </div>
          <Button variant="outline" onClick={() => router.push('/instructor/posts/new')}>
            <Plus className="h-4 w-4 mr-2" />
            Novo Post
          </Button>
        </div>

        {posts && posts.length > 0 ? (
          <div className="space-y-3">
            {posts.slice(0, 5).map((post) => (
              <Card
                key={post.id}
                className="hover:shadow-md transition-shadow cursor-pointer"
                onClick={() => router.push(`/instructor/posts/${post.id}`)}
              >
                <CardContent className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <h3 className="font-medium">{post.title}</h3>
                      <div className="flex items-center gap-3 mt-1">
                        <span className="text-sm text-muted-foreground flex items-center gap-1">
                          <Eye className="h-3 w-3" />
                          {post.views} visualizações
                        </span>
                        {post.isDraft && (
                          <Badge variant="outline" className="bg-yellow-50 text-yellow-700 border-yellow-200">
                            Rascunho
                          </Badge>
                        )}
                      </div>
                    </div>
                    <Button variant="ghost" size="sm">
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
            <Button
              variant="link"
              className="w-full"
              onClick={() => router.push('/instructor/posts')}
            >
              Ver Todos os Posts →
            </Button>
          </div>
        ) : (
          <Card className="border-2 border-dashed">
            <CardContent className="p-8 text-center">
              <FileText className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
              <p className="text-muted-foreground mb-2">
                Nenhum post criado ainda
              </p>
              <p className="text-sm text-muted-foreground mb-4">
                Crie conteúdo educacional para atrair e engajar clientes
              </p>
              <Button onClick={() => router.push('/instructor/posts/new')}>
                <Plus className="h-4 w-4 mr-2" />
                Criar Primeiro Post
              </Button>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
