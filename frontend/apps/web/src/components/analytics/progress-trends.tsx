'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Card } from '@/components/ui/card';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import { TrendingUp, Users, Award, Activity, Calendar } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { useState } from 'react';
import { Button } from '@/components/ui/button';

interface DailyActivity {
  date: string;
  totalSessions: number;
  completedSessions: number;
  uniqueClients: number;
}

interface PlanEngagement {
  planId: string;
  planName: string;
  totalWorkouts: number;
  assignedClients: number;
  totalSessions: number;
  completedSessions: number;
}

interface ClientEngagement {
  clientId: string;
  clientName: string;
  totalSessions: number;
  completedSessions: number;
  lastActivity: string;
  completionRate: number;
}

interface ProgressTrendsData {
  dailyActivity: DailyActivity[];
  planEngagement: PlanEngagement[];
  clientEngagement: ClientEngagement[];
  summary: {
    totalSessionsInPeriod: number;
    completedSessionsInPeriod: number;
    activeClients: number;
    averageCompletionRate: number;
  };
}

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

export function ProgressTrends() {
  const [days, setDays] = useState(30);

  const { data: trends, isLoading } = useQuery({
    queryKey: ['progress-trends', days],
    queryFn: async () => {
      const response = await apiClient.get<ProgressTrendsData>(
        `/personal/analytics/progress-trends?days=${days}`
      );
      return response;
    },
  });

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
    });
  };

  const formatLastActivity = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Hoje';
    if (diffDays === 1) return 'Ontem';
    if (diffDays < 7) return `${diffDays} dias atrás`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} semanas atrás`;
    return date.toLocaleDateString('pt-BR');
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        {[1, 2, 3].map((i) => (
          <Card key={i} className="glass border-primary/20 p-6 animate-pulse">
            <div className="h-80 bg-muted rounded" />
          </Card>
        ))}
      </div>
    );
  }

  if (!trends) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Period Selector */}
      <div className="flex gap-2 flex-wrap">
        <Button
          variant={days === 7 ? 'default' : 'outline'}
          size="sm"
          onClick={() => setDays(7)}
          className="hover-lift tap-scale"
        >
          7 dias
        </Button>
        <Button
          variant={days === 30 ? 'default' : 'outline'}
          size="sm"
          onClick={() => setDays(30)}
          className="hover-lift tap-scale"
        >
          30 dias
        </Button>
        <Button
          variant={days === 90 ? 'default' : 'outline'}
          size="sm"
          onClick={() => setDays(90)}
          className="hover-lift tap-scale"
        >
          90 dias
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in">
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Sessões Totais</p>
            <Activity className="h-5 w-5 text-blue-500" />
          </div>
          <p className="text-3xl font-bold text-blue-500">
            {trends.summary.totalSessionsInPeriod}
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            {trends.summary.completedSessionsInPeriod} concluídas
          </p>
        </Card>

        <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '50ms' }}>
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Clientes Ativos</p>
            <Users className="h-5 w-5 text-green-500" />
          </div>
          <p className="text-3xl font-bold text-green-500">
            {trends.summary.activeClients}
          </p>
          <p className="text-xs text-muted-foreground mt-1">no período</p>
        </Card>

        <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '100ms' }}>
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Taxa de Conclusão</p>
            <TrendingUp className="h-5 w-5 text-purple-500" />
          </div>
          <p className="text-3xl font-bold text-purple-500">
            {trends.summary.averageCompletionRate.toFixed(1)}%
          </p>
          <p className="text-xs text-muted-foreground mt-1">média geral</p>
        </Card>

        <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '150ms' }}>
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Planos Ativos</p>
            <Award className="h-5 w-5 text-orange-500" />
          </div>
          <p className="text-3xl font-bold text-orange-500">
            {trends.planEngagement.length}
          </p>
          <p className="text-xs text-muted-foreground mt-1">com atividade</p>
        </Card>
      </div>

      {/* Daily Activity Chart */}
      {trends.dailyActivity.length > 0 && (
        <Card className="glass border-primary/20 p-6">
          <div className="flex items-center gap-2 mb-4">
            <Calendar className="h-5 w-5 text-primary" />
            <h3 className="font-semibold">Atividade Diária</h3>
          </div>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={trends.dailyActivity}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis
                dataKey="date"
                tickFormatter={formatDate}
                stroke="#9ca3af"
                style={{ fontSize: '12px' }}
              />
              <YAxis stroke="#9ca3af" style={{ fontSize: '12px' }} />
              <Tooltip
                contentStyle={{
                  backgroundColor: '#1f2937',
                  border: '1px solid #374151',
                  borderRadius: '8px',
                }}
                labelStyle={{ color: '#f3f4f6' }}
              />
              <Legend />
              <Line
                type="monotone"
                dataKey="totalSessions"
                stroke="#3b82f6"
                strokeWidth={2}
                name="Total de Sessões"
                dot={{ fill: '#3b82f6' }}
              />
              <Line
                type="monotone"
                dataKey="completedSessions"
                stroke="#10b981"
                strokeWidth={2}
                name="Sessões Concluídas"
                dot={{ fill: '#10b981' }}
              />
              <Line
                type="monotone"
                dataKey="uniqueClients"
                stroke="#8b5cf6"
                strokeWidth={2}
                name="Clientes Únicos"
                dot={{ fill: '#8b5cf6' }}
              />
            </LineChart>
          </ResponsiveContainer>
        </Card>
      )}

      {/* Plan Engagement Chart */}
      {trends.planEngagement.length > 0 && (
        <Card className="glass border-primary/20 p-6">
          <div className="flex items-center gap-2 mb-4">
            <Award className="h-5 w-5 text-primary" />
            <h3 className="font-semibold">Engajamento por Plano</h3>
          </div>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={trends.planEngagement}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis
                dataKey="planName"
                stroke="#9ca3af"
                style={{ fontSize: '12px' }}
                angle={-45}
                textAnchor="end"
                height={100}
              />
              <YAxis stroke="#9ca3af" style={{ fontSize: '12px' }} />
              <Tooltip
                contentStyle={{
                  backgroundColor: '#1f2937',
                  border: '1px solid #374151',
                  borderRadius: '8px',
                }}
              />
              <Legend />
              <Bar dataKey="totalSessions" fill="#3b82f6" name="Total de Sessões" />
              <Bar dataKey="completedSessions" fill="#10b981" name="Sessões Concluídas" />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      )}

      {/* Client Engagement List */}
      {trends.clientEngagement.length > 0 && (
        <Card className="glass border-primary/20 p-6">
          <div className="flex items-center gap-2 mb-4">
            <Users className="h-5 w-5 text-primary" />
            <h3 className="font-semibold">Top 10 Clientes Mais Ativos</h3>
          </div>
          <div className="space-y-3">
            {trends.clientEngagement.map((client, index) => (
              <div
                key={client.clientId}
                className="flex items-center justify-between p-3 glass rounded-lg border border-primary/10 hover-lift animate-scale-in"
                style={{ animationDelay: `${index * 30}ms` }}
              >
                <div className="flex items-center gap-3 flex-1">
                  <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary font-bold text-sm">
                    {index + 1}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium truncate">{client.clientName}</p>
                    <p className="text-sm text-muted-foreground">
                      {client.totalSessions} sessões • {client.completedSessions} concluídas
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-3 flex-shrink-0">
                  <div className="text-right">
                    <Badge variant="secondary" className="mb-1">
                      {client.completionRate.toFixed(0)}% conclusão
                    </Badge>
                    <p className="text-xs text-muted-foreground">
                      {formatLastActivity(client.lastActivity)}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Empty State */}
      {trends.dailyActivity.length === 0 && (
        <Card className="glass border-primary/20 p-12 text-center">
          <Activity className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
          <h3 className="text-lg font-semibold mb-2">Nenhuma atividade no período</h3>
          <p className="text-muted-foreground">
            Não há dados de treino para os últimos {days} dias
          </p>
        </Card>
      )}
    </div>
  );
}
