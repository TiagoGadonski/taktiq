'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
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
  User,
  Edit,
  Save,
  Sparkles
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
  trainerNotes?: string;
}

interface LatestAssessment {
  id: string;
  assessmentType: string;
  assessmentDate: string;
  forwardHead?: string;
  roundedShoulders?: string;
  anteriorPelvicTilt?: string;
  kneeValgus?: string;
  bodyFatPercentage?: number;
  muscleMass?: number;
  flexibilityScore?: number;
  strengthScore?: number;
}

export default function ClientDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const clientId = params?.id as string;

  const [client, setClient] = useState<ClientDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditingNotes, setIsEditingNotes] = useState(false);
  const [trainerNotes, setTrainerNotes] = useState('');
  const [isSavingNotes, setIsSavingNotes] = useState(false);
  const [latestAssessment, setLatestAssessment] = useState<LatestAssessment | null>(null);

  useEffect(() => {
    fetchClientDetails();
    fetchLatestAssessment();
  }, [clientId]);

  const fetchClientDetails = async () => {
    try {
      setIsLoading(true);
      // Using the personal/clients endpoint that should return client details
      const response = await apiClient.get<any>(`/personal/clients`);
      const clientData = response.find((c: any) => c.id === clientId);

      if (clientData) {
        setClient(clientData);
        setTrainerNotes(clientData.trainerNotes || '');
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

  const handleSaveNotes = async () => {
    try {
      setIsSavingNotes(true);
      await apiClient.put(`/users/${clientId}`, {
        trainerNotes,
      });

      toast({
        title: 'Notas salvas!',
        description: 'Suas anotações sobre o cliente foram atualizadas.',
      });

      setIsEditingNotes(false);

      // Update local state
      if (client) {
        setClient({ ...client, trainerNotes });
      }
    } catch (error) {
      console.error('Failed to save trainer notes:', error);
      toast({
        title: 'Erro',
        description: 'Não foi possível salvar as anotações.',
        variant: 'destructive',
      });
    } finally {
      setIsSavingNotes(false);
    }
  };

  const fetchLatestAssessment = async () => {
    try {
      const assessments = await apiClient.get<LatestAssessment[]>(`/assessments/student/${clientId}`);
      if (assessments && assessments.length > 0) {
        // Get the most recent assessment
        const sorted = assessments.sort((a, b) =>
          new Date(b.assessmentDate).getTime() - new Date(a.assessmentDate).getTime()
        );
        setLatestAssessment(sorted[0]);
      }
    } catch (error) {
      console.error('Failed to fetch latest assessment:', error);
      // Don't show error toast - assessment is optional
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

      {/* Trainer Notes - Private notes about this client */}
      <Card className="bg-yellow-50 dark:bg-yellow-950/10 border-yellow-200 dark:border-yellow-900">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-yellow-900 dark:text-yellow-100 flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Anotações Privadas (PT)
            </CardTitle>
            {!isEditingNotes && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setIsEditingNotes(true)}
                className="text-yellow-900 dark:text-yellow-100"
              >
                <Edit className="h-4 w-4 mr-2" />
                Editar
              </Button>
            )}
          </div>
          <CardDescription className="text-yellow-800 dark:text-yellow-200">
            Suas anotações privadas sobre este cliente (apenas você pode ver)
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isEditingNotes ? (
            <div className="space-y-3">
              <Textarea
                value={trainerNotes}
                onChange={(e) => setTrainerNotes(e.target.value)}
                placeholder="Anote observações importantes: objetivos específicos, limitações, preferências, histórico médico relevante, progresso, ajustes necessários..."
                className="min-h-[150px] bg-white dark:bg-gray-900"
                maxLength={2000}
              />
              <div className="flex justify-between items-center">
                <p className="text-xs text-muted-foreground">
                  {trainerNotes.length}/2000 caracteres
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setTrainerNotes(client?.trainerNotes || '');
                      setIsEditingNotes(false);
                    }}
                    disabled={isSavingNotes}
                  >
                    Cancelar
                  </Button>
                  <Button
                    size="sm"
                    onClick={handleSaveNotes}
                    disabled={isSavingNotes}
                  >
                    <Save className="h-4 w-4 mr-2" />
                    {isSavingNotes ? 'Salvando...' : 'Salvar'}
                  </Button>
                </div>
              </div>
            </div>
          ) : (
            <div className="p-4 bg-white/50 dark:bg-gray-900/30 rounded-lg min-h-[100px]">
              {trainerNotes ? (
                <p className="text-yellow-900 dark:text-yellow-100 whitespace-pre-wrap">
                  {trainerNotes}
                </p>
              ) : (
                <p className="text-yellow-700 dark:text-yellow-300 italic">
                  Clique em &quot;Editar&quot; para adicionar suas anotações sobre este cliente...
                </p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Latest Assessment Summary */}
      {latestAssessment && (
        <Card className="bg-blue-50 dark:bg-blue-950/10 border-blue-200 dark:border-blue-900">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-blue-900 dark:text-blue-100 flex items-center gap-2">
                <Activity className="h-5 w-5" />
                Última Avaliação - {latestAssessment.assessmentType === 'Postural' ? 'Postural' : latestAssessment.assessmentType === 'Physical' ? 'Física' : 'Personalizada'}
              </CardTitle>
              <Badge variant="secondary">
                {new Date(latestAssessment.assessmentDate).toLocaleDateString('pt-BR')}
              </Badge>
            </div>
          </CardHeader>
          <CardContent>
            {latestAssessment.assessmentType === 'Postural' && (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                {latestAssessment.forwardHead && latestAssessment.forwardHead !== 'None' && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg">
                    <p className="text-xs text-blue-700 dark:text-blue-300 font-medium">Cabeça Anteriorizada</p>
                    <p className="text-sm font-semibold text-blue-900 dark:text-blue-100">{latestAssessment.forwardHead}</p>
                  </div>
                )}
                {latestAssessment.roundedShoulders && latestAssessment.roundedShoulders !== 'None' && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg">
                    <p className="text-xs text-blue-700 dark:text-blue-300 font-medium">Ombros Protusos</p>
                    <p className="text-sm font-semibold text-blue-900 dark:text-blue-100">{latestAssessment.roundedShoulders}</p>
                  </div>
                )}
                {latestAssessment.anteriorPelvicTilt && latestAssessment.anteriorPelvicTilt !== 'None' && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg">
                    <p className="text-xs text-blue-700 dark:text-blue-300 font-medium">Inclinação Pélvica Ant.</p>
                    <p className="text-sm font-semibold text-blue-900 dark:text-blue-100">{latestAssessment.anteriorPelvicTilt}</p>
                  </div>
                )}
                {latestAssessment.kneeValgus && latestAssessment.kneeValgus !== 'None' && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg">
                    <p className="text-xs text-blue-700 dark:text-blue-300 font-medium">Joelhos Valgos</p>
                    <p className="text-sm font-semibold text-blue-900 dark:text-blue-100">{latestAssessment.kneeValgus}</p>
                  </div>
                )}
              </div>
            )}
            {latestAssessment.assessmentType === 'Physical' && (
              <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
                {latestAssessment.bodyFatPercentage !== undefined && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg text-center">
                    <p className="text-2xl font-bold text-blue-900 dark:text-blue-100">{latestAssessment.bodyFatPercentage}%</p>
                    <p className="text-xs text-blue-700 dark:text-blue-300 mt-1">Gordura</p>
                  </div>
                )}
                {latestAssessment.muscleMass !== undefined && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg text-center">
                    <p className="text-2xl font-bold text-blue-900 dark:text-blue-100">{latestAssessment.muscleMass} kg</p>
                    <p className="text-xs text-blue-700 dark:text-blue-300 mt-1">Músculo</p>
                  </div>
                )}
                {latestAssessment.flexibilityScore !== undefined && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg text-center">
                    <p className="text-2xl font-bold text-blue-900 dark:text-blue-100">{latestAssessment.flexibilityScore}/10</p>
                    <p className="text-xs text-blue-700 dark:text-blue-300 mt-1">Flexibilidade</p>
                  </div>
                )}
                {latestAssessment.strengthScore !== undefined && (
                  <div className="p-3 bg-white/50 dark:bg-gray-900/30 rounded-lg text-center">
                    <p className="text-2xl font-bold text-blue-900 dark:text-blue-100">{latestAssessment.strengthScore}/10</p>
                    <p className="text-xs text-blue-700 dark:text-blue-300 mt-1">Força</p>
                  </div>
                )}
              </div>
            )}
            <div className="mt-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => router.push(`/instructor/clients/${clientId}/assessments`)}
                className="text-blue-900 dark:text-blue-100 border-blue-300 dark:border-blue-800"
              >
                Ver Todas as Avaliações
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

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
            onClick={() => router.push(`/instructor/clients/${clientId}/ai-assessment`)}
            className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
          >
            <Sparkles className="h-4 w-4 mr-2" />
            Avaliação Postural por IA
          </Button>
          <Button
            variant="outline"
            onClick={() => router.push(`/plans/new?studentId=${clientId}`)}
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
