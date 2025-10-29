'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  UserCog,
  Users,
  Plus,
  Search,
  TrendingUp,
  Calendar,
  FileText,
  Dumbbell,
  MoreVertical,
  Eye,
  ClipboardEdit,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useToast } from '@/hooks/use-toast';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

interface Client {
  id: string;
  name: string;
  email: string;
  profilePictureUrl?: string;
  createdAt: string;
  workoutPlans: number;
  lastWorkout?: string;
}

export default function InstructorPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [clients, setClients] = useState<Client[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [notesDialogOpen, setNotesDialogOpen] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [clientNotes, setClientNotes] = useState('');

  // Redirect if not personal trainer
  useEffect(() => {
    if (user && user.role !== 'PersonalTrainer') {
      router.push('/dashboard');
      toast({
        title: 'Acesso negado',
        description: 'Você não tem permissão para acessar esta página.',
        variant: 'destructive',
      });
    }
  }, [user, router, toast]);

  // Fetch clients
  useEffect(() => {
    const fetchClients = async () => {
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(
          'https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/personal/clients',
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        if (response.ok) {
          const data = await response.json();
          setClients(data);
        }
      } catch (error) {
        toast({
          title: 'Erro',
          description: 'Não foi possível carregar os clientes.',
          variant: 'destructive',
        });
      } finally {
        setIsLoading(false);
      }
    };

    if (user?.role === 'PersonalTrainer') {
      fetchClients();
    }
  }, [user, toast]);

  const filteredClients = clients.filter((c) =>
    c.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    c.email.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAddNotes = (client: Client) => {
    setSelectedClient(client);
    setNotesDialogOpen(true);
  };

  const handleSaveNotes = async () => {
    if (!selectedClient) return;

    try {
      const token = localStorage.getItem('token');
      const response = await fetch(
        `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/personal/clients/${selectedClient.id}/notes`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({ notes: clientNotes }),
        }
      );

      if (response.ok) {
        toast({
          title: 'Sucesso',
          description: 'Notas salvas com sucesso.',
        });
        setNotesDialogOpen(false);
        setClientNotes('');
      }
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível salvar as notas.',
        variant: 'destructive',
      });
    }
  };

  if (user?.role !== 'PersonalTrainer') {
    return null;
  }

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
          <p className="text-muted-foreground">Carregando...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <UserCog className="h-8 w-8 text-primary animate-glow-pulse" />
              <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
                Painel do Instrutor
              </h1>
            </div>
            <p className="text-muted-foreground">
              Gerencie seus clientes, treinos e progresso
            </p>
          </div>
          <Button className="bg-primary hover:bg-primary/90 hover-lift tap-scale">
            <Plus className="mr-2 h-4 w-4" />
            Adicionar Cliente
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card className="glass hover-lift tap-scale p-6 border-primary/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-primary/20 rounded-lg">
              <Users className="h-6 w-6 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total de Clientes</p>
              <p className="text-2xl font-bold text-primary">{clients.length}</p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-blue-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-blue-500/20 rounded-lg">
              <Dumbbell className="h-6 w-6 text-blue-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Planos Ativos</p>
              <p className="text-2xl font-bold text-blue-500">
                {clients.reduce((acc, c) => acc + c.workoutPlans, 0)}
              </p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-green-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-green-500/20 rounded-lg">
              <TrendingUp className="h-6 w-6 text-green-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Progresso Médio</p>
              <p className="text-2xl font-bold text-green-500">+12%</p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-orange-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-orange-500/20 rounded-lg">
              <Calendar className="h-6 w-6 text-orange-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Treinos Hoje</p>
              <p className="text-2xl font-bold text-orange-500">
                {clients.filter((c) => c.lastWorkout === new Date().toISOString().split('T')[0]).length}
              </p>
            </div>
          </div>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="clients" className="space-y-4">
        <TabsList className="glass">
          <TabsTrigger value="clients" className="tap-scale">
            <Users className="mr-2 h-4 w-4" />
            Meus Clientes
          </TabsTrigger>
          <TabsTrigger value="plans" className="tap-scale">
            <Dumbbell className="mr-2 h-4 w-4" />
            Planos de Treino
          </TabsTrigger>
          <TabsTrigger value="progress" className="tap-scale">
            <TrendingUp className="mr-2 h-4 w-4" />
            Progresso
          </TabsTrigger>
        </TabsList>

        <TabsContent value="clients" className="space-y-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar clientes por nome ou email..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 glass border-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Clients Grid */}
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {filteredClients.map((client, index) => (
              <Card
                key={client.id}
                className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
                style={{ animationDelay: `${index * 100}ms` }}
              >
                <div className="p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center gap-3">
                      <Avatar className="h-12 w-12 ring-2 ring-primary/30">
                        <AvatarImage
                          src={
                            client.profilePictureUrl
                              ? `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net${client.profilePictureUrl}`
                              : undefined
                          }
                        />
                        <AvatarFallback className="bg-primary/20 text-primary font-bold">
                          {client.name.charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <h3 className="font-semibold">{client.name}</h3>
                        <p className="text-sm text-muted-foreground">
                          {client.email}
                        </p>
                      </div>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon" className="hover-lift">
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="glass">
                        <DropdownMenuItem
                          className="cursor-pointer"
                          onClick={() =>
                            router.push(`/users/${client.id}`)
                          }
                        >
                          <Eye className="mr-2 h-4 w-4" />
                          Ver Perfil
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="cursor-pointer"
                          onClick={() => handleAddNotes(client)}
                        >
                          <FileText className="mr-2 h-4 w-4" />
                          Adicionar Notas
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="cursor-pointer"
                          onClick={() =>
                            router.push(`/plans/new?clientId=${client.id}`)
                          }
                        >
                          <ClipboardEdit className="mr-2 h-4 w-4" />
                          Criar Plano
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Planos Ativos</span>
                      <Badge className="bg-primary/20 text-primary border-primary/30">
                        {client.workoutPlans}
                      </Badge>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Último Treino</span>
                      <span className="font-medium">
                        {client.lastWorkout
                          ? new Date(client.lastWorkout).toLocaleDateString('pt-BR')
                          : 'Nunca'}
                      </span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Cliente desde</span>
                      <span className="font-medium">
                        {new Date(client.createdAt).toLocaleDateString('pt-BR')}
                      </span>
                    </div>
                  </div>

                  <div className="mt-4 pt-4 border-t border-border/50">
                    <Link href={`/users/${client.id}`}>
                      <Button className="w-full bg-primary hover:bg-primary/90 hover-lift tap-scale">
                        <Eye className="mr-2 h-4 w-4" />
                        Ver Detalhes
                      </Button>
                    </Link>
                  </div>
                </div>
              </Card>
            ))}
          </div>

          {filteredClients.length === 0 && (
            <div className="text-center py-12">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">
                Nenhum cliente encontrado
              </h3>
              <p className="text-muted-foreground mb-4">
                {searchTerm
                  ? 'Tente ajustar sua pesquisa'
                  : 'Comece adicionando seu primeiro cliente'}
              </p>
              <Button className="bg-primary hover:bg-primary/90 hover-lift tap-scale">
                <Plus className="mr-2 h-4 w-4" />
                Adicionar Cliente
              </Button>
            </div>
          )}
        </TabsContent>

        <TabsContent value="plans">
          <Card className="glass border-primary/20 p-12 text-center">
            <Dumbbell className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
            <h3 className="text-lg font-semibold mb-2">
              Gestão de Planos de Treino
            </h3>
            <p className="text-muted-foreground mb-4">
              Visualize e gerencie todos os planos de treino dos seus clientes
            </p>
            <Link href="/plans">
              <Button className="bg-primary hover:bg-primary/90 hover-lift tap-scale">
                Ver Todos os Planos
              </Button>
            </Link>
          </Card>
        </TabsContent>

        <TabsContent value="progress">
          <Card className="glass border-primary/20 p-12 text-center">
            <TrendingUp className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
            <h3 className="text-lg font-semibold mb-2">
              Monitoramento de Progresso
            </h3>
            <p className="text-muted-foreground mb-4">
              Acompanhe o progresso e evolução dos seus clientes
            </p>
            <Link href="/progress">
              <Button className="bg-primary hover:bg-primary/90 hover-lift tap-scale">
                Ver Progresso
              </Button>
            </Link>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Notes Dialog */}
      <Dialog open={notesDialogOpen} onOpenChange={setNotesDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle>Adicionar Notas</DialogTitle>
            <DialogDescription>
              Adicione observações sobre {selectedClient?.name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="notes">Notas</Label>
              <Textarea
                id="notes"
                placeholder="Ex: Cliente demonstrou boa forma nos agachamentos. Aumentar carga na próxima sessão..."
                value={clientNotes}
                onChange={(e) => setClientNotes(e.target.value)}
                className="glass min-h-[150px]"
              />
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setNotesDialogOpen(false)}
              className="hover-lift tap-scale"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSaveNotes}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
            >
              Salvar Notas
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
