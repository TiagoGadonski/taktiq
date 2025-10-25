'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Users, Search, UserPlus, Check, X, UserMinus, Loader2 } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

// Types
interface Friend {
  friendshipId: string;
  friendId: string;
  friendName: string;
  friendEmail: string;
}

interface FriendRequest {
  friendshipId: string;
  requesterId: string;
  requesterName: string;
  requesterEmail: string;
  createdAt: string;
}

interface SearchUser {
  id: string;
  name: string;
  email: string;
  isFriend: boolean;
  hasPendingRequest: boolean;
}

export default function FriendsPage() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const queryClient = useQueryClient();

  // Fetch friends list
  const { data: friends = [], isLoading: loadingFriends } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: async () => {
      return apiClient.get('/friends');
    },
  });

  // Fetch pending requests
  const { data: pendingRequests = [], isLoading: loadingRequests } = useQuery<FriendRequest[]>({
    queryKey: ['friend-requests'],
    queryFn: async () => {
      return apiClient.get('/friends/requests/pending');
    },
  });

  // Search users mutation
  const searchMutation = useMutation({
    mutationFn: async (query: string) => {
      return apiClient.get<SearchUser[]>(`/friends/search?query=${encodeURIComponent(query)}`);
    },
    onSuccess: (data) => {
      setSearchResults(data);
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao buscar usuários',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  // Send friend request mutation
  const sendRequestMutation = useMutation({
    mutationFn: async (addresseeId: string) => {
      return apiClient.post(`/friends/requests/${addresseeId}`, {});
    },
    onSuccess: () => {
      toast({
        title: 'Pedido enviado!',
        description: 'Seu pedido de amizade foi enviado com sucesso.',
      });
      queryClient.invalidateQueries({ queryKey: ['friend-requests'] });
      // Re-search to update button states
      if (searchQuery) {
        searchMutation.mutate(searchQuery);
      }
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao enviar pedido',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  // Accept friend request mutation
  const acceptRequestMutation = useMutation({
    mutationFn: async (friendshipId: string) => {
      return apiClient.put(`/friends/requests/${friendshipId}`, { accept: true });
    },
    onSuccess: () => {
      toast({
        title: 'Pedido aceito!',
        description: 'Agora vocês são amigos.',
      });
      queryClient.invalidateQueries({ queryKey: ['friends'] });
      queryClient.invalidateQueries({ queryKey: ['friend-requests'] });
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao aceitar pedido',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  // Decline friend request mutation
  const declineRequestMutation = useMutation({
    mutationFn: async (friendshipId: string) => {
      return apiClient.put(`/friends/requests/${friendshipId}`, { accept: false });
    },
    onSuccess: () => {
      toast({
        title: 'Pedido recusado',
        description: 'O pedido foi recusado.',
      });
      queryClient.invalidateQueries({ queryKey: ['friend-requests'] });
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao recusar pedido',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  // Remove friend mutation
  const removeFriendMutation = useMutation({
    mutationFn: async (friendshipId: string) => {
      return apiClient.delete(`/friends/${friendshipId}`);
    },
    onSuccess: () => {
      toast({
        title: 'Amigo removido',
        description: 'A amizade foi removida.',
      });
      queryClient.invalidateQueries({ queryKey: ['friends'] });
    },
    onError: (error) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao remover amigo',
        description: error instanceof Error ? error.message : 'Tente novamente',
      });
    },
  });

  const handleSearch = () => {
    if (searchQuery.trim().length < 2) {
      toast({
        variant: 'destructive',
        title: 'Digite pelo menos 2 caracteres',
        description: 'Digite pelo menos 2 caracteres para buscar',
      });
      return;
    }
    searchMutation.mutate(searchQuery);
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <div>
        <h1 className="text-2xl font-bold sm:text-3xl flex items-center gap-2">
          <Users className="h-7 w-7 sm:h-8 sm:w-8 text-primary" />
          Amigos
        </h1>
        <p className="text-sm text-muted-foreground sm:text-base mt-1">
          Conecte-se com amigos e compartilhe seus treinos
        </p>
      </div>

      <Tabs defaultValue="friends" className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="friends" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            Amigos
            {friends.length > 0 && (
              <Badge variant="secondary" className="ml-1">{friends.length}</Badge>
            )}
          </TabsTrigger>
          <TabsTrigger value="requests" className="flex items-center gap-2">
            <UserPlus className="h-4 w-4" />
            Pedidos
            {pendingRequests.length > 0 && (
              <Badge variant="destructive" className="ml-1">{pendingRequests.length}</Badge>
            )}
          </TabsTrigger>
          <TabsTrigger value="search" className="flex items-center gap-2">
            <Search className="h-4 w-4" />
            Buscar
          </TabsTrigger>
        </TabsList>

        {/* Friends List Tab */}
        <TabsContent value="friends" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Meus Amigos</CardTitle>
              <CardDescription>
                {friends.length === 0
                  ? 'Você ainda não tem amigos. Use a busca para adicionar!'
                  : `Você tem ${friends.length} ${friends.length === 1 ? 'amigo' : 'amigos'}`}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {loadingFriends ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
              ) : friends.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <Users className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>Nenhum amigo ainda</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {friends.map((friend) => (
                    <Card
                      key={friend.friendshipId}
                      className="border cursor-pointer hover:bg-accent/50 transition-colors"
                    >
                      <CardContent className="flex items-center justify-between p-4">
                        <div
                          className="flex items-center gap-3 flex-1"
                          onClick={() => router.push(`/users/${friend.friendId}`)}
                        >
                          <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                            <span className="text-sm font-semibold text-primary">
                              {friend.friendName.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <div>
                            <p className="font-medium">{friend.friendName}</p>
                            <p className="text-sm text-muted-foreground">{friend.friendEmail}</p>
                          </div>
                        </div>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={(e) => {
                            e.stopPropagation();
                            removeFriendMutation.mutate(friend.friendshipId);
                          }}
                          disabled={removeFriendMutation.isPending}
                        >
                          <UserMinus className="h-4 w-4 mr-2" />
                          Remover
                        </Button>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Pending Requests Tab */}
        <TabsContent value="requests" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Pedidos Pendentes</CardTitle>
              <CardDescription>
                {pendingRequests.length === 0
                  ? 'Você não tem pedidos de amizade pendentes'
                  : `${pendingRequests.length} ${pendingRequests.length === 1 ? 'pedido pendente' : 'pedidos pendentes'}`}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {loadingRequests ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
              ) : pendingRequests.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">
                  <UserPlus className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>Nenhum pedido pendente</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {pendingRequests.map((request) => (
                    <Card key={request.friendshipId} className="border">
                      <CardContent className="flex items-center justify-between p-4">
                        <div className="flex items-center gap-3">
                          <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                            <span className="text-sm font-semibold text-primary">
                              {request.requesterName.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <div>
                            <p className="font-medium">{request.requesterName}</p>
                            <p className="text-sm text-muted-foreground">{request.requesterEmail}</p>
                          </div>
                        </div>
                        <div className="flex gap-2">
                          <Button
                            size="sm"
                            onClick={() => acceptRequestMutation.mutate(request.friendshipId)}
                            disabled={acceptRequestMutation.isPending || declineRequestMutation.isPending}
                          >
                            <Check className="h-4 w-4 mr-2" />
                            Aceitar
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => declineRequestMutation.mutate(request.friendshipId)}
                            disabled={acceptRequestMutation.isPending || declineRequestMutation.isPending}
                          >
                            <X className="h-4 w-4 mr-2" />
                            Recusar
                          </Button>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Search Tab */}
        <TabsContent value="search" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Buscar Usuários</CardTitle>
              <CardDescription>
                Busque por nome ou email para adicionar novos amigos
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex gap-2">
                <Input
                  placeholder="Digite o nome ou email..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  disabled={searchMutation.isPending}
                />
                <Button
                  onClick={handleSearch}
                  disabled={searchMutation.isPending}
                >
                  {searchMutation.isPending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <>
                      <Search className="h-4 w-4 mr-2" />
                      Buscar
                    </>
                  )}
                </Button>
              </div>

              {searchResults.length > 0 && (
                <div className="space-y-3 mt-4">
                  {searchResults.map((user) => (
                    <Card
                      key={user.id}
                      className="border cursor-pointer hover:bg-accent/50 transition-colors"
                    >
                      <CardContent className="flex items-center justify-between p-4">
                        <div
                          className="flex items-center gap-3 flex-1"
                          onClick={() => router.push(`/users/${user.id}`)}
                        >
                          <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                            <span className="text-sm font-semibold text-primary">
                              {user.name.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <div>
                            <p className="font-medium">{user.name}</p>
                            <p className="text-sm text-muted-foreground">{user.email}</p>
                          </div>
                        </div>
                        <div onClick={(e) => e.stopPropagation()}>
                          {user.isFriend ? (
                            <Badge variant="secondary">Já é amigo</Badge>
                          ) : user.hasPendingRequest ? (
                            <Badge variant="outline">Pedido enviado</Badge>
                          ) : (
                            <Button
                              size="sm"
                              onClick={() => sendRequestMutation.mutate(user.id)}
                              disabled={sendRequestMutation.isPending}
                            >
                              <UserPlus className="h-4 w-4 mr-2" />
                              Adicionar
                            </Button>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}

              {searchQuery && !searchMutation.isPending && searchResults.length === 0 && searchMutation.isSuccess && (
                <div className="text-center py-8 text-muted-foreground">
                  <Search className="h-12 w-12 mx-auto mb-4 opacity-50" />
                  <p>Nenhum usuário encontrado</p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
