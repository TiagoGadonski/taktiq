'use client';

import { useState, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Search, UserPlus } from 'lucide-react';
import { ClientList } from '@/components/instructor/client-list';
import { apiClient } from '@/lib/api';
import { useToast } from '@/hooks/use-toast';
import { cn } from '@/lib/utils';

type FilterType = 'all' | 'active' | 'inactive' | 'no-plan' | 'invited';

interface Client {
  id: string;
  name: string;
  email: string;
  profilePicture?: string;
  planCount: number;
  workoutCount: number;
  frequency: number;
  lastWorkout?: string;
  createdAt: string;
  status: 'active' | 'inactive' | 'invited';
  ptNotes?: string;
  latestAssessment?: any;
  activePlans?: any[];
}

export default function ClientsPage() {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<FilterType>('all');
  const [isInviteDialogOpen, setIsInviteDialogOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [isSendingInvite, setIsSendingInvite] = useState(false);

  // Fetch clients
  const { data: clients, isLoading } = useQuery({
    queryKey: ['clients'],
    queryFn: async () => {
      try {
        const response = await apiClient.get<any[]>('/personal/clients');

        // Transform API response to match Client interface
        return response.map((client: any) => ({
          id: client.id,
          name: client.name,
          email: client.email,
          profilePicture: client.profilePicture,
          planCount: client.activePlans?.length || 0,
          workoutCount: client.workoutCount || 0,
          frequency: client.frequency || 0,
          lastWorkout: client.lastWorkout,
          createdAt: client.createdAt,
          status: client.status || (client.activePlans?.length > 0 ? 'active' : 'inactive'),
          ptNotes: client.ptNotes,
          latestAssessment: client.latestAssessment,
          activePlans: client.activePlans,
        })) as Client[];
      } catch (error) {
        console.error('Failed to fetch clients:', error);
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Filter and search clients
  const filteredClients = useMemo(() => {
    if (!clients) return [];

    let filtered = clients;

    // Apply search query
    if (searchQuery) {
      filtered = filtered.filter(
        (client) =>
          client.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
          client.email.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Apply filter
    switch (activeFilter) {
      case 'active':
        filtered = filtered.filter((c) => c.planCount > 0 && c.status !== 'invited');
        break;
      case 'inactive':
        filtered = filtered.filter((c) => c.planCount === 0 && c.status !== 'invited');
        break;
      case 'no-plan':
        filtered = filtered.filter((c) => c.planCount === 0);
        break;
      case 'invited':
        filtered = filtered.filter((c) => c.status === 'invited');
        break;
      default:
        break;
    }

    return filtered;
  }, [clients, searchQuery, activeFilter]);

  // Count clients by filter
  const filterCounts = useMemo(() => {
    if (!clients) return { all: 0, active: 0, inactive: 0, noPlan: 0, invited: 0 };

    return {
      all: clients.length,
      active: clients.filter((c) => c.planCount > 0 && c.status !== 'invited').length,
      inactive: clients.filter((c) => c.planCount === 0 && c.status !== 'invited').length,
      noPlan: clients.filter((c) => c.planCount === 0).length,
      invited: clients.filter((c) => c.status === 'invited').length,
    };
  }, [clients]);

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
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    } catch (error: any) {
      console.error('Failed to send invite:', error);
      toast({
        title: 'Erro ao enviar convite',
        description: error.response?.data?.message || 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    } finally {
      setIsSendingInvite(false);
    }
  };

  const handleDeleteClient = (clientId: string) => {
    // TODO: Implement delete functionality
    console.log('Delete client:', clientId);
  };

  return (
    <div className="container mx-auto p-6 max-w-7xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            Clientes ({filterCounts.all})
          </h1>
          <p className="text-muted-foreground mt-1">
            Gerencie seus alunos e acompanhe o progresso
          </p>
        </div>
        <Button onClick={() => setIsInviteDialogOpen(true)}>
          <UserPlus className="h-4 w-4 mr-2" />
          Adicionar Cliente
        </Button>
      </div>

      {/* Search and Filters */}
      <div className="space-y-4">
        {/* Search Bar */}
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar por nome ou email..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>

        {/* Filter Chips */}
        <div className="flex items-center gap-2 flex-wrap">
          <button
            onClick={() => setActiveFilter('all')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'all'
                ? 'bg-primary text-primary-foreground'
                : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
            )}
          >
            Todos ({filterCounts.all})
          </button>
          <button
            onClick={() => setActiveFilter('active')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'active'
                ? 'bg-green-600 text-white dark:bg-green-700'
                : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
            )}
          >
            Ativos ({filterCounts.active})
          </button>
          <button
            onClick={() => setActiveFilter('inactive')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'inactive'
                ? 'bg-yellow-600 text-white dark:bg-yellow-700'
                : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
            )}
          >
            Inativos ({filterCounts.inactive})
          </button>
          <button
            onClick={() => setActiveFilter('no-plan')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'no-plan'
                ? 'bg-red-600 text-white dark:bg-red-700'
                : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
            )}
          >
            Sem Plano ({filterCounts.noPlan})
          </button>
          <button
            onClick={() => setActiveFilter('invited')}
            className={cn(
              'px-4 py-2 rounded-full text-sm font-medium transition-colors',
              activeFilter === 'invited'
                ? 'bg-blue-600 text-white dark:bg-blue-700'
                : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
            )}
          >
            Convidados ({filterCounts.invited})
          </button>
        </div>
      </div>

      {/* Client List */}
      {isLoading ? (
        <div className="text-center py-12">
          <p className="text-muted-foreground">Carregando clientes...</p>
        </div>
      ) : (
        <ClientList
          clients={filteredClients}
          onDeleteClient={handleDeleteClient}
        />
      )}

      {/* Results Count */}
      {!isLoading && filteredClients.length > 0 && (
        <p className="text-sm text-muted-foreground text-center">
          Mostrando {filteredClients.length} de {filterCounts.all} cliente(s)
        </p>
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
