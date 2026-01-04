'use client';

import { useState, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import {
  DollarSign,
  TrendingUp,
  Clock,
  CreditCard,
  Download,
  CheckCircle2,
  AlertCircle,
  Eye,
} from 'lucide-react';
import { apiClient } from '@/lib/api';
import { cn } from '@/lib/utils';

interface FinancialStats {
  monthlyRevenue: number;
  availableBalance: number;
  pendingBalance: number;
  totalSales: number;
  totalWithdrawals: number;
}

interface Sale {
  id: string;
  planName: string;
  studentName: string;
  amount: number;
  date: string;
  status: 'completed' | 'pending' | 'refunded';
}

interface Withdrawal {
  id: string;
  amount: number;
  status: 'completed' | 'pending' | 'processing';
  requestedAt: string;
  completedAt?: string;
}

interface Transaction {
  id: string;
  type: 'sale' | 'withdrawal' | 'refund';
  description: string;
  amount: number;
  date: string;
  status: string;
}

export default function FinancialPage() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState('sales');

  // Fetch financial stats
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['financial-stats'],
    queryFn: async () => {
      try {
        // TODO: Replace with real API when available
        return {
          monthlyRevenue: 3450.0,
          availableBalance: 2100.0,
          pendingBalance: 1350.0,
          totalSales: 45,
          totalWithdrawals: 12,
        } as FinancialStats;
      } catch (error) {
        return {
          monthlyRevenue: 0,
          availableBalance: 0,
          pendingBalance: 0,
          totalSales: 0,
          totalWithdrawals: 0,
        } as FinancialStats;
      }
    },
    staleTime: 1 * 60 * 1000,
  });

  // Fetch sales
  const { data: sales } = useQuery({
    queryKey: ['sales'],
    queryFn: async () => {
      try {
        // TODO: Replace with real API when available
        return [] as Sale[];
      } catch (error) {
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Fetch withdrawals
  const { data: withdrawals } = useQuery({
    queryKey: ['withdrawals'],
    queryFn: async () => {
      try {
        // TODO: Replace with real API when available
        return [] as Withdrawal[];
      } catch (error) {
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Fetch transactions
  const { data: transactions } = useQuery({
    queryKey: ['transactions'],
    queryFn: async () => {
      try {
        // TODO: Replace with real API when available
        return [] as Transaction[];
      } catch (error) {
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Check Stripe Connect status
  const { data: stripeStatus } = useQuery({
    queryKey: ['stripe-status'],
    queryFn: async () => {
      try {
        // TODO: Replace with real API when available
        return {
          connected: false,
          detailsSubmitted: false,
          chargesEnabled: false,
        };
      } catch (error) {
        return {
          connected: false,
          detailsSubmitted: false,
          chargesEnabled: false,
        };
      }
    },
    staleTime: 5 * 60 * 1000,
  });

  const handleRequestWithdrawal = () => {
    // TODO: Implement withdrawal request
    router.push('/instructor/financial/withdraw');
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'completed':
        return (
          <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
            <CheckCircle2 className="h-3 w-3 mr-1" />
            Concluído
          </Badge>
        );
      case 'pending':
        return (
          <Badge variant="outline" className="bg-yellow-50 text-yellow-700 border-yellow-200">
            <Clock className="h-3 w-3 mr-1" />
            Pendente
          </Badge>
        );
      case 'processing':
        return (
          <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
            <Clock className="h-3 w-3 mr-1" />
            Processando
          </Badge>
        );
      case 'refunded':
        return (
          <Badge variant="outline" className="bg-muted text-muted-foreground">
            Reembolsado
          </Badge>
        );
      default:
        return null;
    }
  };

  return (
    <div className="container mx-auto p-6 max-w-7xl space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Financeiro</h1>
          <p className="text-muted-foreground mt-1">
            Gerencie seus ganhos, vendas e saques
          </p>
        </div>
        <Button
          onClick={handleRequestWithdrawal}
          disabled={!stats?.availableBalance || stats.availableBalance <= 0}
        >
          <Download className="h-4 w-4 mr-2" />
          Solicitar Saque
        </Button>
      </div>

      {/* Stripe Connect Status Alert */}
      {stripeStatus && !stripeStatus.connected && (
        <Alert className="border-orange-200 bg-orange-50">
          <AlertCircle className="h-4 w-4 text-orange-600" />
          <AlertDescription>
            <strong>Configure sua conta Stripe</strong> para receber pagamentos.{' '}
            <Button
              variant="link"
              className="p-0 h-auto text-orange-700"
              onClick={() => router.push('/instructor/settings?tab=payments')}
            >
              Conectar Stripe →
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Monthly Summary */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Resumo do Mês</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <TrendingUp className="h-4 w-4" />
                Faturamento
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-green-600">
                R$ {statsLoading ? '...' : (stats?.monthlyRevenue || 0).toFixed(2)}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Total faturado este mês
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <DollarSign className="h-4 w-4" />
                Disponível
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-blue-600">
                R$ {statsLoading ? '...' : (stats?.availableBalance || 0).toFixed(2)}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Disponível para saque
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <Clock className="h-4 w-4" />
                A Receber
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-orange-600">
                R$ {statsLoading ? '...' : (stats?.pendingBalance || 0).toFixed(2)}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Aguardando liberação
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Tabs Section */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full max-w-md grid-cols-3">
          <TabsTrigger value="sales">
            Vendas ({sales?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="withdrawals">
            Saques ({withdrawals?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="transactions">
            Transações ({transactions?.length || 0})
          </TabsTrigger>
        </TabsList>

        {/* Sales Tab */}
        <TabsContent value="sales" className="mt-6">
          {sales && sales.length > 0 ? (
            <div className="border rounded-lg overflow-hidden">
              <table className="w-full">
                <thead className="bg-muted border-b">
                  <tr>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Plano
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Cliente
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Valor
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Data
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {sales.map((sale) => (
                    <tr key={sale.id} className="hover:bg-accent">
                      <td className="py-3 px-4 font-medium">{sale.planName}</td>
                      <td className="py-3 px-4">{sale.studentName}</td>
                      <td className="py-3 px-4 text-green-600 font-semibold">
                        R$ {sale.amount.toFixed(2)}
                      </td>
                      <td className="py-3 px-4 text-sm text-muted-foreground">
                        {new Date(sale.date).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="py-3 px-4">
                        {getStatusBadge(sale.status)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <Card className="border-2 border-dashed">
              <CardContent className="p-12 text-center">
                <DollarSign className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
                <p className="text-muted-foreground mb-2">
                  Nenhuma venda registrada
                </p>
                <p className="text-sm text-muted-foreground">
                  Suas vendas aparecerão aqui quando você vender planos
                </p>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Withdrawals Tab */}
        <TabsContent value="withdrawals" className="mt-6">
          {withdrawals && withdrawals.length > 0 ? (
            <div className="border rounded-lg overflow-hidden">
              <table className="w-full">
                <thead className="bg-muted border-b">
                  <tr>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Valor
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Solicitado em
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Concluído em
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {withdrawals.map((withdrawal) => (
                    <tr key={withdrawal.id} className="hover:bg-accent">
                      <td className="py-3 px-4 font-semibold">
                        R$ {withdrawal.amount.toFixed(2)}
                      </td>
                      <td className="py-3 px-4 text-sm text-muted-foreground">
                        {new Date(withdrawal.requestedAt).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="py-3 px-4 text-sm text-muted-foreground">
                        {withdrawal.completedAt
                          ? new Date(withdrawal.completedAt).toLocaleDateString('pt-BR')
                          : '-'}
                      </td>
                      <td className="py-3 px-4">
                        {getStatusBadge(withdrawal.status)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <Card className="border-2 border-dashed">
              <CardContent className="p-12 text-center">
                <Download className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
                <p className="text-muted-foreground mb-2">
                  Nenhum saque registrado
                </p>
                <p className="text-sm text-muted-foreground mb-4">
                  Solicite saques quando tiver saldo disponível
                </p>
                <Button
                  onClick={handleRequestWithdrawal}
                  disabled={!stats?.availableBalance || stats.availableBalance <= 0}
                >
                  <Download className="h-4 w-4 mr-2" />
                  Solicitar Primeiro Saque
                </Button>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Transactions Tab */}
        <TabsContent value="transactions" className="mt-6">
          {transactions && transactions.length > 0 ? (
            <div className="border rounded-lg overflow-hidden">
              <table className="w-full">
                <thead className="bg-muted border-b">
                  <tr>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Tipo
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Descrição
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Valor
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Data
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {transactions.map((transaction) => (
                    <tr key={transaction.id} className="hover:bg-accent">
                      <td className="py-3 px-4">
                        <Badge variant="outline">
                          {transaction.type === 'sale' && 'Venda'}
                          {transaction.type === 'withdrawal' && 'Saque'}
                          {transaction.type === 'refund' && 'Reembolso'}
                        </Badge>
                      </td>
                      <td className="py-3 px-4">{transaction.description}</td>
                      <td
                        className={cn(
                          'py-3 px-4 font-semibold',
                          transaction.type === 'sale' ? 'text-green-600' : 'text-red-600'
                        )}
                      >
                        {transaction.type === 'sale' ? '+' : '-'}R${' '}
                        {transaction.amount.toFixed(2)}
                      </td>
                      <td className="py-3 px-4 text-sm text-muted-foreground">
                        {new Date(transaction.date).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="py-3 px-4">
                        {getStatusBadge(transaction.status)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <Card className="border-2 border-dashed">
              <CardContent className="p-12 text-center">
                <CreditCard className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
                <p className="text-muted-foreground mb-2">
                  Nenhuma transação registrada
                </p>
                <p className="text-sm text-muted-foreground">
                  Todas as suas transações aparecerão aqui
                </p>
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
