'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trophy, Target, Users, Calendar, Info } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface Challenge {
  id: string;
  creatorId: string;
  title: string;
  type: string;
  targetValue: number;
  currentValue: number;
  startDate: string;
  endDate: string;
  status: string;
  targetType: number;
  isDefault: boolean;
  isParticipating: boolean;
  progresses: ChallengeProgress[];
}

interface ChallengeProgress {
  participantId: string;
  participantName?: string;
  currentValue: number;
  lastUpdate: string;
}

interface Client {
  id: string;
  name: string;
  email: string;
}

export function InstructorChallenges() {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [challengeTitle, setChallengeTitle] = useState('');
  const [challengeType, setChallengeType] = useState('');
  const [targetValue, setTargetValue] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedClients, setSelectedClients] = useState<string[]>([]);
  const [activeTab, setActiveTab] = useState<'my' | 'client'>('my');

  const queryClient = useQueryClient();

  // Fetch all challenges
  const { data: challenges = [], isLoading: loadingChallenges } = useQuery<Challenge[]>({
    queryKey: ['challenges', 'all'],
    queryFn: async () => {
      return apiClient.get('/challenges/all');
    },
  });

  // Fetch clients
  const { data: clients = [] } = useQuery<Client[]>({
    queryKey: ['instructor-clients'],
    queryFn: async () => {
      return apiClient.get('/personal/clients');
    },
  });

  // Filter challenges: my challenges vs challenges I created for clients
  const myChallenges = challenges.filter(
    (c) => c.isParticipating && !c.progresses.some((p) => p.participantId !== c.creatorId)
  );

  const clientChallenges = challenges.filter(
    (c) => c.progresses.some((p) => clients.some((client) => client.id === p.participantId))
  );

  // Create challenge mutation
  const createChallengeMutation = useMutation({
    mutationFn: async (data: {
      title: string;
      type: string;
      targetValue: number;
      startDate: string;
      endDate: string;
      friendIds: string[];
    }) => {
      return apiClient.post('/challenges/custom', data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['challenges'] });
      toast({
        title: 'Desafio criado!',
        description: 'O desafio foi criado e seus clientes foram notificados.',
      });
      setIsCreateDialogOpen(false);
      resetForm();
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao criar desafio',
        description: error?.response?.data?.message || 'Não foi possível criar o desafio.',
      });
    },
  });

  const resetForm = () => {
    setChallengeTitle('');
    setChallengeType('');
    setTargetValue('');
    setStartDate('');
    setEndDate('');
    setSelectedClients([]);
  };

  const handleCreateChallenge = () => {
    if (!challengeTitle || !challengeType || !targetValue || !startDate || !endDate) {
      toast({
        variant: 'destructive',
        title: 'Campos obrigatórios',
        description: 'Por favor, preencha todos os campos.',
      });
      return;
    }

    if (selectedClients.length === 0) {
      toast({
        variant: 'destructive',
        title: 'Selecione clientes',
        description: 'Selecione pelo menos um cliente para atribuir este desafio.',
      });
      return;
    }

    createChallengeMutation.mutate({
      title: challengeTitle,
      type: challengeType,
      targetValue: parseInt(targetValue),
      startDate,
      endDate,
      friendIds: selectedClients,
    });
  };

  const getStatusBadge = (challenge: Challenge) => {
    if (challenge.status === 'Completed') {
      return <Badge className="bg-green-500/20 text-green-500">Completo</Badge>;
    }
    if (new Date(challenge.endDate) < new Date()) {
      return <Badge variant="destructive">Expirado</Badge>;
    }
    return <Badge className="bg-blue-500/20 text-blue-500">Ativo</Badge>;
  };

  const getProgressPercentage = (challenge: Challenge) => {
    return Math.min(100, (challenge.currentValue / challenge.targetValue) * 100);
  };

  const renderChallengeCard = (challenge: Challenge) => (
    <Card key={challenge.id} className="glass border-primary/20 hover-lift tap-scale">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="h-12 w-12 rounded-full bg-primary/20 flex items-center justify-center">
              <Trophy className="h-6 w-6 text-primary" />
            </div>
            <div>
              <CardTitle className="text-lg">{challenge.title}</CardTitle>
              <p className="text-sm text-muted-foreground">{challenge.type}</p>
            </div>
          </div>
          {getStatusBadge(challenge)}
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <div className="flex justify-between text-sm mb-2">
            <span>Progresso</span>
            <span className="font-medium">
              {challenge.currentValue} / {challenge.targetValue}
            </span>
          </div>
          <div className="w-full bg-muted rounded-full h-2">
            <div
              className="bg-primary h-2 rounded-full transition-all"
              style={{ width: `${getProgressPercentage(challenge)}%` }}
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-muted-foreground">Início</p>
            <p className="font-medium">
              {new Date(challenge.startDate).toLocaleDateString('pt-BR')}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Fim</p>
            <p className="font-medium">
              {new Date(challenge.endDate).toLocaleDateString('pt-BR')}
            </p>
          </div>
        </div>

        {challenge.progresses && challenge.progresses.length > 0 && (
          <div>
            <p className="text-sm text-muted-foreground mb-2">Participantes</p>
            <div className="space-y-1">
              {challenge.progresses.map((progress, idx) => (
                <div
                  key={idx}
                  className="flex items-center justify-between p-2 rounded bg-muted/50 text-sm"
                >
                  <span>{progress.participantName || 'Participante'}</span>
                  <span className="font-medium">
                    {progress.currentValue} / {challenge.targetValue}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Desafios</h1>
          <p className="text-muted-foreground">
            Crie e gerencie desafios para seus clientes
          </p>
        </div>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Novo Desafio
        </Button>
      </div>

      <Alert className="border-primary/20 bg-primary/5">
        <Info className="h-4 w-4" />
        <AlertDescription>
          Como instrutor, você pode criar desafios personalizados para motivar seus clientes a atingir objetivos específicos.
        </AlertDescription>
      </Alert>

      <Tabs value={activeTab} onValueChange={(value) => setActiveTab(value as 'my' | 'client')}>
        <TabsList className="glass">
          <TabsTrigger value="my" className="tap-scale">
            <Target className="mr-2 h-4 w-4" />
            Meus Desafios
          </TabsTrigger>
          <TabsTrigger value="client" className="tap-scale">
            <Users className="mr-2 h-4 w-4" />
            Desafios dos Clientes
            {clientChallenges.length > 0 && (
              <Badge className="ml-2 bg-primary/20">{clientChallenges.length}</Badge>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="my" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {myChallenges.map(renderChallengeCard)}
          </div>
          {myChallenges.length === 0 && (
            <Card className="glass border-primary/20 p-12 text-center">
              <Trophy className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">
                Nenhum desafio pessoal
              </h3>
              <p className="text-muted-foreground">
                Crie um desafio para você mesmo ou participe de desafios da comunidade
              </p>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="client" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {clientChallenges.map(renderChallengeCard)}
          </div>
          {clientChallenges.length === 0 && (
            <Card className="glass border-primary/20 p-12 text-center">
              <Users className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">
                Nenhum desafio para clientes
              </h3>
              <p className="text-muted-foreground mb-4">
                Crie desafios personalizados para motivar seus clientes
              </p>
              <Button onClick={() => setIsCreateDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Criar Desafio para Clientes
              </Button>
            </Card>
          )}
        </TabsContent>
      </Tabs>

      {/* Create Challenge Dialog */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Criar Novo Desafio</DialogTitle>
            <DialogDescription>
              Crie um desafio personalizado para seus clientes
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="title">Título do Desafio</Label>
              <Input
                id="title"
                value={challengeTitle}
                onChange={(e) => setChallengeTitle(e.target.value)}
                placeholder="Ex: Desafio 30 Dias de Treino"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="type">Tipo de Desafio</Label>
              <Input
                id="type"
                value={challengeType}
                onChange={(e) => setChallengeType(e.target.value)}
                placeholder="Ex: Treinos Completos, Volume Total, etc."
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="target">Meta</Label>
              <Input
                id="target"
                type="number"
                value={targetValue}
                onChange={(e) => setTargetValue(e.target.value)}
                placeholder="Ex: 30 (para 30 treinos)"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="startDate">Data de Início</Label>
                <Input
                  id="startDate"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="endDate">Data de Término</Label>
                <Input
                  id="endDate"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label>Selecionar Clientes</Label>
              <div className="border rounded-lg p-4 max-h-60 overflow-y-auto space-y-2">
                {clients.length === 0 ? (
                  <p className="text-sm text-muted-foreground text-center py-4">
                    Você ainda não tem clientes. Convide novos alunos para criar desafios.
                  </p>
                ) : (
                  clients.map((client) => (
                    <div key={client.id} className="flex items-center space-x-2">
                      <Checkbox
                        id={client.id}
                        checked={selectedClients.includes(client.id)}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setSelectedClients([...selectedClients, client.id]);
                          } else {
                            setSelectedClients(
                              selectedClients.filter((id) => id !== client.id)
                            );
                          }
                        }}
                      />
                      <Label
                        htmlFor={client.id}
                        className="text-sm font-normal cursor-pointer flex-1"
                      >
                        <div>
                          <p className="font-medium">{client.name}</p>
                          <p className="text-xs text-muted-foreground">{client.email}</p>
                        </div>
                      </Label>
                    </div>
                  ))
                )}
              </div>
              {selectedClients.length > 0 && (
                <p className="text-sm text-muted-foreground">
                  {selectedClients.length} cliente(s) selecionado(s)
                </p>
              )}
            </div>
          </div>

          <div className="flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() => {
                setIsCreateDialogOpen(false);
                resetForm();
              }}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleCreateChallenge}
              disabled={createChallengeMutation.isPending}
            >
              {createChallengeMutation.isPending ? 'Criando...' : 'Criar Desafio'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
