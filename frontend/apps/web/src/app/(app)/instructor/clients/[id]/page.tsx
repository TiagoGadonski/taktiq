'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import {
  ArrowLeft,
  FileText,
  TrendingUp,
  BarChart3,
  Plus,
  Dumbbell,
  Mail,
  Calendar,
  Activity,
  User
} from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { getAssetUrl } from '@/lib/env';

interface ClientDetail {
  id: string;
  name: string;
  email: string;
  profilePictureUrl?: string;
  createdAt: string;
  workoutPlans: number;
  lastWorkout?: string;
  role: string;
}

export default function ClientDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const clientId = params?.id as string;

  const [client, setClient] = useState<ClientDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchClientDetails();
  }, [clientId]);

  const fetchClientDetails = async () => {
    try {
      setIsLoading(true);
      // Using the personal/clients endpoint that should return client details
      const response = await apiClient.get<any>(`/personal/clients`);
      const clientData = response.find((c: any) => c.id === clientId);

      if (clientData) {
        setClient(clientData);
      } else {
        toast({
          title: 'Cliente não encontrado',
          description: 'Não foi possível carregar os dados do cliente.',
          variant: 'destructive',
        });
        router.back();
      }
    } catch (error) {
      console.error('Failed to fetch client details:', error);
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar os dados do cliente.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Activity className="h-12 w-12 animate-pulse mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando dados do cliente...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!client) {
    return null;
  }

  return (
    <div className="container mx-auto p-6 max-w-6xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.push('/instructor')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div className="flex items-center gap-4">
            <Avatar className="h-16 w-16">
              <AvatarImage
                src={client.profilePictureUrl ? getAssetUrl(client.profilePictureUrl) : undefined}
              />
              <AvatarFallback>
                {client.name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)}
              </AvatarFallback>
            </Avatar>
            <div>
              <h1 className="text-3xl font-bold">{client.name}</h1>
              <div className="flex items-center gap-2 mt-1">
                <Mail className="h-4 w-4 text-muted-foreground" />
                <p className="text-muted-foreground">{client.email}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Dumbbell className="h-4 w-4" />
              Planos Ativos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{client.workoutPlans || 0}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Planos de treino atribuídos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Último Treino
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {client.lastWorkout
                ? new Date(client.lastWorkout).toLocaleDateString('pt-BR')
                : '—'}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {client.lastWorkout ? 'Data do último treino' : 'Nenhum treino registrado'}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <User className="h-4 w-4" />
              Cliente Desde
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {client.createdAt && !isNaN(Date.parse(client.createdAt))
                ? new Date(client.createdAt).toLocaleDateString('pt-BR')
                : '—'}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Data de cadastro
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Main Actions */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Avaliações */}
        <Card className="hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => router.push(`/instructor/clients/${clientId}/assessments`)}>
          <CardHeader>
            <div className="flex items-center justify-between">
              <FileText className="h-8 w-8 text-blue-500" />
              <Badge variant="secondary">Novo</Badge>
            </div>
            <CardTitle className="mt-4">Avaliações</CardTitle>
            <CardDescription>
              Avaliações posturais, físicas e personalizadas
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button className="w-full" variant="outline">
              <FileText className="h-4 w-4 mr-2" />
              Gerenciar Avaliações
            </Button>
          </CardContent>
        </Card>

        {/* Evolução */}
        <Card className="hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => router.push(`/instructor/clients/${clientId}/evolution`)}>
          <CardHeader>
            <div className="flex items-center justify-between">
              <TrendingUp className="h-8 w-8 text-green-500" />
              <Badge variant="secondary">Gráficos</Badge>
            </div>
            <CardTitle className="mt-4">Evolução</CardTitle>
            <CardDescription>
              Acompanhe o progresso temporal do aluno
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button className="w-full" variant="outline">
              <TrendingUp className="h-4 w-4 mr-2" />
              Ver Evolução
            </Button>
          </CardContent>
        </Card>

        {/* Estatísticas */}
        <Card className="hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => router.push(`/instructor/clients/${clientId}/stats`)}>
          <CardHeader>
            <div className="flex items-center justify-between">
              <BarChart3 className="h-8 w-8 text-purple-500" />
              <Badge variant="secondary">Analytics</Badge>
            </div>
            <CardTitle className="mt-4">Estatísticas</CardTitle>
            <CardDescription>
              Frequência, feedback e relatórios
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button className="w-full" variant="outline">
              <BarChart3 className="h-4 w-4 mr-2" />
              Ver Estatísticas
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Ações Rápidas</CardTitle>
          <CardDescription>Atalhos para operações comuns</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-3">
          <Button
            onClick={() => router.push(`/instructor/clients/${clientId}/assessments/new`)}
          >
            <Plus className="h-4 w-4 mr-2" />
            Nova Avaliação
          </Button>
          <Button
            variant="outline"
            onClick={() => router.push('/plans/new')}
          >
            <Dumbbell className="h-4 w-4 mr-2" />
            Criar Plano de Treino
          </Button>
          <Button
            variant="outline"
            onClick={() => router.push(`/users/${clientId}`)}
          >
            <User className="h-4 w-4 mr-2" />
            Ver Perfil Completo
          </Button>
        </CardContent>
      </Card>

      {/* Information Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card className="bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-900">
          <CardHeader>
            <CardTitle className="text-blue-900 dark:text-blue-100 flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Sistema de Avaliações
            </CardTitle>
          </CardHeader>
          <CardContent className="text-blue-900 dark:text-blue-100 space-y-2">
            <p className="text-sm">
              <strong>Avaliação Postural:</strong> Identifique desvios posturais (cabeça anteriorizada, ombros protusos, etc.)
            </p>
            <p className="text-sm">
              <strong>Avaliação Física:</strong> Registre % de gordura, massa muscular, scores de condicionamento
            </p>
            <p className="text-sm">
              <strong>Personalizada:</strong> Crie campos customizados para suas necessidades específicas
            </p>
            <p className="text-sm mt-3 font-semibold">
              💡 A AI usará automaticamente as avaliações para sugerir exercícios corretivos!
            </p>
          </CardContent>
        </Card>

        <Card className="bg-green-50 dark:bg-green-950/20 border-green-200 dark:border-green-900">
          <CardHeader>
            <CardTitle className="text-green-900 dark:text-green-100 flex items-center gap-2">
              <TrendingUp className="h-5 w-5" />
              Gráficos de Evolução
            </CardTitle>
          </CardHeader>
          <CardContent className="text-green-900 dark:text-green-100 space-y-2">
            <p className="text-sm">
              <strong>Composição Corporal:</strong> Acompanhe % de gordura e massa muscular ao longo do tempo
            </p>
            <p className="text-sm">
              <strong>Performance:</strong> Veja a evolução dos scores de flexibilidade, força e cardio
            </p>
            <p className="text-sm">
              <strong>Insights Automáticos:</strong> O sistema destaca ganhos e melhorias significativas
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
