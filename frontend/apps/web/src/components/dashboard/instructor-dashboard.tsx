'use client';

import { useQuery } from '@tanstack/react-query';
import {
  Users,
  DollarSign,
  FileText,
  TrendingUp,
  Activity,
  Calendar,
  Target,
  ShoppingCart,
  Plus,
  Eye
} from 'lucide-react';
import { apiClient } from '@/lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import Link from 'next/link';

interface InstructorAnalytics {
  totalClients: number;
  activeClients: number;
  totalPosts: number;
  publishedPosts: number;
  totalPlans: number;
  plansForSale: number;
  totalViews: number;
  totalInvitations: number;
  acceptedInvitations: number;
  monthlyRevenue: number;
  totalRevenue: number;
}

export function InstructorDashboard() {
  const { data: analytics, isLoading } = useQuery<InstructorAnalytics>({
    queryKey: ['instructor-analytics'],
    queryFn: () => apiClient.get('/personal/analytics'),
  });

  const { data: recentClients = [] } = useQuery({
    queryKey: ['recent-clients'],
    queryFn: async () => {
      const response = await apiClient.get('/personal/clients');
      return Array.isArray(response) ? response.slice(0, 5) : [];
    },
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Dashboard do Instrutor</h1>
        <p className="text-muted-foreground">
          Gerencie seus alunos, posts e planos de treino
        </p>
      </div>

      {/* Quick Actions */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Link href="/instructor?tab=clients">
          <Card className="hover-lift tap-scale cursor-pointer border-primary/20 transition-all hover:border-primary/50">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Clientes Ativos</CardTitle>
              <Users className="h-4 w-4 text-primary" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {isLoading ? '...' : analytics?.activeClients || 0}
              </div>
              <p className="text-xs text-muted-foreground">
                {analytics?.totalClients || 0} clientes totais
              </p>
            </CardContent>
          </Card>
        </Link>

        <Link href="/instructor?tab=sales">
          <Card className="hover-lift tap-scale cursor-pointer border-primary/20 transition-all hover:border-primary/50">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Receita Mensal</CardTitle>
              <DollarSign className="h-4 w-4 text-green-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                R$ {isLoading ? '...' : (analytics?.monthlyRevenue || 0).toFixed(2)}
              </div>
              <p className="text-xs text-muted-foreground">
                Total: R$ {(analytics?.totalRevenue || 0).toFixed(2)}
              </p>
            </CardContent>
          </Card>
        </Link>

        <Link href="/instructor?tab=posts">
          <Card className="hover-lift tap-scale cursor-pointer border-primary/20 transition-all hover:border-primary/50">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Posts Publicados</CardTitle>
              <FileText className="h-4 w-4 text-blue-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {isLoading ? '...' : analytics?.publishedPosts || 0}
              </div>
              <p className="text-xs text-muted-foreground">
                {analytics?.totalPosts || 0} posts totais
              </p>
            </CardContent>
          </Card>
        </Link>

        <Link href="/marketplace">
          <Card className="hover-lift tap-scale cursor-pointer border-primary/20 transition-all hover:border-primary/50">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Planos à Venda</CardTitle>
              <ShoppingCart className="h-4 w-4 text-purple-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {isLoading ? '...' : analytics?.plansForSale || 0}
              </div>
              <p className="text-xs text-muted-foreground">
                {analytics?.totalViews || 0} visualizações
              </p>
            </CardContent>
          </Card>
        </Link>
      </div>

      {/* Main Actions */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5 text-primary" />
              Adicionar Cliente
            </CardTitle>
            <CardDescription>
              Convide novos alunos para treinar com você
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/instructor?tab=invitations">
              <Button className="w-full">
                <Plus className="mr-2 h-4 w-4" />
                Enviar Convite
              </Button>
            </Link>
          </CardContent>
        </Card>

        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              Criar Post
            </CardTitle>
            <CardDescription>
              Compartilhe conteúdo com seus alunos
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/instructor?tab=posts">
              <Button className="w-full">
                <Plus className="mr-2 h-4 w-4" />
                Novo Post
              </Button>
            </Link>
          </CardContent>
        </Card>

        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Target className="h-5 w-5 text-primary" />
              Criar Plano
            </CardTitle>
            <CardDescription>
              Crie um novo plano de treino
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/plans/new">
              <Button className="w-full">
                <Plus className="mr-2 h-4 w-4" />
                Novo Plano
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>

      {/* Recent Clients */}
      {recentClients && recentClients.length > 0 && (
        <Card className="border-primary/20">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Clientes Recentes</CardTitle>
                <CardDescription>
                  Últimos alunos que se juntaram a você
                </CardDescription>
              </div>
              <Link href="/instructor?tab=clients">
                <Button variant="outline" size="sm">
                  Ver Todos
                </Button>
              </Link>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {recentClients.slice(0, 5).map((client: any) => (
                <div
                  key={client.id}
                  className="flex items-center justify-between p-3 rounded-lg border border-border/50 hover:border-primary/50 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-primary/20 flex items-center justify-center text-primary font-bold">
                      {client.name?.charAt(0).toUpperCase() || 'A'}
                    </div>
                    <div>
                      <p className="font-medium">{client.name}</p>
                      <p className="text-sm text-muted-foreground">
                        {client.email}
                      </p>
                    </div>
                  </div>
                  <div className="text-right text-sm text-muted-foreground">
                    <p>{new Date(client.createdAt).toLocaleDateString('pt-BR')}</p>
                    <p className="text-xs">{client.workoutPlans || 0} planos</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Statistics */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Activity className="h-4 w-4" />
              Taxa de Conversão
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analytics?.totalInvitations
                ? ((analytics.acceptedInvitations / analytics.totalInvitations) * 100).toFixed(0)
                : 0}%
            </div>
            <p className="text-xs text-muted-foreground">
              {analytics?.acceptedInvitations || 0} de {analytics?.totalInvitations || 0} convites aceitos
            </p>
          </CardContent>
        </Card>

        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Eye className="h-4 w-4" />
              Visualizações de Planos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analytics?.totalViews || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              Média: {analytics?.totalPlans
                ? (analytics.totalViews / analytics.totalPlans).toFixed(1)
                : 0} por plano
            </p>
          </CardContent>
        </Card>

        <Card className="border-primary/20">
          <CardHeader>
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <TrendingUp className="h-4 w-4" />
              Planos Ativos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analytics?.totalPlans || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              {analytics?.plansForSale || 0} no marketplace
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Links */}
      <Card className="border-primary/20">
        <CardHeader>
          <CardTitle>Acesso Rápido</CardTitle>
          <CardDescription>
            Navegue rapidamente para as principais funcionalidades
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Link href="/instructor?tab=profile">
              <Button variant="outline" className="w-full justify-start">
                <Users className="mr-2 h-4 w-4" />
                Perfil Público
              </Button>
            </Link>
            <Link href="/plans">
              <Button variant="outline" className="w-full justify-start">
                <Target className="mr-2 h-4 w-4" />
                Meus Planos
              </Button>
            </Link>
            <Link href="/transactions">
              <Button variant="outline" className="w-full justify-start">
                <DollarSign className="mr-2 h-4 w-4" />
                Transações
              </Button>
            </Link>
            <Link href="/instructor?tab=progress">
              <Button variant="outline" className="w-full justify-start">
                <TrendingUp className="mr-2 h-4 w-4" />
                Progresso dos Clientes
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
