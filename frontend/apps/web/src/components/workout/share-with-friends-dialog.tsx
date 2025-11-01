'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Send, UserPlus, Loader2 } from 'lucide-react';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/use-toast';
import { Card, CardContent } from '@/components/ui/card';

interface Friend {
  friendshipId: string;
  friendId: string;
  friendName: string;
  friendEmail: string;
}

interface ShareWithFriendsDialogProps {
  planId: string;
  planName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ShareWithFriendsDialog({
  planId,
  planName,
  open,
  onOpenChange,
}: ShareWithFriendsDialogProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [selectedFriends, setSelectedFriends] = useState<string[]>([]);

  // Fetch friends list
  const { data: friends = [], isLoading: loadingFriends } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: async () => {
      return apiClient.get('/friends');
    },
    enabled: open, // Only fetch when dialog is open
  });

  // Share with friends mutation
  const sharePlanMutation = useMutation({
    mutationFn: async (friendIds: string[]) => {
      return apiClient.post(`/workout-plans/${planId}/share`, {
        friendIds,
      });
    },
    onSuccess: () => {
      toast({
        title: 'Plano compartilhado!',
        description: `"${planName}" foi compartilhado com ${selectedFriends.length} ${selectedFriends.length === 1 ? 'amigo' : 'amigos'}.`,
      });
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      setSelectedFriends([]);
      onOpenChange(false);
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao compartilhar',
        description: error.response?.data?.message || 'Não foi possível compartilhar o plano.',
      });
    },
  });

  const toggleFriendSelection = (friendId: string) => {
    setSelectedFriends((prev) =>
      prev.includes(friendId) ? prev.filter((id) => id !== friendId) : [...prev, friendId]
    );
  };

  const handleShare = () => {
    if (selectedFriends.length === 0) {
      toast({
        variant: 'destructive',
        title: 'Selecione amigos',
        description: 'Você precisa selecionar pelo menos um amigo para compartilhar.',
      });
      return;
    }

    sharePlanMutation.mutate(selectedFriends);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Send className="h-5 w-5 text-primary" />
            Compartilhar com Amigos
          </DialogTitle>
          <DialogDescription>
            Envie &quot;{planName}&quot; diretamente para seus amigos
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          {loadingFriends ? (
            <div className="flex justify-center py-8">
              <Loader2 className="h-6 w-6 animate-spin text-primary" />
            </div>
          ) : friends.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center">
                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-muted mb-4">
                  <UserPlus className="h-8 w-8 text-muted-foreground" />
                </div>
                <h3 className="font-semibold mb-2">Nenhum amigo encontrado</h3>
                <p className="text-sm text-muted-foreground">
                  Adicione amigos para poder compartilhar planos com eles
                </p>
              </CardContent>
            </Card>
          ) : (
            <>
              <p className="text-sm text-muted-foreground mb-4">
                Selecione os amigos com quem deseja compartilhar este plano:
              </p>
              <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2">
                {friends.map((friend) => (
                  <div
                    key={friend.friendId}
                    className={`flex items-center space-x-3 p-4 rounded-lg border-2 transition-all cursor-pointer hover:border-primary/50 ${
                      selectedFriends.includes(friend.friendId)
                        ? 'border-primary bg-primary/5'
                        : 'border-border'
                    }`}
                    onClick={() => toggleFriendSelection(friend.friendId)}
                  >
                    <Checkbox
                      id={friend.friendId}
                      checked={selectedFriends.includes(friend.friendId)}
                      onCheckedChange={() => toggleFriendSelection(friend.friendId)}
                      className="pointer-events-none"
                    />
                    <label
                      htmlFor={friend.friendId}
                      className="flex-1 cursor-pointer"
                    >
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                          <span className="text-sm font-semibold text-primary">
                            {friend.friendName.charAt(0).toUpperCase()}
                          </span>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="font-medium truncate">{friend.friendName}</p>
                          <p className="text-sm text-muted-foreground truncate">
                            {friend.friendEmail}
                          </p>
                        </div>
                      </div>
                    </label>
                  </div>
                ))}
              </div>
              {selectedFriends.length > 0 && (
                <p className="text-sm text-muted-foreground mt-4">
                  {selectedFriends.length} {selectedFriends.length === 1 ? 'amigo selecionado' : 'amigos selecionados'}
                </p>
              )}
            </>
          )}
        </div>

        <DialogFooter className="flex-col sm:flex-row gap-2">
          <Button
            variant="outline"
            onClick={() => {
              setSelectedFriends([]);
              onOpenChange(false);
            }}
            disabled={sharePlanMutation.isPending}
            className="w-full sm:w-auto"
          >
            Cancelar
          </Button>
          <Button
            onClick={handleShare}
            disabled={sharePlanMutation.isPending || friends.length === 0}
            className="w-full sm:w-auto"
          >
            {sharePlanMutation.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Compartilhando...
              </>
            ) : (
              <>
                <Send className="mr-2 h-4 w-4" />
                Compartilhar
              </>
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
