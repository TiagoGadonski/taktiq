'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trophy, Target, TrendingUp, Users, Calendar, Loader2, UserPlus } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';

// Types
interface Challenge {
  id: string;
  title: string;
  type: string;
  targetValue: number;
  startDate: string;
  endDate: string;
  status: string;
  progresses: ChallengeProgress[];
}

interface ChallengeProgress {
  participantId: string;
  participantName?: string;
  currentValue: number;
  lastUpdate: string;
}

interface Friend {
  friendshipId: string;
  friendId: string;
  friendName: string;
  friendEmail: string;
}

export default function ChallengesPage() {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [challengeTitle, setChallengeTitle] = useState('');
  const [challengeType, setChallengeType] = useState('');
  const [targetValue, setTargetValue] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedFriends, setSelectedFriends] = useState<string[]>([]);

  const queryClient = useQueryClient();

  // Fetch challenges
  const { data: challenges = [], isLoading: loadingChallenges } = useQuery<Challenge[]>({
    queryKey: ['challenges'],
    queryFn: async () => {
      return apiClient.get('/challenges');
    },
  });

  // Fetch friends for invite
  const { data: friends = [] } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: async () => {
      return apiClient.get('/friends');
    },
  });

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
      if (data.friendIds.length > 0) {
        return apiClient.post('/challenges/custom', data);
      } else {
        return apiClient.post('/challenges', {
          title: data.title,
          type: data.type,
          targetValue: data.targetValue,
          startDate: data.startDate,
          endDate: data.endDate,
        });
      }
    },
    onSuccess: () => {
      toast({
        title: 'Desafio criado!',
        description: 'Seu desafio foi criado com sucesso.',
      });
      queryClient.invalidateQueries({ queryKey: ['challenges'] });
      setIsCreateDialogOpen(false);
      resetForm();
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao criar desafio',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  const resetForm = () => {
    setChallengeTitle('');
    setChallengeType('');
    setTargetValue('');
    setStartDate('');
    setEndDate('');
    setSelectedFriends([]);
  };

  const handleCreateChallenge = () => {
    if (!challengeTitle || !challengeType || !targetValue || !startDate || !endDate) {
      toast({
        variant: 'destructive',
        title: 'Preencha todos os campos',
        description: 'Todos os campos são obrigatórios',
      });
      return;
    }

    const target = parseFloat(targetValue);
    if (isNaN(target) || target <= 0) {
      toast({
        variant: 'destructive',
        title: 'Valor inválido',
        description: 'O valor da meta deve ser um número positivo',
      });
      return;
    }

    createChallengeMutation.mutate({
      title: challengeTitle,
      type: challengeType,
      targetValue: target,
      startDate,
      endDate,
      friendIds: selectedFriends,
    });
  };

  const toggleFriendSelection = (friendId: string) => {
    setSelectedFriends((prev) =>
      prev.includes(friendId) ? prev.filter((id) => id !== friendId) : [...prev, friendId]
    );
  };

  const getProgressPercentage = (challenge: Challenge) => {
    if (!challenge.progresses || challenge.progresses.length === 0) return 0;
    const myProgress = challenge.progresses[0]; // Assuming first is current user
    return Math.min(100, (myProgress.currentValue / challenge.targetValue) * 100);
  };

  const getDaysRemaining = (endDate: string) => {
    const end = new Date(endDate);
    const now = new Date();
    const diff = end.getTime() - now.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
      case 'ativo':
        return 'bg-blue-500';
      case 'completed':
      case 'concluído':
        return 'bg-green-500';
      case 'failed':
      case 'falhou':
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold sm:text-3xl flex items-center gap-2">
            <Trophy className="h-7 w-7 sm:h-8 sm:w-8 text-primary" />
            Desafios
          </h1>
          <p className="text-sm text-muted-foreground sm:text-base mt-1">
            Crie desafios e compita com amigos
          </p>
        </div>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Criar Desafio
        </Button>
      </div>

      {/* Challenges Grid */}
      {loadingChallenges ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : challenges.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <div className="mx-auto max-w-md space-y-4">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-muted">
                <Trophy className="h-10 w-10 text-muted-foreground" />
              </div>
              <h3 className="text-xl font-semibold">Nenhum desafio criado</h3>
              <p className="text-muted-foreground">
                Crie desafios personalizados para manter sua motivação e competir com amigos
              </p>
              <Button onClick={() => setIsCreateDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Criar Primeiro Desafio
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {challenges.map((challenge) => {
            const progress = getProgressPercentage(challenge);
            const daysRemaining = getDaysRemaining(challenge.endDate);
            const participants = challenge.progresses?.length || 0;

            return (
              <Card key={challenge.id} className="border-2">
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <CardTitle className="flex items-center gap-2">
                      <Trophy className="h-5 w-5 text-primary" />
                      {challenge.title}
                    </CardTitle>
                    <Badge className={getStatusColor(challenge.status)}>
                      {challenge.status}
                    </Badge>
                  </div>
                  <CardDescription className="flex items-center gap-2">
                    <Target className="h-4 w-4" />
                    {challenge.type}
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Progress Bar */}
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Progresso</span>
                      <span className="font-medium">{progress.toFixed(0)}%</span>
                    </div>
                    <div className="h-2 w-full overflow-hidden rounded-full bg-secondary">
                      <div
                        className="h-full bg-primary transition-all"
                        style={{ width: `${progress}%` }}
                      />
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="space-y-2 text-sm">
                    {challenge.progresses && challenge.progresses.length > 0 && (
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Atual:</span>
                        <span className="font-medium">
                          {challenge.progresses[0].currentValue} / {challenge.targetValue}
                        </span>
                      </div>
                    )}
                    {daysRemaining > 0 && (
                      <div className="flex justify-between">
                        <span className="text-muted-foreground flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          Dias restantes:
                        </span>
                        <span className="font-medium">{daysRemaining}</span>
                      </div>
                    )}
                    {participants > 1 && (
                      <div className="flex justify-between">
                        <span className="text-muted-foreground flex items-center gap-1">
                          <Users className="h-3 w-3" />
                          Participantes:
                        </span>
                        <span className="font-medium">{participants}</span>
                      </div>
                    )}
                  </div>

                  {/* Participants Progress */}
                  {challenge.progresses && challenge.progresses.length > 1 && (
                    <div className="pt-2 space-y-2">
                      <p className="text-sm font-medium">Ranking:</p>
                      <div className="space-y-1">
                        {challenge.progresses
                          .sort((a, b) => b.currentValue - a.currentValue)
                          .map((progress, index) => (
                            <div key={progress.participantId} className="flex items-center justify-between text-sm">
                              <span className="flex items-center gap-2">
                                <Badge variant={index === 0 ? 'default' : 'outline'} className="text-xs">
                                  {index + 1}º
                                </Badge>
                                <span className="text-muted-foreground">
                                  {progress.participantName || 'Participante'}
                                </span>
                              </span>
                              <span className="font-medium">{progress.currentValue}</span>
                            </div>
                          ))}
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* Create Challenge Dialog */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Trophy className="h-5 w-5 text-primary" />
              Criar Novo Desafio
            </DialogTitle>
            <DialogDescription>
              Crie um desafio pessoal ou convide amigos para competir juntos
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 mt-4">
            <div className="space-y-2">
              <Label htmlFor="title">Título do Desafio</Label>
              <Input
                id="title"
                placeholder="Ex: Correr 50km este mês"
                value={challengeTitle}
                onChange={(e) => setChallengeTitle(e.target.value)}
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="type">Tipo</Label>
                <Input
                  id="type"
                  placeholder="Ex: Corrida, Musculação, etc."
                  value={challengeType}
                  onChange={(e) => setChallengeType(e.target.value)}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="target">Meta (valor numérico)</Label>
                <Input
                  id="target"
                  type="number"
                  placeholder="Ex: 50"
                  value={targetValue}
                  onChange={(e) => setTargetValue(e.target.value)}
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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

            {/* Invite Friends Section */}
            {friends.length > 0 && (
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <UserPlus className="h-4 w-4" />
                  Convidar Amigos (Opcional)
                </Label>
                <Card>
                  <CardContent className="pt-6">
                    <div className="space-y-3 max-h-48 overflow-y-auto">
                      {friends.map((friend) => (
                        <div key={friend.friendId} className="flex items-center space-x-2">
                          <Checkbox
                            id={friend.friendId}
                            checked={selectedFriends.includes(friend.friendId)}
                            onCheckedChange={() => toggleFriendSelection(friend.friendId)}
                          />
                          <label
                            htmlFor={friend.friendId}
                            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer flex items-center gap-2"
                          >
                            <div className="h-6 w-6 rounded-full bg-primary/10 flex items-center justify-center">
                              <span className="text-xs font-semibold text-primary">
                                {friend.friendName.charAt(0).toUpperCase()}
                              </span>
                            </div>
                            {friend.friendName}
                          </label>
                        </div>
                      ))}
                    </div>
                    {selectedFriends.length > 0 && (
                      <p className="text-sm text-muted-foreground mt-3">
                        {selectedFriends.length} {selectedFriends.length === 1 ? 'amigo selecionado' : 'amigos selecionados'}
                      </p>
                    )}
                  </CardContent>
                </Card>
              </div>
            )}

            <div className="flex gap-2 pt-4">
              <Button
                variant="outline"
                className="flex-1"
                onClick={() => setIsCreateDialogOpen(false)}
              >
                Cancelar
              </Button>
              <Button
                className="flex-1"
                onClick={handleCreateChallenge}
                disabled={createChallengeMutation.isPending}
              >
                {createChallengeMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Criando...
                  </>
                ) : (
                  <>
                    <Trophy className="mr-2 h-4 w-4" />
                    Criar Desafio
                  </>
                )}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
