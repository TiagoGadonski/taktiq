'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/hooks/use-auth';
import { useToast } from '@/hooks/use-toast';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { QuickActionsCard } from '@/components/instructor/quick-actions-card';
import { ActivityFeed } from '@/components/instructor/activity-feed';
import {
  Plus,
  TrendingUp,
  Mail,
  Users,
  Dumbbell,
  Clock,
  DollarSign,
  AlertCircle
} from 'lucide-react';
import { apiClient, api } from '@/lib/api';
import { useQuery } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface DashboardStats {
  totalClients: number;
  activePlans: number;
  pendingInvites: number;
  monthlyRevenue: number;
}

interface ActivityItem {
  id: string;
  type: 'workout_completed' | 'check_in' | 'alert';
  clientName: string;
  message: string;
  timestamp: Date;
  urgent?: boolean;
}

export default function InstructorDashboard() {
  const router = useRouter();
  const { user } = useAuth();
  const { toast } = useToast();
  const [isInviteDialogOpen, setIsInviteDialogOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [isSendingInvite, setIsSendingInvite] = useState(false);

  // Fetch dashboard stats from analytics API
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: async () => {
      try {
        const data = await api.analytics.getDashboard();
        // Map analytics response to DashboardStats structure
        return {
          totalClients: data.totalClients || 0,
          activePlans: data.activePlans || 0,
          pendingInvites: data.pendingInvites || 0,
          monthlyRevenue: data.monthlyRevenue || 0,
        } as DashboardStats;
      } catch (error) {
        // Fallback to zeros if API fails
        return {
          totalClients: 0,
          activePlans: 0,
          pendingInvites: 0,
          monthlyRevenue: 0,
        } as DashboardStats;
      }
    },
    staleTime: 2 * 60 * 1000, // 2 minutes
  });

  // Fetch recent activity
  const { data: activities } = useQuery({
    queryKey: ['recent-activity'],
    queryFn: async () => {
      try {
        const data = await apiClient.get<ActivityItem[]>('/personal/dashboard/recent-activity');
        return data;
      } catch (error) {
        return [] as ActivityItem[];
      }
    },
    staleTime: 1 * 60 * 1000, // 1 minute
  });

  const handleSendInvite = async () => {
    if (!inviteEmail || !inviteEmail.includes('@')) {
      toast({
        title: 'Email inválido',
        description: 'Por favor, insira um email válido.',
        variant: 'destructive',
      });
      return;
    }

    setIsSendingInvite(true);
    try {
      await apiClient.post('/personal/send-invite', {
        studentEmail: inviteEmail,
      });

      toast({
        title: 'Convite enviado!',
        description: `Convite enviado para ${inviteEmail}`,
      });

      setInviteEmail('');
      setIsInviteDialogOpen(false);
    } catch (error: any) {
      toast({
        title: 'Erro ao enviar convite',
        description: error.response?.data?.message || 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    } finally {
      setIsSendingInvite(false);
    }
  };

  return (
    <div className="container mx-auto p-6 max-w-7xl space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            Bem-vindo, {user?.name?.split(' ')[0] || 'Personal'} 👋
          </h1>
          <p className="text-muted-foreground mt-1">
            Gerencie seus clientes e impulsione seus resultados
          </p>
        </div>
        <Button onClick={() => setIsInviteDialogOpen(true)}>
          <Mail className="h-4 w-4 mr-2" />
          Convidar Aluno
        </Button>
      </div>

      {/* Quick Actions */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Ações Rápidas</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <QuickActionsCard
            title="Criar Plano"
            description="Crie um novo plano de treino para seus alunos"
            icon={Plus}
            variant="primary"
            onClick={() => router.push('/plans/new')}
          />
          <QuickActionsCard
            title="Acompanhar Progresso"
            description="Veja a evolução e atividades dos clientes"
            icon={TrendingUp}
            variant="secondary"
            onClick={() => router.push('/instructor/clients')}
          />
          <QuickActionsCard
            title="Convidar Aluno"
            description="Envie um convite para um novo aluno"
            icon={Mail}
            variant="default"
            onClick={() => setIsInviteDialogOpen(true)}
          />
        </div>
      </div>

      {/* Essential Metrics */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Métricas Essenciais</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Total Clients */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <Users className="h-4 w-4" />
                Clientes
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {statsLoading ? '...' : stats?.totalClients || 0}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Total de clientes
              </p>
            </CardContent>
          </Card>

          {/* Active Plans */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <Dumbbell className="h-4 w-4" />
                Planos
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {statsLoading ? '...' : stats?.activePlans || 0}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Planos ativos
              </p>
            </CardContent>
          </Card>

          {/* Pending Invites */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <Clock className="h-4 w-4" />
                Pendentes
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                {statsLoading ? '...' : stats?.pendingInvites || 0}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Convites pendentes
              </p>
            </CardContent>
          </Card>

          {/* Monthly Revenue */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium flex items-center gap-2 text-muted-foreground">
                <DollarSign className="h-4 w-4" />
                Faturamento
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold">
                R$ {statsLoading ? '...' : (stats?.monthlyRevenue || 0).toFixed(2)}
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Este mês
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Activity Feed */}
      <ActivityFeed
        activities={activities || []}
        onViewAll={() => router.push('/instructor/clients')}
      />

      {/* Pending Invites Alert */}
      {stats && stats.pendingInvites > 0 && (
        <Alert className="border-orange-200 bg-orange-50">
          <AlertCircle className="h-4 w-4 text-orange-600" />
          <AlertDescription>
            <strong>⚠️ {stats.pendingInvites} convite(s) pendente(s)</strong>
            {' '}aguardando aprovação.{' '}
            <Button
              variant="link"
              className="p-0 h-auto text-orange-700"
              onClick={() => router.push('/instructor/clients?filter=invited')}
            >
              Gerenciar Convites →
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Invite Dialog */}
      <Dialog open={isInviteDialogOpen} onOpenChange={setIsInviteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Convidar Novo Aluno</DialogTitle>
            <DialogDescription>
              Envie um convite por email para um novo aluno se juntar ao seu time.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label htmlFor="email">Email do Aluno</Label>
              <Input
                id="email"
                type="email"
                placeholder="aluno@example.com"
                value={inviteEmail}
                onChange={(e) => setInviteEmail(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    handleSendInvite();
                  }
                }}
              />
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsInviteDialogOpen(false)}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSendInvite}
              disabled={isSendingInvite}
            >
              {isSendingInvite ? 'Enviando...' : 'Enviar Convite'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
