'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';
import { useAuth } from '@/hooks/use-auth';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/use-toast';
import {
  UserCheck,
  CheckCircle,
  XCircle,
  ArrowLeft,
  Loader2,
  MessageSquare,
  Calendar,
} from 'lucide-react';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';

interface PTRequest {
  id: string;
  trainerId: string;
  trainerName: string;
  trainerProfilePicture?: string;
  message?: string;
  status: string;
  createdAt: string;
}

export default function PTRequestsPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { toast } = useToast();

  const [requests, setRequests] = useState<PTRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [respondingTo, setRespondingTo] = useState<string | null>(null);
  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean;
    requestId: string;
    action: 'accept' | 'reject';
    trainerName: string;
  } | null>(null);

  useEffect(() => {
    fetchRequests();
  }, []);

  const fetchRequests = async () => {
    try {
      setIsLoading(true);
      const response = await apiClient.get<PTRequest[]>('/me/pt-requests');
      setRequests(Array.isArray(response) ? response : []);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao carregar solicitações',
        description: error.response?.data?.message || 'Não foi possível carregar as solicitações',
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleRespond = async (requestId: string, accepted: boolean) => {
    try {
      setRespondingTo(requestId);
      await apiClient.patch(`/me/pt-requests/${requestId}`, { accepted });

      toast({
        title: accepted ? 'Solicitação aceita!' : 'Solicitação recusada',
        description: accepted
          ? 'Você agora tem um Personal Trainer!'
          : 'A solicitação foi recusada.',
      });

      // Remove the request from the list
      setRequests(requests.filter(r => r.id !== requestId));
      setConfirmDialog(null);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao responder',
        description: error.response?.data?.message || 'Não foi possível responder à solicitação',
      });
    } finally {
      setRespondingTo(null);
    }
  };

  const openConfirmDialog = (requestId: string, action: 'accept' | 'reject', trainerName: string) => {
    setConfirmDialog({
      isOpen: true,
      requestId,
      action,
      trainerName,
    });
  };

  if (isLoading) {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto mb-4 text-primary" />
          <p className="text-muted-foreground">Carregando solicitações...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      {/* Header */}
      <div className="mb-8">
        <Button
          variant="ghost"
          onClick={() => router.back()}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Voltar
        </Button>

        <div className="flex items-center gap-3 mb-2">
          <div className="p-3 rounded-lg bg-primary/10">
            <UserCheck className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-3xl font-bold">Solicitações de Personal Trainer</h1>
            <p className="text-muted-foreground">
              Gerencie as solicitações de Personal Trainers
            </p>
          </div>
        </div>
      </div>

      {/* Requests List */}
      {requests.length === 0 ? (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center py-16">
            <UserCheck className="h-16 w-16 text-muted-foreground mb-4 opacity-50" />
            <h3 className="text-lg font-semibold mb-2">Nenhuma solicitação pendente</h3>
            <p className="text-muted-foreground text-center max-w-md">
              Você não tem solicitações de Personal Trainers no momento.
            </p>
            <Button
              variant="outline"
              className="mt-6"
              onClick={() => router.push('/trainers')}
            >
              Explorar Personal Trainers
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {requests.map((request) => (
            <Card key={request.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-4">
                    <Avatar className="h-16 w-16">
                      <AvatarImage src={getAssetUrl(request.trainerProfilePicture)} />
                      <AvatarFallback className="bg-primary/20 text-primary text-xl font-bold">
                        {request.trainerName.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <CardTitle className="text-xl">{request.trainerName}</CardTitle>
                      <CardDescription className="flex items-center gap-2 mt-1">
                        <Calendar className="h-3 w-3" />
                        {new Date(request.createdAt).toLocaleDateString('pt-BR', {
                          day: 'numeric',
                          month: 'long',
                          year: 'numeric',
                        })}
                      </CardDescription>
                    </div>
                  </div>
                  <Badge variant="secondary" className="bg-yellow-500/10 text-yellow-700">
                    Pendente
                  </Badge>
                </div>
              </CardHeader>

              {request.message && (
                <CardContent className="pt-0">
                  <div className="bg-muted/50 rounded-lg p-4 border-l-4 border-primary">
                    <div className="flex items-start gap-2">
                      <MessageSquare className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                      <p className="text-sm italic">&ldquo;{request.message}&rdquo;</p>
                    </div>
                  </div>
                </CardContent>
              )}

              <CardContent className={request.message ? 'pt-4' : ''}>
                <div className="flex gap-3">
                  <Button
                    onClick={() => openConfirmDialog(request.id, 'accept', request.trainerName)}
                    disabled={respondingTo === request.id}
                    className="flex-1"
                  >
                    {respondingTo === request.id ? (
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    ) : (
                      <CheckCircle className="mr-2 h-4 w-4" />
                    )}
                    Aceitar
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => openConfirmDialog(request.id, 'reject', request.trainerName)}
                    disabled={respondingTo === request.id}
                    className="flex-1"
                  >
                    <XCircle className="mr-2 h-4 w-4" />
                    Recusar
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Confirm Dialog */}
      {confirmDialog && (
        <AlertDialog open={confirmDialog.isOpen} onOpenChange={(open) => !open && setConfirmDialog(null)}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>
                {confirmDialog.action === 'accept'
                  ? 'Aceitar Personal Trainer?'
                  : 'Recusar solicitação?'}
              </AlertDialogTitle>
              <AlertDialogDescription>
                {confirmDialog.action === 'accept' ? (
                  <>
                    Você está prestes a aceitar <strong>{confirmDialog.trainerName}</strong> como seu Personal Trainer.
                    Ele poderá criar e gerenciar seus planos de treino.
                  </>
                ) : (
                  <>
                    Você está prestes a recusar a solicitação de <strong>{confirmDialog.trainerName}</strong>.
                    Esta ação não pode ser desfeita.
                  </>
                )}
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancelar</AlertDialogCancel>
              <AlertDialogAction
                onClick={() => handleRespond(confirmDialog.requestId, confirmDialog.action === 'accept')}
                className={confirmDialog.action === 'reject' ? 'bg-destructive hover:bg-destructive/90' : ''}
              >
                {confirmDialog.action === 'accept' ? 'Sim, aceitar' : 'Sim, recusar'}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      )}
    </div>
  );
}
