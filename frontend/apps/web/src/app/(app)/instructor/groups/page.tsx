'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import {
  Users,
  Plus,
  ArrowLeft,
  Loader2,
  Edit,
  Trash2,
  UserPlus,
} from 'lucide-react';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';

interface StudentGroup {
  id: string;
  name: string;
  description?: string;
  tags?: string;
  memberCount: number;
  createdAt: string;
}

export default function StudentGroupsPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { toast } = useToast();
  const [deleteDialog, setDeleteDialog] = useState<{ isOpen: boolean; groupId: string; groupName: string } | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch groups
  const { data: groups, isLoading, refetch } = useQuery({
    queryKey: ['student-groups'],
    queryFn: async () => {
      const response = await apiClient.get<StudentGroup[]>('/personal/groups');
      return response || [];
    },
    enabled: user?.role === 'PersonalTrainer',
  });

  const handleDeleteGroup = async (groupId: string) => {
    setIsDeleting(true);
    try {
      await apiClient.delete(`/personal/groups/${groupId}`);
      toast({
        title: 'Grupo deletado!',
        description: 'O grupo foi removido com sucesso.',
      });
      refetch();
      setDeleteDialog(null);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao deletar grupo',
        description: error.response?.data?.message || 'Não foi possível deletar o grupo.',
      });
    } finally {
      setIsDeleting(false);
    }
  };

  if (user?.role !== 'PersonalTrainer') {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground">Esta página é exclusiva para Personal Trainers.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto mb-4 text-primary" />
          <p className="text-muted-foreground">Carregando grupos...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Header */}
      <div className="mb-8">
        <Button
          variant="ghost"
          onClick={() => router.push('/instructor')}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Voltar
        </Button>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3 mb-2">
            <div className="p-3 rounded-lg bg-primary/10">
              <Users className="h-6 w-6 text-primary" />
            </div>
            <div>
              <h1 className="text-3xl font-bold">Grupos de Alunos</h1>
              <p className="text-muted-foreground">
                Organize seus alunos em grupos para facilitar o gerenciamento
              </p>
            </div>
          </div>

          <Button onClick={() => router.push('/instructor/groups/new')}>
            <Plus className="mr-2 h-4 w-4" />
            Novo Grupo
          </Button>
        </div>
      </div>

      {/* Groups List */}
      {!groups || groups.length === 0 ? (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center py-16">
            <Users className="h-16 w-16 text-muted-foreground mb-4 opacity-50" />
            <h3 className="text-lg font-semibold mb-2">Nenhum grupo criado</h3>
            <p className="text-muted-foreground text-center max-w-md mb-6">
              Crie grupos para organizar seus alunos e facilitar a atribuição de planos de treino em lote.
            </p>
            <Button onClick={() => router.push('/instructor/groups/new')}>
              <Plus className="mr-2 h-4 w-4" />
              Criar Primeiro Grupo
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {groups.map((group) => (
            <Card key={group.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <CardTitle className="text-xl">{group.name}</CardTitle>
                    <CardDescription className="mt-2">
                      {group.memberCount} {group.memberCount === 1 ? 'aluno' : 'alunos'}
                    </CardDescription>
                  </div>
                </div>
                {group.description && (
                  <p className="text-sm text-muted-foreground mt-2">
                    {group.description}
                  </p>
                )}
              </CardHeader>
              <CardContent>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1"
                    onClick={() => router.push(`/instructor/groups/${group.id}`)}
                  >
                    <UserPlus className="mr-2 h-4 w-4" />
                    Ver Detalhes
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setDeleteDialog({ isOpen: true, groupId: group.id, groupName: group.name })}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      {deleteDialog && (
        <AlertDialog open={deleteDialog.isOpen} onOpenChange={(open) => !open && setDeleteDialog(null)}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Deletar grupo?</AlertDialogTitle>
              <AlertDialogDescription>
                Você está prestes a deletar o grupo <strong>{deleteDialog.groupName}</strong>.
                Esta ação não pode ser desfeita. Os alunos não serão removidos, apenas o agrupamento será deletado.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={isDeleting}>Cancelar</AlertDialogCancel>
              <AlertDialogAction
                onClick={() => handleDeleteGroup(deleteDialog.groupId)}
                disabled={isDeleting}
                className="bg-destructive hover:bg-destructive/90"
              >
                {isDeleting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Deletando...
                  </>
                ) : (
                  'Sim, deletar'
                )}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      )}
    </div>
  );
}
