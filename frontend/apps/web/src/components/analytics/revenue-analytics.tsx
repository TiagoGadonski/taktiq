'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Card } from '@/components/ui/card';
import {
  DollarSign,
  ShoppingCart,
  TrendingUp,
  RefreshCw,
  Award,
  Calendar,
  ArrowUpRight,
  ArrowDownRight,
} from 'lucide-react';
import { Badge } from '@/components/ui/badge';

interface RevenueAnalytics {
  totalRevenue: number;
  totalSales: number;
  completedSales: number;
  refundedSales: number;
  averageOrderValue: number;
  topSellingPlans: Array<{
    planId: string;
    planName: string;
    salesCount: number;
    totalRevenue: number;
  }>;
  recentSales: Array<{
    transactionId: string;
    buyerName: string;
    workoutPlanName: string;
    amount: number;
    createdAt: string;
    status: string;
  }>;
  revenueByMonth: Array<{
    year: number;
    month: number;
    revenue: number;
    salesCount: number;
  }>;
}

export function RevenueAnalytics() {
  const { data: analytics, isLoading } = useQuery({
    queryKey: ['revenue-analytics'],
    queryFn: async () => {
      const response = await apiClient.get<RevenueAnalytics>('/payments/revenue-analytics');
      return response;
    },
  });

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  };

  const getMonthName = (month: number) => {
    const months = [
      'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
      'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'
    ];
    return months[month - 1];
  };

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return <Badge variant="default">Concluído</Badge>;
      case 'pending':
        return <Badge variant="secondary">Pendente</Badge>;
      case 'refunded':
        return <Badge variant="outline">Reembolsado</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} className="glass border-primary/20 p-6 animate-pulse">
              <div className="h-6 bg-muted rounded w-3/4 mb-2" />
              <div className="h-8 bg-muted rounded w-1/2" />
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (!analytics) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Key Metrics */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {/* Total Revenue */}
        <Card className="glass border-primary/20 hover-lift tap-scale animate-scale-in p-6">
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Receita Total</p>
            <DollarSign className="h-5 w-5 text-green-500" />
          </div>
          <p className="text-3xl font-bold text-green-500">
            {formatCurrency(analytics.totalRevenue)}
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            {analytics.completedSales} vendas concluídas
          </p>
        </Card>

        {/* Total Sales */}
        <Card
          className="glass border-primary/20 hover-lift tap-scale animate-scale-in p-6"
          style={{ animationDelay: '50ms' }}
        >
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Total de Vendas</p>
            <ShoppingCart className="h-5 w-5 text-primary" />
          </div>
          <p className="text-3xl font-bold">{analytics.totalSales}</p>
          <p className="text-xs text-muted-foreground mt-1">
            {analytics.refundedSales} reembolsadas
          </p>
        </Card>

        {/* Average Order Value */}
        <Card
          className="glass border-primary/20 hover-lift tap-scale animate-scale-in p-6"
          style={{ animationDelay: '100ms' }}
        >
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Ticket Médio</p>
            <TrendingUp className="h-5 w-5 text-blue-500" />
          </div>
          <p className="text-3xl font-bold text-blue-500">
            {formatCurrency(analytics.averageOrderValue)}
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            por venda
          </p>
        </Card>

        {/* Refund Rate */}
        <Card
          className="glass border-primary/20 hover-lift tap-scale animate-scale-in p-6"
          style={{ animationDelay: '150ms' }}
        >
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-muted-foreground">Taxa de Reembolso</p>
            <RefreshCw className="h-5 w-5 text-orange-500" />
          </div>
          <p className="text-3xl font-bold text-orange-500">
            {analytics.totalSales > 0
              ? ((analytics.refundedSales / analytics.totalSales) * 100).toFixed(1)
              : '0'}%
          </p>
          <p className="text-xs text-muted-foreground mt-1">
            {analytics.refundedSales} de {analytics.totalSales} vendas
          </p>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Top Selling Plans */}
        <Card className="glass border-primary/20 p-6">
          <div className="flex items-center gap-2 mb-4">
            <Award className="h-5 w-5 text-primary" />
            <h3 className="font-semibold">Planos Mais Vendidos</h3>
          </div>

          {analytics.topSellingPlans.length > 0 ? (
            <div className="space-y-3">
              {analytics.topSellingPlans.map((plan, index) => (
                <div
                  key={plan.planId}
                  className="flex items-center justify-between p-3 glass rounded-lg border border-primary/10 hover-lift"
                >
                  <div className="flex items-center gap-3 flex-1 min-w-0">
                    <div className="flex-shrink-0 flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary font-bold text-sm">
                      {index + 1}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{plan.planName}</p>
                      <p className="text-sm text-muted-foreground">
                        {plan.salesCount} {plan.salesCount === 1 ? 'venda' : 'vendas'}
                      </p>
                    </div>
                  </div>
                  <div className="text-right flex-shrink-0">
                    <p className="font-bold text-green-500">
                      {formatCurrency(plan.totalRevenue)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <Award className="h-12 w-12 mx-auto mb-2 opacity-50" />
              <p className="text-sm">Nenhuma venda ainda</p>
            </div>
          )}
        </Card>

        {/* Revenue by Month */}
        <Card className="glass border-primary/20 p-6">
          <div className="flex items-center gap-2 mb-4">
            <Calendar className="h-5 w-5 text-primary" />
            <h3 className="font-semibold">Receita por Mês</h3>
          </div>

          {analytics?.revenueByMonth && analytics.revenueByMonth.length > 0 ? (
            <div className="space-y-2">
              {(analytics.revenueByMonth || []).slice(-6).map((period, index) => {
                const maxRevenue = Math.max(...(analytics.revenueByMonth || []).map(p => p.revenue));
                const widthPercent = maxRevenue > 0 ? (period.revenue / maxRevenue) * 100 : 0;

                return (
                  <div key={`${period.year}-${period.month}`} className="space-y-1">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">
                        {getMonthName(period.month)} {period.year}
                      </span>
                      <span className="font-medium">
                        {formatCurrency(period.revenue)}
                      </span>
                    </div>
                    <div className="h-2 bg-muted rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-primary to-primary/70 transition-all duration-500"
                        style={{ width: `${widthPercent}%` }}
                      />
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {period.salesCount} {period.salesCount === 1 ? 'venda' : 'vendas'}
                    </p>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <Calendar className="h-12 w-12 mx-auto mb-2 opacity-50" />
              <p className="text-sm">Nenhuma receita nos últimos meses</p>
            </div>
          )}
        </Card>
      </div>

      {/* Recent Sales */}
      <Card className="glass border-primary/20 p-6">
        <div className="flex items-center gap-2 mb-4">
          <ShoppingCart className="h-5 w-5 text-primary" />
          <h3 className="font-semibold">Vendas Recentes</h3>
        </div>

        {analytics.recentSales.length > 0 ? (
          <div className="space-y-2">
            {analytics.recentSales.map((sale, index) => (
              <div
                key={sale.transactionId}
                className="flex items-center justify-between p-3 glass rounded-lg border border-primary/10 hover-lift animate-scale-in"
                style={{ animationDelay: `${index * 30}ms` }}
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium truncate">{sale.workoutPlanName}</p>
                  <p className="text-sm text-muted-foreground">
                    {sale.buyerName} • {formatDate(sale.createdAt)}
                  </p>
                </div>
                <div className="flex items-center gap-3 flex-shrink-0">
                  <p className="font-bold text-green-500">
                    {formatCurrency(sale.amount)}
                  </p>
                  {getStatusBadge(sale.status)}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-muted-foreground">
            <ShoppingCart className="h-12 w-12 mx-auto mb-2 opacity-50" />
            <p className="text-sm">Nenhuma venda registrada</p>
          </div>
        )}
      </Card>
    </div>
  );
}
