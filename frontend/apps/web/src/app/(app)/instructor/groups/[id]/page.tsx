'use client';

import { useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useToast } from '@/components/ui/use-toast';
import { getAssetUrl } from '@/lib/env';
import {
  Users,
  ArrowLeft,
  Loader2,
  UserPlus,
  UserMinus,
  FileText,
  Edit,
  Trash2,
} from 'lucide-react';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';

interface GroupMember {
  id: string;
  name: string;
  email: string;
  profilePictureUrl?: string;
  addedAt: string;
}

interface StudentGroupDetail {
  id: string;
  name: string;
  description?: string;
  tags?: string;
  memberCount: number;
  createdAt: string;
  members: GroupMember[];
}

interface Student {
  id: string;
  name: string;
  email: string;
}

export default function GroupDetailPage() {
  const router = useRouter();
  const params = useParams();
  const groupId = params.id as string;
  const { user } = useAuth();
  const { toast } = useToast();

  const [removeDialog, setRemoveDialog] = useState<{ isOpen: boolean; memberId: string; memberName: string } | null>(null);
  const [addMembersDialog, setAddMembersDialog] = useState(false);
  const [selectedStudentIds, setSelectedStudentIds] = useState<string[]>([]);
  const [isRemoving, setIsRemoving] = useState(false);
  const [isAdding, setIsAdding] = useState(false);

  // Fetch group details
  const { data: group, isLoading, refetch } = useQuery({
    queryKey: ['student-group', groupId],
    queryFn: async () => {
      const response = await apiClient.get<StudentGroupDetail>(`/personal/groups/${groupId}`);
      return response;
    },
    enabled: user?.role === 'PersonalTrainer' && !!groupId,
  });

  // Fetch all students for adding members
  const { data: allStudents } = useQuery({
    queryKey: ['personal-students'],
    queryFn: async () => {
      const response = await apiClient.get<Student[]>('/personal/clients');
      return response || [];
    },
    enabled: user?.role === 'PersonalTrainer' && addMembersDialog,
  });

  const handleRemoveMember = async (memberId: string) => {
    setIsRemoving(true);
    try {
      await apiClient.delete(`/personal/groups/${groupId}/members/${memberId}`);
      toast({
        title: 'Aluno removido!',
        description: 'O aluno foi removido do grupo com sucesso.',
      });
      refetch();
      setRemoveDialog(null);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao remover aluno',
        description: error.response?.data?.message || 'Não foi possível remover o aluno.',
      });
    } finally {
      setIsRemoving(false);
    }
  };

  const handleAddMembers = async () => {
    if (selectedStudentIds.length === 0) {
      toast({
        variant: 'destructive',
        title: 'Selecione alunos',
        description: 'Você deve selecionar pelo menos um aluno.',
      });
      return;
    }

    setIsAdding(true);
    try {
      await apiClient.post(`/personal/groups/${groupId}/members`, {
        studentIds: selectedStudentIds,
      });
      toast({
        title: 'Alunos adicionados!',
        description: `${selectedStudentIds.length} ${selectedStudentIds.length === 1 ? 'aluno foi adicionado' : 'alunos foram adicionados'} ao grupo.`,
      });
      setSelectedStudentIds([]);
      setAddMembersDialog(false);
      refetch();
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao adicionar alunos',
        description: error.response?.data?.message || 'Não foi possível adicionar os alunos.',
      });
    } finally {
      setIsAdding(false);
    }
  };

  const handleAssignPlan = () => {
    router.push(`/plans/new?groupId=${groupId}`);
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
          <p className="text-muted-foreground">Carregando grupo...</p>
        </div>
      </div>
    );
  }

  if (!group) {
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-muted-foreground">Grupo não encontrado.</p>
            <Button className="mt-4" onClick={() => router.push('/instructor/groups')}>
              Voltar para Grupos
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Filter out students already in the group
  const availableStudents = allStudents?.filter(
    student => !group.members.some(member => member.id === student.id)
  ) || [];

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Header */}
      <div className="mb-8">
        <Button
          variant="ghost"
          onClick={() => router.push('/instructor/groups')}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Voltar
        </Button>

        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="p-3 rounded-lg bg-primary/10">
              <Users className="h-6 w-6 text-primary" />
            </div>
            <div>
              <h1 className="text-3xl font-bold">{group.name}</h1>
              {group.description && (
                <p className="text-muted-foreground mt-1">{group.description}</p>
              )}
              <p className="text-sm text-muted-foreground mt-2">
                {group.memberCount} {group.memberCount === 1 ? 'aluno' : 'alunos'}
              </p>
            </div>
          </div>

          <div className="flex gap-2">
            <Button variant="outline" onClick={handleAssignPlan}>
              <FileText className="mr-2 h-4 w-4" />
              Atribuir Plano
            </Button>
            <Button onClick={() => setAddMembersDialog(true)}>
              <UserPlus className="mr-2 h-4 w-4" />
              Adicionar Alunos
            </Button>
          </div>
        </div>
      </div>

      {/* Members List */}
      <Card>
        <CardHeader>
          <CardTitle>Membros do Grupo</CardTitle>
          <CardDescription>Alunos que fazem parte deste grupo</CardDescription>
        </CardHeader>
        <CardContent>
          {group.members.length === 0 ? (
            <div className="text-center py-12">
              <Users className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">Nenhum aluno no grupo</h3>
              <p className="text-muted-foreground mb-4">
                Adicione alunos a este grupo para começar.
              </p>
              <Button onClick={() => setAddMembersDialog(true)}>
                <UserPlus className="mr-2 h-4 w-4" />
                Adicionar Alunos
              </Button>
            </div>
          ) : (
            <div className="space-y-2">
              {group.members.map((member) => (
                <div
                  key={member.id}
                  className="flex items-center justify-between p-4 border rounded-lg hover:bg-accent transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={getAssetUrl(member.profilePictureUrl)} />
                      <AvatarFallback className="bg-primary/20 text-primary font-bold">
                        {member.name.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-medium">{member.name}</p>
                      <p className="text-sm text-muted-foreground">{member.email}</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        Adicionado em {new Date(member.addedAt).toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setRemoveDialog({ isOpen: true, memberId: member.id, memberName: member.name })}
                  >
                    <UserMinus className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Remove Member Dialog */}
      {removeDialog && (
        <AlertDialog open={removeDialog.isOpen} onOpenChange={(open) => !open && setRemoveDialog(null)}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Remover aluno do grupo?</AlertDialogTitle>
              <AlertDialogDescription>
                Você está prestes a remover <strong>{removeDialog.memberName}</strong> deste grupo.
                O aluno não será deletado, apenas removido do agrupamento.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={isRemoving}>Cancelar</AlertDialogCancel>
              <AlertDialogAction
                onClick={() => handleRemoveMember(removeDialog.memberId)}
                disabled={isRemoving}
                className="bg-destructive hover:bg-destructive/90"
              >
                {isRemoving ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Removendo...
                  </>
                ) : (
                  'Sim, remover'
                )}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      )}

      {/* Add Members Dialog */}
      <Dialog open={addMembersDialog} onOpenChange={setAddMembersDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Adicionar Alunos ao Grupo</DialogTitle>
            <DialogDescription>
              Selecione os alunos que deseja adicionar a este grupo
            </DialogDescription>
          </DialogHeader>

          {availableStudents.length === 0 ? (
            <div className="py-8 text-center text-sm text-muted-foreground">
              <p>Todos os seus alunos já estão neste grupo.</p>
            </div>
          ) : (
            <>
              <div className="flex items-center justify-between mb-2">
                <Label>
                  {selectedStudentIds.length} {selectedStudentIds.length === 1 ? 'aluno selecionado' : 'alunos selecionados'}
                </Label>
                {selectedStudentIds.length > 0 && (
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setSelectedStudentIds([])}
                  >
                    Limpar
                  </Button>
                )}
              </div>

              <div className="border rounded-lg max-h-96 overflow-y-auto">
                <div className="divide-y">
                  {availableStudents.map((student) => (
                    <div
                      key={student.id}
                      className="flex items-center space-x-3 p-3 hover:bg-accent cursor-pointer"
                      onClick={() => {
                        setSelectedStudentIds(prev =>
                          prev.includes(student.id)
                            ? prev.filter(id => id !== student.id)
                            : [...prev, student.id]
                        );
                      }}
                    >
                      <Checkbox
                        checked={selectedStudentIds.includes(student.id)}
                        onCheckedChange={(checked) => {
                          setSelectedStudentIds(prev =>
                            checked
                              ? [...prev, student.id]
                              : prev.filter(id => id !== student.id)
                          );
                        }}
                      />
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-sm">{student.name}</p>
                        <p className="text-xs text-muted-foreground truncate">{student.email}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => { setAddMembersDialog(false); setSelectedStudentIds([]); }} disabled={isAdding}>
              Cancelar
            </Button>
            <Button onClick={handleAddMembers} disabled={isAdding || selectedStudentIds.length === 0 || availableStudents.length === 0}>
              {isAdding ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Adicionando...
                </>
              ) : (
                <>
                  <UserPlus className="mr-2 h-4 w-4" />
                  Adicionar
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
