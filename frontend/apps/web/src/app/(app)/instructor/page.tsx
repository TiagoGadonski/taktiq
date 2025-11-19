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
  Mail,
  Send,
  Clock,
  CheckCircle,
  XCircle,
  Globe,
  Instagram,
  Facebook,
  Save,
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';

interface Client {
  id: string;
  name: string;
  email: string;
  profilePictureUrl?: string;
  createdAt: string;
  workoutPlans: number;
  lastWorkout?: string;
}

interface Invitation {
  id: string;
  studentEmail: string;
  studentName?: string;
  workoutPlanId?: string;
  status: string;
  createdAt: string;
  expiresAt: string;
  activatedAt?: string;
  isExpired: boolean;
}

interface WorkoutPlan {
  id: string;
  name: string;
  goal: string;
}

interface PTProfile {
  profileSlug?: string;
  specialization?: string;
  education?: string;
  experience?: string;
  pricingInfo?: string;
  isPublicProfile: boolean;
  instagramUrl?: string;
  facebookUrl?: string;
  websiteUrl?: string;
}

export default function InstructorPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [clients, setClients] = useState<Client[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [notesDialogOpen, setNotesDialogOpen] = useState(false);
  const [addClientDialogOpen, setAddClientDialogOpen] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [clientNotes, setClientNotes] = useState('');
  const [clientEmail, setClientEmail] = useState('');
  const [isAddingClient, setIsAddingClient] = useState(false);

  // Invitation state
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [inviteDialogOpen, setInviteDialogOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteName, setInviteName] = useState('');
  const [selectedPlanId, setSelectedPlanId] = useState<string>('');
  const [workoutPlans, setWorkoutPlans] = useState<WorkoutPlan[]>([]);
  const [isSendingInvite, setIsSendingInvite] = useState(false);

  // Profile state
  const [profileSlug, setProfileSlug] = useState('');
  const [specialization, setSpecialization] = useState('');
  const [education, setEducation] = useState('');
  const [experience, setExperience] = useState('');
  const [pricingInfo, setPricingInfo] = useState('');
  const [isPublicProfile, setIsPublicProfile] = useState(false);
  const [instagramUrl, setInstagramUrl] = useState('');
  const [facebookUrl, setFacebookUrl] = useState('');
  const [websiteUrl, setWebsiteUrl] = useState('');
  const [isSavingProfile, setIsSavingProfile] = useState(false);

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
        const response = await apiClient.get<any>('/personal/clients');
        // Ensure we always set an array, even if response.data is undefined or null
        setClients(Array.isArray(response.data) ? response.data : []);
      } catch (error) {
        toast({
          title: 'Erro',
          description: 'Não foi possível carregar os clientes.',
          variant: 'destructive',
        });
        // Set to empty array on error to prevent undefined issues
        setClients([]);
      } finally {
        setIsLoading(false);
      }
    };

    if (user?.role === 'PersonalTrainer') {
      fetchClients();
    }
  }, [user, toast]);

  // Fetch invitations
  useEffect(() => {
    const fetchInvitations = async () => {
      try {
        const response = await apiClient.get<Invitation[]>('/personal/invitations');
        setInvitations(Array.isArray(response) ? response : []);
      } catch (error) {
        console.error('Error fetching invitations:', error);
        setInvitations([]);
      }
    };

    if (user?.role === 'PersonalTrainer') {
      fetchInvitations();
    }
  }, [user]);

  // Fetch workout plans for invitation dialog
  useEffect(() => {
    const fetchWorkoutPlans = async () => {
      try {
        const response = await apiClient.get<WorkoutPlan[]>('/workout-plans');
        setWorkoutPlans(Array.isArray(response) ? response : []);
      } catch (error) {
        console.error('Error fetching workout plans:', error);
        setWorkoutPlans([]);
      }
    };

    if (user?.role === 'PersonalTrainer' && inviteDialogOpen) {
      fetchWorkoutPlans();
    }
  }, [user, inviteDialogOpen]);

  // Fetch PT profile
  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await apiClient.get<any>('/me');
        if (response) {
          setProfileSlug(response.profileSlug || '');
          setSpecialization(response.specialization || '');
          setEducation(response.education || '');
          setExperience(response.experience || '');
          setPricingInfo(response.pricingInfo || '');
          setIsPublicProfile(response.isPublicProfile || false);
          setInstagramUrl(response.instagramUrl || '');
          setFacebookUrl(response.facebookUrl || '');
          setWebsiteUrl(response.websiteUrl || '');
        }
      } catch (error) {
        console.error('Error fetching profile:', error);
      }
    };

    if (user?.role === 'PersonalTrainer') {
      fetchProfile();
    }
  }, [user]);

  const filteredClients = (clients || []).filter((c) =>
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
      await apiClient.post(
        `/personal/clients/${selectedClient.id}/notes`,
        { notes: clientNotes }
      );

      toast({
        title: 'Sucesso',
        description: 'Notas salvas com sucesso.',
      });
      setNotesDialogOpen(false);
      setClientNotes('');
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível salvar as notas.',
        variant: 'destructive',
      });
    }
  };

  const handleAddClient = () => {
    setClientEmail('');
    setAddClientDialogOpen(true);
  };

  const handleSaveClient = async () => {
    if (!clientEmail.trim()) {
      toast({
        title: 'Erro',
        description: 'Por favor, insira o email do cliente.',
        variant: 'destructive',
      });
      return;
    }

    // Basic email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(clientEmail)) {
      toast({
        title: 'Erro',
        description: 'Por favor, insira um email válido.',
        variant: 'destructive',
      });
      return;
    }

    setIsAddingClient(true);

    try {
      await apiClient.post('/personal/clients', {
        ClientEmail: clientEmail.trim(),
      });

      toast({
        title: 'Sucesso',
        description: 'Cliente adicionado com sucesso.',
      });

      setAddClientDialogOpen(false);
      setClientEmail('');

      // Refresh client list
      const response = await apiClient.get<any>('/personal/clients');
      setClients(Array.isArray(response.data) ? response.data : []);
    } catch (error: any) {
      console.error('Error adding client:', error);
      console.error('Error response:', error.response?.data);

      const errorMessage = error.response?.data?.message ||
        error.response?.data?.detail ||
        error.response?.data?.title ||
        'Não foi possível adicionar o cliente.';

      toast({
        title: 'Erro',
        description: errorMessage,
        variant: 'destructive',
      });
    } finally {
      setIsAddingClient(false);
    }
  };

  const handleOpenInviteDialog = () => {
    setInviteEmail('');
    setInviteName('');
    setSelectedPlanId('');
    setInviteDialogOpen(true);
  };

  const handleSendInvite = async () => {
    if (!inviteEmail.trim()) {
      toast({
        title: 'Erro',
        description: 'Por favor, insira o email do aluno.',
        variant: 'destructive',
      });
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(inviteEmail)) {
      toast({
        title: 'Erro',
        description: 'Por favor, insira um email válido.',
        variant: 'destructive',
      });
      return;
    }

    setIsSendingInvite(true);

    try {
      await apiClient.post('/personal/invitations', {
        Email: inviteEmail.trim(),
        Name: inviteName.trim() || null,
        WorkoutPlanId: selectedPlanId || null,
      });

      toast({
        title: 'Convite enviado!',
        description: `Um email foi enviado para ${inviteEmail} com instruções para ativar a conta.`,
      });

      setInviteDialogOpen(false);
      setInviteEmail('');
      setInviteName('');
      setSelectedPlanId('');

      // Refresh invitations list
      const response = await apiClient.get<Invitation[]>('/personal/invitations');
      setInvitations(Array.isArray(response) ? response : []);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message ||
        'Não foi possível enviar o convite.';

      toast({
        title: 'Erro',
        description: errorMessage,
        variant: 'destructive',
      });
    } finally {
      setIsSendingInvite(false);
    }
  };

  const handleSaveProfile = async () => {
    // Validate slug format if provided
    if (profileSlug && !/^[a-z0-9-]+$/.test(profileSlug)) {
      toast({
        title: 'Erro',
        description: 'O URL deve conter apenas letras minúsculas, números e hífens',
        variant: 'destructive',
      });
      return;
    }

    setIsSavingProfile(true);

    try {
      await apiClient.put('/personal/profile', {
        ProfileSlug: profileSlug || null,
        Specialization: specialization || null,
        Education: education || null,
        Experience: experience || null,
        PricingInfo: pricingInfo || null,
        IsPublicProfile: isPublicProfile,
        InstagramUrl: instagramUrl || null,
        FacebookUrl: facebookUrl || null,
        WebsiteUrl: websiteUrl || null,
      });

      toast({
        title: 'Perfil atualizado!',
        description: 'Suas informações foram salvas com sucesso.',
      });
    } catch (error: any) {
      const errorMessage = error.response?.data?.message ||
        'Não foi possível atualizar o perfil.';

      toast({
        title: 'Erro',
        description: errorMessage,
        variant: 'destructive',
      });
    } finally {
      setIsSavingProfile(false);
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
          <div className="flex gap-2">
            <Button
              onClick={handleOpenInviteDialog}
              className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale"
            >
              <Mail className="mr-2 h-4 w-4" />
              Convidar Aluno
            </Button>
            <Button
              onClick={handleAddClient}
              variant="outline"
              className="hover-lift tap-scale"
            >
              <Plus className="mr-2 h-4 w-4" />
              Adicionar Cliente
            </Button>
          </div>
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
          <TabsTrigger value="invitations" className="tap-scale">
            <Mail className="mr-2 h-4 w-4" />
            Convites
            {invitations.filter(i => i.status === 'Pending').length > 0 && (
              <Badge className="ml-2 bg-primary/20 text-primary border-primary/30">
                {invitations.filter(i => i.status === 'Pending').length}
              </Badge>
            )}
          </TabsTrigger>
          <TabsTrigger value="plans" className="tap-scale">
            <Dumbbell className="mr-2 h-4 w-4" />
            Planos de Treino
          </TabsTrigger>
          <TabsTrigger value="progress" className="tap-scale">
            <TrendingUp className="mr-2 h-4 w-4" />
            Progresso
          </TabsTrigger>
          <TabsTrigger value="profile" className="tap-scale">
            <Globe className="mr-2 h-4 w-4" />
            Perfil Público
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
                          src={getAssetUrl(client.profilePictureUrl)}
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
              <Button
                onClick={handleAddClient}
                className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
              >
                <Plus className="mr-2 h-4 w-4" />
                Adicionar Cliente
              </Button>
            </div>
          )}
        </TabsContent>

        <TabsContent value="invitations" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {invitations.map((invitation, index) => (
              <Card
                key={invitation.id}
                className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
                style={{ animationDelay: `${index * 100}ms` }}
              >
                <div className="p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center gap-3">
                      <div className="p-3 bg-primary/20 rounded-lg">
                        <Mail className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <h3 className="font-semibold">{invitation.studentName || 'Novo Aluno'}</h3>
                        <p className="text-sm text-muted-foreground">
                          {invitation.studentEmail}
                        </p>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Status</span>
                      {invitation.status === 'Pending' && !invitation.isExpired && (
                        <Badge className="bg-yellow-500/20 text-yellow-500 border-yellow-500/30">
                          <Clock className="h-3 w-3 mr-1" />
                          Pendente
                        </Badge>
                      )}
                      {invitation.status === 'Activated' && (
                        <Badge className="bg-green-500/20 text-green-500 border-green-500/30">
                          <CheckCircle className="h-3 w-3 mr-1" />
                          Ativado
                        </Badge>
                      )}
                      {invitation.isExpired && (
                        <Badge className="bg-red-500/20 text-red-500 border-red-500/30">
                          <XCircle className="h-3 w-3 mr-1" />
                          Expirado
                        </Badge>
                      )}
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Enviado em</span>
                      <span className="font-medium">
                        {new Date(invitation.createdAt).toLocaleDateString('pt-BR')}
                      </span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Expira em</span>
                      <span className="font-medium">
                        {new Date(invitation.expiresAt).toLocaleDateString('pt-BR')}
                      </span>
                    </div>
                    {invitation.activatedAt && (
                      <div className="flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">Ativado em</span>
                        <span className="font-medium text-green-500">
                          {new Date(invitation.activatedAt).toLocaleDateString('pt-BR')}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              </Card>
            ))}
          </div>

          {invitations.length === 0 && (
            <div className="text-center py-12">
              <Mail className="h-12 w-12 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">
                Nenhum convite enviado
              </h3>
              <p className="text-muted-foreground mb-4">
                Convide novos alunos para começarem a treinar com você
              </p>
              <Button
                onClick={handleOpenInviteDialog}
                className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
              >
                <Mail className="mr-2 h-4 w-4" />
                Enviar Convite
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

        <TabsContent value="profile" className="space-y-4">
          <Card className="glass border-primary/20">
            <div className="p-6">
              <div className="flex items-start justify-between mb-6">
                <div>
                  <h3 className="text-xl font-semibold flex items-center gap-2">
                    <Globe className="h-5 w-5 text-primary" />
                    Perfil Público
                  </h3>
                  <p className="text-sm text-muted-foreground mt-1">
                    Configure seu perfil público para que alunos possam encontrar você
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Label htmlFor="isPublic" className="text-sm">Perfil Público</Label>
                  <input
                    type="checkbox"
                    id="isPublic"
                    checked={isPublicProfile}
                    onChange={(e) => setIsPublicProfile(e.target.checked)}
                    className="h-4 w-4 rounded border-input"
                  />
                </div>
              </div>

              <div className="space-y-6">
                {/* Profile URL Slug */}
                <div className="space-y-2">
                  <Label htmlFor="profileSlug">URL do Perfil</Label>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-muted-foreground">taktiq.app/trainer/</span>
                    <Input
                      id="profileSlug"
                      value={profileSlug}
                      onChange={(e) => setProfileSlug(e.target.value.toLowerCase())}
                      placeholder="seu-nome"
                      className="glass flex-1"
                      disabled={isSavingProfile}
                    />
                  </div>
                  <p className="text-xs text-muted-foreground">
                    Apenas letras minúsculas, números e hífens. Ex: tiago-cordeiro
                  </p>
                </div>

                {/* Specialization */}
                <div className="space-y-2">
                  <Label htmlFor="specialization">Especialização</Label>
                  <Input
                    id="specialization"
                    value={specialization}
                    onChange={(e) => setSpecialization(e.target.value)}
                    placeholder="Ex: Musculação, Funcional, Crossfit..."
                    className="glass"
                    disabled={isSavingProfile}
                  />
                  <p className="text-xs text-muted-foreground">
                    Suas áreas de especialização e expertise
                  </p>
                </div>

                {/* Education */}
                <div className="space-y-2">
                  <Label htmlFor="education">Formação e Certificações</Label>
                  <Textarea
                    id="education"
                    value={education}
                    onChange={(e) => setEducation(e.target.value)}
                    placeholder="Descreva sua formação acadêmica e certificações profissionais..."
                    className="glass min-h-[100px]"
                    disabled={isSavingProfile}
                  />
                </div>

                {/* Experience */}
                <div className="space-y-2">
                  <Label htmlFor="experience">Experiência Profissional</Label>
                  <Textarea
                    id="experience"
                    value={experience}
                    onChange={(e) => setExperience(e.target.value)}
                    placeholder="Conte sobre sua experiência e trajetória profissional..."
                    className="glass min-h-[100px]"
                    disabled={isSavingProfile}
                  />
                </div>

                {/* Pricing Info */}
                <div className="space-y-2">
                  <Label htmlFor="pricingInfo">Informações de Preços</Label>
                  <Textarea
                    id="pricingInfo"
                    value={pricingInfo}
                    onChange={(e) => setPricingInfo(e.target.value)}
                    placeholder="Descreva seus planos e valores (ex: Plano mensal: R$ 150, Plano trimestral: R$ 400...)"
                    className="glass min-h-[100px]"
                    disabled={isSavingProfile}
                  />
                </div>

                {/* Social Media Links */}
                <div className="border-t border-border/50 pt-6">
                  <h4 className="text-sm font-semibold mb-4">Redes Sociais e Contato</h4>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label htmlFor="instagramUrl" className="flex items-center gap-2">
                        <Instagram className="h-4 w-4 text-pink-500" />
                        Instagram
                      </Label>
                      <Input
                        id="instagramUrl"
                        value={instagramUrl}
                        onChange={(e) => setInstagramUrl(e.target.value)}
                        placeholder="https://instagram.com/seu-perfil"
                        className="glass"
                        disabled={isSavingProfile}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="facebookUrl" className="flex items-center gap-2">
                        <Facebook className="h-4 w-4 text-blue-500" />
                        Facebook
                      </Label>
                      <Input
                        id="facebookUrl"
                        value={facebookUrl}
                        onChange={(e) => setFacebookUrl(e.target.value)}
                        placeholder="https://facebook.com/seu-perfil"
                        className="glass"
                        disabled={isSavingProfile}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="websiteUrl" className="flex items-center gap-2">
                        <Globe className="h-4 w-4 text-primary" />
                        Site Pessoal
                      </Label>
                      <Input
                        id="websiteUrl"
                        value={websiteUrl}
                        onChange={(e) => setWebsiteUrl(e.target.value)}
                        placeholder="https://seusite.com"
                        className="glass"
                        disabled={isSavingProfile}
                      />
                    </div>
                  </div>
                </div>

                {/* Save Button */}
                <div className="border-t border-border/50 pt-6">
                  <Button
                    onClick={handleSaveProfile}
                    disabled={isSavingProfile}
                    className="w-full bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale"
                  >
                    {isSavingProfile ? (
                      <>
                        <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-background border-t-transparent" />
                        Salvando...
                      </>
                    ) : (
                      <>
                        <Save className="mr-2 h-4 w-4" />
                        Salvar Perfil
                      </>
                    )}
                  </Button>
                </div>

                {/* Preview Link */}
                {profileSlug && isPublicProfile && (
                  <div className="glass rounded-lg p-4 border border-primary/20">
                    <p className="text-sm text-muted-foreground mb-2">
                      Seu perfil público estará disponível em:
                    </p>
                    <Link
                      href={`/trainer/${profileSlug}`}
                      className="text-primary font-medium hover:underline flex items-center gap-2"
                    >
                      <Globe className="h-4 w-4" />
                      taktiq.app/trainer/{profileSlug}
                    </Link>
                  </div>
                )}
              </div>
            </div>
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

      {/* Add Client Dialog */}
      <Dialog open={addClientDialogOpen} onOpenChange={setAddClientDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle>Adicionar Cliente</DialogTitle>
            <DialogDescription>
              Insira o email do cliente que deseja adicionar à sua lista
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="clientEmail">Email do Cliente</Label>
              <Input
                id="clientEmail"
                type="email"
                placeholder="cliente@exemplo.com"
                value={clientEmail}
                onChange={(e) => setClientEmail(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !isAddingClient) {
                    handleSaveClient();
                  }
                }}
                className="glass"
                disabled={isAddingClient}
              />
              <p className="text-sm text-muted-foreground">
                O cliente deve estar cadastrado no sistema e ter a função de Aluno
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setAddClientDialogOpen(false)}
              className="hover-lift tap-scale"
              disabled={isAddingClient}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSaveClient}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
              disabled={isAddingClient}
            >
              {isAddingClient ? (
                <>
                  <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-background border-t-transparent" />
                  Adicionando...
                </>
              ) : (
                <>
                  <Plus className="mr-2 h-4 w-4" />
                  Adicionar
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Invite Student Dialog */}
      <Dialog open={inviteDialogOpen} onOpenChange={setInviteDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Mail className="h-5 w-5 text-primary" />
              Convidar Novo Aluno
            </DialogTitle>
            <DialogDescription>
              Envie um convite por email para um aluno que ainda não possui conta na plataforma
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="inviteEmail">
                Email do Aluno <span className="text-destructive">*</span>
              </Label>
              <Input
                id="inviteEmail"
                type="email"
                placeholder="aluno@exemplo.com"
                value={inviteEmail}
                onChange={(e) => setInviteEmail(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !isSendingInvite) {
                    handleSendInvite();
                  }
                }}
                className="glass"
                disabled={isSendingInvite}
              />
              <p className="text-xs text-muted-foreground">
                Um email será enviado com um link para ativação da conta
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="inviteName">Nome do Aluno (opcional)</Label>
              <Input
                id="inviteName"
                type="text"
                placeholder="Nome do aluno"
                value={inviteName}
                onChange={(e) => setInviteName(e.target.value)}
                className="glass"
                disabled={isSendingInvite}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="workoutPlan">Plano de Treino (opcional)</Label>
              <Select
                value={selectedPlanId}
                onValueChange={setSelectedPlanId}
                disabled={isSendingInvite}
              >
                <SelectTrigger className="glass">
                  <SelectValue placeholder="Selecione um plano (opcional)" />
                </SelectTrigger>
                <SelectContent className="glass">
                  <SelectItem value="">Nenhum plano</SelectItem>
                  {workoutPlans.map((plan) => (
                    <SelectItem key={plan.id} value={plan.id}>
                      {plan.name} - {plan.goal}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground">
                O plano selecionado será copiado para o aluno ao ativar a conta
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setInviteDialogOpen(false)}
              className="hover-lift tap-scale"
              disabled={isSendingInvite}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSendInvite}
              className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale"
              disabled={isSendingInvite}
            >
              {isSendingInvite ? (
                <>
                  <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-background border-t-transparent" />
                  Enviando...
                </>
              ) : (
                <>
                  <Send className="mr-2 h-4 w-4" />
                  Enviar Convite
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
