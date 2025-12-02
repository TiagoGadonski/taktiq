'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Sparkles,
  Search,
  Dumbbell,
  Calendar,
  Eye,
  TrendingUp,
  Check,
  Filter,
  X,
  CreditCard,
  Users,
  Gift,
  Crown,
  ShoppingCart,
  Star,
  Flame,
  ArrowUpDown,
  BookmarkCheck,
  Edit,
  Share2,
  Send,
  Settings,
  Trash2,
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useToast } from '@/components/ui/use-toast';
import { useAuth } from '@/hooks/use-auth';
import { ShareSettingsDialog } from '@/components/workout/share-settings-dialog';
import { ShareWithFriendsDialog } from '@/components/workout/share-with-friends-dialog';
import { MarketplaceSettingsDialog } from '@/components/workout/marketplace-settings-dialog';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

interface DiscoverPlan {
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
  forSale: boolean;
  isPublic: boolean;
}

type PlanTab = 'free' | 'premium' | 'friends' | 'myplans';
type SortOption = 'newest' | 'popular' | 'mostViewed';

export default function DiscoverPlansPage() {
  const { toast } = useToast();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const router = useRouter();

  const [activeTab, setActiveTab] = useState<PlanTab>('free');
  const [searchTerm, setSearchTerm] = useState('');
  const [goalFilter, setGoalFilter] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [sortBy, setSortBy] = useState<SortOption>('newest');
  const [page, setPage] = useState(1);

  // Dialog states for plan management
  const [selectedPlan, setSelectedPlan] = useState<DiscoverPlan | null>(null);
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [shareWithFriendsDialogOpen, setShareWithFriendsDialogOpen] = useState(false);
  const [marketplaceDialogOpen, setMarketplaceDialogOpen] = useState(false);

  // Fetch marketplace plans
  const { data: marketplaceData, isLoading: isLoadingMarketplace } = useQuery({
    queryKey: ['discover-plans', 'marketplace', page, searchTerm, goalFilter, activeTab],
    queryFn: async () => {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '12');
      if (searchTerm) params.append('search', searchTerm);
      if (goalFilter) params.append('goal', goalFilter);

      const queryString = params.toString();
      const url = `/marketplace/plans${queryString ? `?${queryString}` : ''}`;
      const response = await apiClient.get<{
        data: DiscoverPlan[];
        page: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
      }>(url);
      return response;
    },
    enabled: activeTab === 'free' || activeTab === 'premium',
  });

  // Fetch friend plans (plans shared by friends)
  const { data: friendPlans, isLoading: isLoadingFriends } = useQuery({
    queryKey: ['discover-plans', 'friends', searchTerm],
    queryFn: async () => {
      // This endpoint would need to be created to fetch plans from friends
      // For now, return empty array
      return { data: [], totalCount: 0 };
    },
    enabled: activeTab === 'friends' && !!user,
  });

  // Fetch user's own plans (created + purchased/assigned)
  const { data: myPlans, isLoading: isLoadingMyPlans } = useQuery({
    queryKey: ['workout-plans', user?.id],
    queryFn: async () => {
      const response = await apiClient.get<any[]>('/workout-plans');
      const plans = Array.isArray(response) ? response : [];

      // Separate created plans from acquired plans
      const createdPlans = plans.filter(p => p.ownerId === user?.id || p.creatorId === user?.id);
      const acquiredPlans = plans.filter(p => p.ownerId !== user?.id && p.creatorId !== user?.id);

      return {
        data: plans,
        createdPlans,
        acquiredPlans,
        totalCount: plans.length
      };
    },
    enabled: activeTab === 'myplans' && !!user,
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

  const deleteMutation = useMutation({
    mutationFn: (planId: string) => api.workoutPlans.delete(planId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Plano excluído',
        description: 'O plano foi excluído com sucesso.',
      });
    },
    onError: () => {
      toast({
        variant: 'destructive',
        title: 'Erro ao excluir plano',
        description: 'Não foi possível excluir o plano.',
      });
    },
  });

  const handlePurchaseClick = (plan: DiscoverPlan) => {
    if (!user) {
      toast({
        variant: 'destructive',
        title: 'Login necessário',
        description: 'Faça login para adquirir planos.',
      });
      return;
    }

    if (plan.price && plan.price > 0) {
      toast({
        title: 'Em Breve',
        description: 'O sistema de pagamentos estará disponível em breve. Por enquanto, apenas planos gratuitos podem ser adquiridos.',
      });
      return;
    }

    purchaseMutation.mutate(plan.id);
  };

  const clearFilters = () => {
    setSearchTerm('');
    setGoalFilter('');
  };

  const handleTabChange = (tab: string) => {
    setActiveTab(tab as PlanTab);
    setPage(1);
    clearFilters();
    setSortBy('newest'); // Reset sort when changing tabs
  };

  // Helper function to determine if a plan is "new" (published within 7 days)
  const isNewPlan = (plan: DiscoverPlan) => {
    if (!plan.publishedAt) return false;
    const publishedDate = new Date(plan.publishedAt);
    const daysSincePublished = (Date.now() - publishedDate.getTime()) / (1000 * 60 * 60 * 24);
    return daysSincePublished <= 7;
  };

  // Helper function to determine if a plan is "popular" (high view count)
  const isPopularPlan = (plan: DiscoverPlan) => {
    return plan.viewCount >= 50; // Plans with 50+ views are considered popular
  };

  // Helper function to determine if a plan is "trending" (highest view count)
  const isTrendingPlan = (plan: DiscoverPlan, allPlans: DiscoverPlan[]) => {
    if (allPlans.length === 0) return false;
    const sortedByViews = [...allPlans].sort((a, b) => b.viewCount - a.viewCount);
    const topViewCount = sortedByViews[0]?.viewCount || 0;
    return plan.viewCount >= topViewCount * 0.8 && plan.viewCount >= 100; // Top 20% with at least 100 views
  };

  // Filter and sort plans based on active tab
  const getFilteredPlans = () => {
    let filtered: DiscoverPlan[] = [];

    if (activeTab === 'friends') {
      filtered = friendPlans?.data || [];
    } else if (activeTab === 'myplans') {
      filtered = myPlans?.data || [];
    } else {
      const allPlans = marketplaceData?.data || [];

      if (activeTab === 'free') {
        filtered = allPlans.filter(plan => !plan.price || plan.price === 0);
      } else if (activeTab === 'premium') {
        filtered = allPlans.filter(plan => plan.price && plan.price > 0);
      } else {
        filtered = allPlans;
      }
    }

    // Apply sorting
    const sorted = [...filtered].sort((a, b) => {
      switch (sortBy) {
        case 'newest':
          if (!a.publishedAt) return 1;
          if (!b.publishedAt) return -1;
          return new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime();
        case 'popular':
          // Sort by view count descending
          return b.viewCount - a.viewCount;
        case 'mostViewed':
          return b.viewCount - a.viewCount;
        default:
          return 0;
      }
    });

    return sorted;
  };

  const hasActiveFilters = searchTerm || goalFilter;
  const plans = getFilteredPlans();
  const isLoading = activeTab === 'friends' ? isLoadingFriends :
                    activeTab === 'myplans' ? isLoadingMyPlans :
                    isLoadingMarketplace;
  const totalCount = activeTab === 'friends' ? friendPlans?.totalCount || 0 :
                     activeTab === 'myplans' ? myPlans?.totalCount || 0 :
                     plans.length;

  const renderPlanCard = (plan: DiscoverPlan, index: number) => {
    const isNew = isNewPlan(plan);
    const isPopular = isPopularPlan(plan);
    const isTrending = isTrendingPlan(plan, plans);

    return (
      <Card
        key={plan.id}
        className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
        style={{ animationDelay: `${index * 50}ms` }}
      >
        <div className="p-6 space-y-4">
          {/* Plan Header */}
          <div>
            <div className="flex items-start justify-between mb-2">
              <h3 className="font-bold text-lg flex-1">{plan.name}</h3>
              <div className="flex gap-2 flex-wrap justify-end">
                {plan.price && plan.price > 0 ? (
                  <Badge className="bg-gradient-to-r from-yellow-500 to-orange-500 text-white border-0">
                    <Crown className="h-3 w-3 mr-1" />
                    Premium
                  </Badge>
                ) : (
                  <Badge variant="secondary" className="bg-green-500/20 text-green-500 border-green-500/30">
                    <Gift className="h-3 w-3 mr-1" />
                    Grátis
                  </Badge>
                )}
              </div>
            </div>
            {/* Status Badges */}
            <div className="flex gap-2 mb-2 flex-wrap">
              {isTrending && (
                <Badge variant="outline" className="bg-red-500/10 text-red-500 border-red-500/30">
                  <Flame className="h-3 w-3 mr-1" />
                  Trending
                </Badge>
              )}
              {isPopular && !isTrending && (
                <Badge variant="outline" className="bg-blue-500/10 text-blue-500 border-blue-500/30">
                  <Star className="h-3 w-3 mr-1" />
                  Popular
                </Badge>
              )}
              {isNew && (
                <Badge variant="outline" className="bg-purple-500/10 text-purple-500 border-purple-500/30">
                  <Sparkles className="h-3 w-3 mr-1" />
                  Novo
                </Badge>
              )}
            </div>
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

        {/* Price and Actions */}
        <div className="pt-4 border-t border-border/50 space-y-3">
          <div className="flex items-center justify-between">
            <div>
              {plan.price && plan.price > 0 ? (
                <p className="text-2xl font-bold bg-gradient-to-r from-yellow-500 to-orange-500 bg-clip-text text-transparent">
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
              variant="outline"
              size="sm"
              onClick={() => router.push(`/plans/public/${plan.id}`)}
            >
              <Eye className="h-4 w-4 mr-2" />
              Ver Detalhes
            </Button>
          </div>
          <Button
            onClick={() => handlePurchaseClick(plan)}
            disabled={!user || purchaseMutation.isPending}
            className="w-full bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale"
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
    );
  };

  // Render card for "My Plans" tab (created + acquired plans)
  const renderMyPlanCard = (plan: any, index: number) => {
    const isOwnPlan = plan.ownerId === user?.id || plan.creatorId === user?.id;

    const handleDelete = (planId: string) => {
      if (confirm('Tem certeza que deseja excluir este plano?')) {
        deleteMutation.mutate(planId);
      }
    };

    return (
      <Card
        key={plan.id}
        className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
        style={{ animationDelay: `${index * 50}ms` }}
      >
        <div className="p-6 space-y-4">
          {/* Plan Header */}
          <div>
            <div className="flex items-start justify-between mb-2">
              <h3 className="font-bold text-lg flex-1">{plan.name}</h3>
              <div className="flex gap-2 flex-wrap justify-end">
                {isOwnPlan && (
                  <Badge variant="outline" className="bg-primary/10 text-primary border-primary/30">
                    <Edit className="h-3 w-3 mr-1" />
                    Meu Plano
                  </Badge>
                )}
                {!isOwnPlan && (
                  <Badge variant="outline" className="bg-blue-500/10 text-blue-500 border-blue-500/30">
                    <ShoppingCart className="h-3 w-3 mr-1" />
                    Adquirido
                  </Badge>
                )}
              </div>
            </div>
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
              <span className="font-medium">{plan.workoutCount || plan.workouts?.length || 0}</span>
            </div>
            {plan.viewCount !== undefined && (
              <div className="flex items-center gap-2 text-sm">
                <Eye className="h-4 w-4 text-primary" />
                <span className="text-muted-foreground">Visualizações:</span>
                <span className="font-medium">{plan.viewCount}</span>
              </div>
            )}
          </div>

          {/* Creator Info */}
          {!isOwnPlan && (
            <div className="pt-4 border-t border-border/50">
              <p className="text-sm text-muted-foreground">
                Criado por <span className="font-medium text-foreground">{plan.creatorName || plan.ownerName}</span>
              </p>
            </div>
          )}

          {/* Action Buttons */}
          <div className="pt-4 border-t border-border/50 space-y-2">
            {isOwnPlan ? (
              // Actions for own plans
              <>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => router.push(`/plans/${plan.id}/edit`)}
                    className="flex-1"
                  >
                    <Edit className="h-4 w-4 mr-2" />
                    Editar
                  </Button>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                      >
                        <Share2 className="h-4 w-4 mr-2" />
                        Compartilhar
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedPlan(plan);
                          setShareWithFriendsDialogOpen(true);
                        }}
                      >
                        <Send className="mr-2 h-4 w-4" />
                        Enviar para Amigos
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedPlan(plan);
                          setShareDialogOpen(true);
                        }}
                      >
                        <Settings className="mr-2 h-4 w-4" />
                        Configurações
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedPlan(plan);
                          setMarketplaceDialogOpen(true);
                        }}
                      >
                        <ShoppingCart className="mr-2 h-4 w-4" />
                        Marketplace
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => handleDelete(plan.id)}
                  disabled={deleteMutation.isPending}
                  className="w-full text-destructive hover:text-destructive hover:bg-destructive/10"
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Excluir Plano
                </Button>
              </>
            ) : (
              // Actions for acquired plans
              <Button
                size="sm"
                onClick={() => router.push(`/plans/${plan.id}`)}
                className="w-full"
              >
                <Eye className="h-4 w-4 mr-2" />
                Ver Plano
              </Button>
            )}
          </div>
        </div>
      </Card>
    );
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Sparkles className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Descobrir Planos
          </h1>
        </div>
        <p className="text-muted-foreground">
          Explore planos de treino criados por personal trainers profissionais e amigos
        </p>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={handleTabChange} className="w-full">
        <TabsList className="grid w-full grid-cols-2 sm:grid-cols-4 glass border border-primary/20">
          <TabsTrigger value="free" className="data-[state=active]:bg-green-500/20 data-[state=active]:text-green-500">
            <Gift className="h-4 w-4 mr-2" />
            Grátis
          </TabsTrigger>
          <TabsTrigger value="premium" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-yellow-500/20 data-[state=active]:to-orange-500/20">
            <Crown className="h-4 w-4 mr-2" />
            Premium
          </TabsTrigger>
          <TabsTrigger value="friends" className="data-[state=active]:bg-blue-500/20 data-[state=active]:text-blue-500">
            <Users className="h-4 w-4 mr-2" />
            Amigos
          </TabsTrigger>
          <TabsTrigger value="myplans" className="data-[state=active]:bg-primary/20 data-[state=active]:text-primary">
            <BookmarkCheck className="h-4 w-4 mr-2" />
            Meus Planos
          </TabsTrigger>
        </TabsList>

        {/* Search and Filters */}
        <Card className="glass border-primary/20 p-6 mt-6">
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

            {/* Filter Toggle and Sort */}
            <div className="flex items-center justify-between flex-wrap gap-3">
              <div className="flex gap-2">
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

              {/* Sort Dropdown */}
              {activeTab !== 'myplans' && (
                <div className="flex items-center gap-2">
                  <ArrowUpDown className="h-4 w-4 text-muted-foreground" />
                  <Select value={sortBy} onValueChange={(value) => setSortBy(value as SortOption)}>
                    <SelectTrigger className="w-[180px] glass">
                      <SelectValue placeholder="Ordenar por" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="newest">Mais Recentes</SelectItem>
                      <SelectItem value="popular">Mais Populares</SelectItem>
                      <SelectItem value="mostViewed">Mais Visualizados</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
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

        {/* Tab Contents */}
        <TabsContent value={activeTab} className="space-y-6 mt-6">
          {/* Results Info */}
          {!isLoading && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                {totalCount} {totalCount === 1 ? 'plano encontrado' : 'planos encontrados'}
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
              {plans.map((plan, index) =>
                activeTab === 'myplans' ? renderMyPlanCard(plan, index) : renderPlanCard(plan, index)
              )}
            </div>
          )}

          {/* Empty State */}
          {!isLoading && plans.length === 0 && (
            <Card className="glass border-primary/20 p-12 text-center">
              {activeTab === 'free' && <Gift className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />}
              {activeTab === 'premium' && <Crown className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />}
              {activeTab === 'friends' && <Users className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />}
              {activeTab === 'myplans' && <BookmarkCheck className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />}
              <h3 className="text-lg font-semibold mb-2">
                {activeTab === 'friends' ? 'Nenhum plano de amigos' :
                 activeTab === 'myplans' ? 'Nenhum plano adquirido' :
                 'Nenhum plano disponível'}
              </h3>
              <p className="text-muted-foreground mb-4">
                {hasActiveFilters
                  ? 'Tente ajustar seus filtros de busca'
                  : activeTab === 'friends'
                  ? 'Seus amigos ainda não compartilharam nenhum plano público'
                  : activeTab === 'myplans'
                  ? 'Você ainda não adquiriu nenhum plano de treino. Explore os planos disponíveis!'
                  : `Ainda não há planos ${activeTab === 'free' ? 'gratuitos' : 'premium'} disponíveis`}
              </p>
              {hasActiveFilters && (
                <Button onClick={clearFilters} variant="outline" className="hover-lift tap-scale">
                  Limpar Filtros
                </Button>
              )}
              {activeTab === 'myplans' && !hasActiveFilters && (
                <Button onClick={() => setActiveTab('free')} variant="outline" className="hover-lift tap-scale">
                  Explorar Planos Gratuitos
                </Button>
              )}
            </Card>
          )}

          {/* Pagination */}
          {!isLoading && marketplaceData && marketplaceData.totalPages > 1 && activeTab !== 'friends' && activeTab !== 'myplans' && (
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
                Faça login para adquirir planos de treino
              </p>
            </Card>
          )}
        </TabsContent>
      </Tabs>

      {/* Payment Info */}
      {activeTab === 'premium' && (
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
                Pagamentos integrados com Stripe estarão disponíveis em breve.
                Todos os pagamentos serão processados com segurança.
              </p>
            </div>
          </div>
        </Card>
      )}

      {/* Dialogs for plan management */}
      {selectedPlan && (
        <>
          <ShareSettingsDialog
            planId={selectedPlan.id}
            planName={selectedPlan.name}
            currentVisibility={(selectedPlan as any).visibilityLevel ?? 0}
            currentAllowCopying={(selectedPlan as any).allowCopying ?? true}
            open={shareDialogOpen}
            onOpenChange={setShareDialogOpen}
          />
          <ShareWithFriendsDialog
            planId={selectedPlan.id}
            planName={selectedPlan.name}
            open={shareWithFriendsDialogOpen}
            onOpenChange={setShareWithFriendsDialogOpen}
          />
          <MarketplaceSettingsDialog
            planId={selectedPlan.id}
            planName={selectedPlan.name}
            currentForSale={(selectedPlan as any).forSale ?? false}
            currentPrice={(selectedPlan as any).price ?? 0}
            isPublic={(selectedPlan as any).isPublic ?? false}
            open={marketplaceDialogOpen}
            onOpenChange={setMarketplaceDialogOpen}
          />
        </>
      )}
    </div>
  );
}
