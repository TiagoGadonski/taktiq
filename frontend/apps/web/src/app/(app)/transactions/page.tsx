'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  Receipt,
  ArrowUpRight,
  ArrowDownLeft,
  DollarSign,
  Calendar,
  CheckCircle2,
  XCircle,
  Clock,
  Ban,
  RefreshCw,
  AlertCircle,
  Download,
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';

interface Transaction {
  id: string;
  workoutPlanName: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
  isPurchase: boolean;
}

export default function TransactionsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState<'all' | 'purchases' | 'sales'>('all');
  const [refundingTransaction, setRefundingTransaction] = useState<Transaction | null>(null);

  const { data: transactions, isLoading } = useQuery({
    queryKey: ['transactions', filter],
    queryFn: async () => {
      const params = filter !== 'all' ? `?type=${filter}` : '';
      const response = await apiClient.get<Transaction[]>(`/payments/transactions${params}`);
      return response;
    },
  });

  const refundMutation = useMutation({
    mutationFn: (transactionId: string) =>
      apiClient.post(`/payments/transactions/${transactionId}/refund`, {}),
    onSuccess: () => {
      toast({
        title: 'Reembolso processado',
        description: 'O reembolso foi processado com sucesso.',
      });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      setRefundingTransaction(null);
    },
    onError: (error: any) => {
      const errorData = error?.response?.data;
      toast({
        variant: 'destructive',
        title: 'Erro ao processar reembolso',
        description: errorData?.message || 'Não foi possível processar o reembolso.',
      });
    },
  });

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return <CheckCircle2 className="h-4 w-4 text-green-500" />;
      case 'pending':
        return <Clock className="h-4 w-4 text-yellow-500" />;
      case 'failed':
        return <XCircle className="h-4 w-4 text-red-500" />;
      case 'cancelled':
        return <Ban className="h-4 w-4 text-gray-500" />;
      case 'refunded':
        return <RefreshCw className="h-4 w-4 text-blue-500" />;
      default:
        return <AlertCircle className="h-4 w-4 text-gray-500" />;
    }
  };

  const getStatusBadgeVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'default';
      case 'pending':
        return 'secondary';
      case 'failed':
      case 'cancelled':
        return 'destructive';
      case 'refunded':
        return 'outline';
      default:
        return 'secondary';
    }
  };

  const getStatusLabel = (status: string) => {
    const labels: Record<string, string> = {
      completed: 'Concluído',
      pending: 'Pendente',
      failed: 'Falhou',
      cancelled: 'Cancelado',
      refunded: 'Reembolsado',
    };
    return labels[status.toLowerCase()] || status;
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatCurrency = (amount: number, currency: string) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: currency,
    }).format(amount);
  };

  const handleDownloadReceipt = async (transactionId: string) => {
    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/payments/transactions/${transactionId}/receipt`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error('Failed to download receipt');
      }

      // Get the blob from response
      const blob = await response.blob();

      // Create a download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Receipt-${transactionId}-${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();

      // Cleanup
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      toast({
        title: 'Recibo baixado',
        description: 'O recibo foi baixado com sucesso.',
      });
    } catch (error) {
      toast({
        variant: 'destructive',
        title: 'Erro ao baixar recibo',
        description: 'Não foi possível baixar o recibo.',
      });
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Receipt className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Transações
          </h1>
        </div>
        <p className="text-muted-foreground">
          Histórico de compras e vendas no marketplace
        </p>
      </div>

      {/* Filter Tabs */}
      <Card className="glass border-primary/20 p-4">
        <div className="flex gap-2 flex-wrap">
          <Button
            variant={filter === 'all' ? 'default' : 'outline'}
            onClick={() => setFilter('all')}
            size="sm"
            className="hover-lift tap-scale"
          >
            Todas
          </Button>
          <Button
            variant={filter === 'purchases' ? 'default' : 'outline'}
            onClick={() => setFilter('purchases')}
            size="sm"
            className="hover-lift tap-scale"
          >
            <ArrowUpRight className="h-4 w-4 mr-2" />
            Compras
          </Button>
          <Button
            variant={filter === 'sales' ? 'default' : 'outline'}
            onClick={() => setFilter('sales')}
            size="sm"
            className="hover-lift tap-scale"
          >
            <ArrowDownLeft className="h-4 w-4 mr-2" />
            Vendas
          </Button>
        </div>
      </Card>

      {/* Loading State */}
      {isLoading && (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Card key={i} className="glass border-primary/20 p-6 animate-pulse">
              <div className="space-y-3">
                <div className="h-6 bg-muted rounded w-1/3" />
                <div className="h-4 bg-muted rounded w-1/4" />
                <div className="h-4 bg-muted rounded w-1/2" />
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Transactions List */}
      {!isLoading && transactions && transactions.length > 0 && (
        <div className="space-y-4">
          {transactions.map((transaction, index) => (
            <Card
              key={transaction.id}
              className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
              style={{ animationDelay: `${index * 50}ms` }}
            >
              <div className="p-6">
                <div className="flex items-start justify-between gap-4">
                  {/* Transaction Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-2">
                      {transaction.isPurchase ? (
                        <ArrowUpRight className="h-5 w-5 text-blue-500 flex-shrink-0" />
                      ) : (
                        <ArrowDownLeft className="h-5 w-5 text-green-500 flex-shrink-0" />
                      )}
                      <h3 className="font-semibold truncate">{transaction.workoutPlanName}</h3>
                    </div>

                    <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
                      <div className="flex items-center gap-1.5">
                        <DollarSign className="h-4 w-4" />
                        <span className="font-medium text-foreground">
                          {formatCurrency(transaction.amount, transaction.currency)}
                        </span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        <Calendar className="h-4 w-4" />
                        <span>{formatDate(transaction.createdAt)}</span>
                      </div>

                      <div className="flex items-center gap-1.5">
                        {getStatusIcon(transaction.status)}
                        <Badge variant={getStatusBadgeVariant(transaction.status)}>
                          {getStatusLabel(transaction.status)}
                        </Badge>
                      </div>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex-shrink-0 flex gap-2">
                    {transaction.status.toLowerCase() === 'completed' && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleDownloadReceipt(transaction.id)}
                        className="hover-lift tap-scale"
                      >
                        <Download className="h-4 w-4 mr-2" />
                        Recibo
                      </Button>
                    )}
                    {!transaction.isPurchase &&
                      transaction.status.toLowerCase() === 'completed' && (
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setRefundingTransaction(transaction)}
                          disabled={refundMutation.isPending}
                          className="hover-lift tap-scale"
                        >
                          <RefreshCw className="h-4 w-4 mr-2" />
                          Reembolsar
                        </Button>
                      )}
                  </div>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Empty State */}
      {!isLoading && transactions && transactions.length === 0 && (
        <Card className="glass border-primary/20 p-12 text-center">
          <Receipt className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
          <h3 className="text-lg font-semibold mb-2">Nenhuma transação encontrada</h3>
          <p className="text-muted-foreground">
            {filter === 'purchases' &&
              'Você ainda não fez compras no marketplace'}
            {filter === 'sales' &&
              'Você ainda não vendeu planos no marketplace'}
            {filter === 'all' && 'Ainda não há transações registradas'}
          </p>
        </Card>
      )}

      {/* Refund Confirmation Dialog */}
      <Dialog
        open={!!refundingTransaction}
        onOpenChange={() => setRefundingTransaction(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar Reembolso</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja reembolsar esta transação?
            </DialogDescription>
          </DialogHeader>

          {refundingTransaction && (
            <div className="space-y-4 py-4">
              <div className="glass rounded-lg p-4 border">
                <h4 className="font-semibold mb-2">Detalhes da Transação</h4>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Plano:</span>
                    <span className="font-medium">{refundingTransaction.workoutPlanName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Valor:</span>
                    <span className="font-medium">
                      {formatCurrency(refundingTransaction.amount, refundingTransaction.currency)}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Data:</span>
                    <span>{formatDate(refundingTransaction.createdAt)}</span>
                  </div>
                </div>
              </div>

              <div className="bg-yellow-500/10 border border-yellow-500/30 rounded-lg p-4">
                <div className="flex gap-3">
                  <AlertCircle className="h-5 w-5 text-yellow-500 flex-shrink-0 mt-0.5" />
                  <div className="text-sm">
                    <p className="font-medium mb-1">Atenção</p>
                    <p className="text-muted-foreground">
                      O valor total será reembolsado ao comprador. Esta ação não pode ser desfeita.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setRefundingTransaction(null)}
              disabled={refundMutation.isPending}
            >
              Cancelar
            </Button>
            <Button
              variant="destructive"
              onClick={() =>
                refundingTransaction &&
                refundMutation.mutate(refundingTransaction.id)
              }
              disabled={refundMutation.isPending}
              className="hover-lift tap-scale"
            >
              {refundMutation.isPending ? (
                <>
                  <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                  Processando...
                </>
              ) : (
                <>
                  <RefreshCw className="h-4 w-4 mr-2" />
                  Confirmar Reembolso
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
