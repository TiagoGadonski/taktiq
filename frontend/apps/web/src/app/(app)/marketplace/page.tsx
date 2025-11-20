'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  ShoppingCart,
  Search,
  Dumbbell,
  Calendar,
  Eye,
  TrendingUp,
  Check,
  Filter,
  X,
  CreditCard,
  Info,
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { useAuth } from '@/hooks/use-auth';
import { StripeCheckout } from '@/components/payment/stripe-checkout';

interface MarketplacePlan {
  id: string;
  name: string;
  description?: string;
  goal?: string;
  duration?: number;
  price?: number;
  creatorId: string;
  creatorName: string;
  viewCount: number;
  workoutCount: number;
  exerciseCount: number;
  publishedAt?: string;
}

export default function MarketplacePage() {
  const { toast } = useToast();
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const [searchTerm, setSearchTerm] = useState('');
  const [goalFilter, setGoalFilter] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [page, setPage] = useState(1);
  const [checkoutPlan, setCheckoutPlan] = useState<MarketplacePlan | null>(null);
  const [showCheckout, setShowCheckout] = useState(false);

  const { data: marketplaceData, isLoading } = useQuery({
    queryKey: ['marketplace', page, searchTerm, goalFilter],
    queryFn: async () => {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '12');
      if (searchTerm) params.append('search', searchTerm);
      if (goalFilter) params.append('goal', goalFilter);

      const queryString = params.toString();
      const url = `/marketplace/plans${queryString ? `?${queryString}` : ''}`;
      const response = await apiClient.get<{
        data: MarketplacePlan[];
        page: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
      }>(url);
      return response;
    },
  });

  const purchaseMutation = useMutation({
    mutationFn: (planId: string) => apiClient.post(`/marketplace/plans/${planId}/purchase`, {}),
    onSuccess: () => {
      toast({
        title: 'Plano adquirido!',
        description: 'O plano foi adicionado à sua conta com sucesso.',
      });
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
    },
    onError: (error: any) => {
      const errorData = error?.response?.data;
      toast({
        variant: 'destructive',
        title: 'Erro ao adquirir plano',
        description: errorData?.message || 'Não foi possível adquirir o plano.',
      });
    },
  });

  const handlePurchaseClick = (plan: MarketplacePlan) => {
    if (!user) {
      toast({
        variant: 'destructive',
        title: 'Login necessário',
        description: 'Faça login para adquirir planos do marketplace.',
      });
      return;
    }

    // If the plan is paid, open Stripe checkout
    if (plan.price && plan.price > 0) {
      setCheckoutPlan(plan);
      setShowCheckout(true);
    } else {
      // For free plans, purchase directly
      purchaseMutation.mutate(plan.id);
    }
  };

  const handleCheckoutSuccess = () => {
    setShowCheckout(false);
    setCheckoutPlan(null);
    toast({
      title: 'Pagamento confirmado!',
      description: 'O plano foi adicionado à sua conta com sucesso.',
    });
    queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
  };

  const handleCheckoutCancel = () => {
    setShowCheckout(false);
    setCheckoutPlan(null);
  };

  const clearFilters = () => {
    setSearchTerm('');
    setGoalFilter('');
  };

  const hasActiveFilters = searchTerm || goalFilter;
  const plans = marketplaceData?.data || [];

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <ShoppingCart className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Marketplace de Planos
          </h1>
        </div>
        <p className="text-muted-foreground">
          Descubra e adquira planos de treino criados por personal trainers profissionais
        </p>
      </div>

      {/* Payment Info */}
      <Card className="glass border-green-500/30 bg-green-500/5 p-4">
        <div className="flex items-start gap-3">
          <div className="rounded-full bg-green-500/10 p-2">
            <CreditCard className="h-5 w-5 text-green-500" />
          </div>
          <div className="flex-1">
            <h3 className="font-semibold text-sm mb-1 flex items-center gap-2">
              <Check className="h-4 w-4" />
              Pagamentos Seguros com Stripe
            </h3>
            <p className="text-xs text-muted-foreground">
              Planos gratuitos podem ser adquiridos imediatamente. Para planos pagos, utilize nosso checkout seguro integrado com Stripe.
              Todos os pagamentos são processados com segurança.
            </p>
          </div>
        </div>
      </Card>

      {/* Search and Filters */}
      <Card className="glass border-primary/20 p-6">
        <div className="space-y-4">
          {/* Search Bar */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar planos por nome, descrição ou objetivo..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 glass border-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Filter Toggle */}
          <div className="flex items-center justify-between">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowFilters(!showFilters)}
              className="hover-lift tap-scale"
            >
              <Filter className="mr-2 h-4 w-4" />
              {showFilters ? 'Esconder Filtros' : 'Mostrar Filtros'}
            </Button>

            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="sm"
                onClick={clearFilters}
                className="text-muted-foreground hover:text-foreground"
              >
                <X className="mr-2 h-4 w-4" />
                Limpar Filtros
              </Button>
            )}
          </div>

          {/* Filter Inputs */}
          {showFilters && (
            <div className="pt-4 border-t border-border/50 animate-scale-in">
              <div className="space-y-2">
                <label className="text-sm font-medium">Objetivo</label>
                <Input
                  placeholder="Ex: Hipertrofia, Força, Emagrecimento"
                  value={goalFilter}
                  onChange={(e) => setGoalFilter(e.target.value)}
                  className="glass"
                />
              </div>
            </div>
          )}
        </div>
      </Card>

      {/* Results Info */}
      {!isLoading && marketplaceData && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {marketplaceData.totalCount} {marketplaceData.totalCount === 1 ? 'plano encontrado' : 'planos encontrados'}
          </p>
          {hasActiveFilters && (
            <div className="flex gap-2 flex-wrap">
              {searchTerm && (
                <Badge variant="outline" className="border-primary/30">
                  Busca: {searchTerm}
                </Badge>
              )}
              {goalFilter && (
                <Badge variant="outline" className="border-primary/30">
                  Objetivo: {goalFilter}
                </Badge>
              )}
            </div>
          )}
        </div>
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Card key={i} className="glass border-primary/20 p-6 animate-pulse">
              <div className="space-y-4">
                <div className="h-6 bg-muted rounded w-3/4" />
                <div className="space-y-2">
                  <div className="h-4 bg-muted rounded" />
                  <div className="h-4 bg-muted rounded w-5/6" />
                </div>
                <div className="h-10 bg-muted rounded" />
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Plans Grid */}
      {!isLoading && plans.length > 0 && (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan, index) => (
            <Card
              key={plan.id}
              className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
              style={{ animationDelay: `${index * 50}ms` }}
            >
              <div className="p-6 space-y-4">
                {/* Plan Header */}
                <div>
                  <h3 className="font-bold text-lg mb-2">{plan.name}</h3>
                  {plan.description && (
                    <p className="text-sm text-muted-foreground line-clamp-2">
                      {plan.description}
                    </p>
                  )}
                </div>

                {/* Plan Details */}
                <div className="space-y-2">
                  {plan.goal && (
                    <div className="flex items-center gap-2 text-sm">
                      <TrendingUp className="h-4 w-4 text-primary" />
                      <span className="text-muted-foreground">Objetivo:</span>
                      <span className="font-medium">{plan.goal}</span>
                    </div>
                  )}
                  {plan.duration && (
                    <div className="flex items-center gap-2 text-sm">
                      <Calendar className="h-4 w-4 text-primary" />
                      <span className="text-muted-foreground">Duração:</span>
                      <span className="font-medium">{plan.duration} semanas</span>
                    </div>
                  )}
                  <div className="flex items-center gap-2 text-sm">
                    <Dumbbell className="h-4 w-4 text-primary" />
                    <span className="text-muted-foreground">Treinos:</span>
                    <span className="font-medium">{plan.workoutCount}</span>
                  </div>
                  <div className="flex items-center gap-2 text-sm">
                    <Eye className="h-4 w-4 text-primary" />
                    <span className="text-muted-foreground">Visualizações:</span>
                    <span className="font-medium">{plan.viewCount}</span>
                  </div>
                </div>

                {/* Creator Info */}
                <div className="pt-4 border-t border-border/50">
                  <p className="text-sm text-muted-foreground">
                    Criado por <span className="font-medium text-foreground">{plan.creatorName}</span>
                  </p>
                </div>

                {/* Price and Purchase */}
                <div className="flex items-center justify-between pt-4 border-t border-border/50">
                  <div>
                    {plan.price ? (
                      <p className="text-2xl font-bold text-primary">
                        R$ {plan.price.toFixed(2)}
                      </p>
                    ) : (
                      <p className="text-sm font-semibold text-green-500 flex items-center gap-1">
                        <Check className="h-4 w-4" />
                        Grátis
                      </p>
                    )}
                  </div>
                  <Button
                    onClick={() => handlePurchaseClick(plan)}
                    disabled={!user || purchaseMutation.isPending}
                    className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale"
                  >
                    {purchaseMutation.isPending ? (
                      <div className="h-4 w-4 animate-spin rounded-full border-2 border-background border-t-transparent" />
                    ) : (
                      <>
                        <ShoppingCart className="mr-2 h-4 w-4" />
                        {plan.price && plan.price > 0 ? 'Comprar' : 'Adquirir'}
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Empty State */}
      {!isLoading && plans.length === 0 && (
        <Card className="glass border-primary/20 p-12 text-center">
          <ShoppingCart className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
          <h3 className="text-lg font-semibold mb-2">
            Nenhum plano disponível
          </h3>
          <p className="text-muted-foreground mb-4">
            {hasActiveFilters
              ? 'Tente ajustar seus filtros de busca'
              : 'Ainda não há planos de treino disponíveis para compra'}
          </p>
          {hasActiveFilters && (
            <Button onClick={clearFilters} variant="outline" className="hover-lift tap-scale">
              Limpar Filtros
            </Button>
          )}
        </Card>
      )}

      {/* Pagination */}
      {!isLoading && marketplaceData && marketplaceData.totalPages > 1 && (
        <div className="flex justify-center gap-2">
          <Button
            variant="outline"
            onClick={() => setPage(page - 1)}
            disabled={page === 1}
            className="hover-lift tap-scale"
          >
            Anterior
          </Button>
          <div className="flex items-center gap-2 px-4">
            <span className="text-sm text-muted-foreground">
              Página {page} de {marketplaceData.totalPages}
            </span>
          </div>
          <Button
            variant="outline"
            onClick={() => setPage(page + 1)}
            disabled={page === marketplaceData.totalPages}
            className="hover-lift tap-scale"
          >
            Próxima
          </Button>
        </div>
      )}

      {/* Login Prompt */}
      {!user && (
        <Card className="glass border-yellow-500/30 bg-yellow-500/5 p-6 text-center">
          <p className="text-sm text-muted-foreground">
            Faça login para adquirir planos de treino do marketplace
          </p>
        </Card>
      )}

      {/* Stripe Checkout Dialog */}
      <Dialog open={showCheckout} onOpenChange={setShowCheckout}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Finalizar Compra</DialogTitle>
          </DialogHeader>
          {checkoutPlan && (
            <StripeCheckout
              planId={checkoutPlan.id}
              planName={checkoutPlan.name}
              planPrice={checkoutPlan.price || 0}
              onSuccess={handleCheckoutSuccess}
              onCancel={handleCheckoutCancel}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
