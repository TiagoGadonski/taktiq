'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { apiClient } from '@/lib/api';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/use-toast';
import {
  Users,
  ArrowLeft,
  Save,
  Loader2,
} from 'lucide-react';

const groupSchema = z.object({
  name: z.string().min(3, 'Nome deve ter no mínimo 3 caracteres'),
  description: z.string().optional(),
  tags: z.string().optional(),
});

type GroupFormData = z.infer<typeof groupSchema>;

interface Student {
  id: string;
  name: string;
  email: string;
}

export default function NewGroupPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { toast } = useToast();
  const [selectedStudentIds, setSelectedStudentIds] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<GroupFormData>({
    resolver: zodResolver(groupSchema),
  });

  // Fetch PT's students
  const { data: students, isLoading: isLoadingStudents } = useQuery({
    queryKey: ['personal-students'],
    queryFn: async () => {
      const response = await apiClient.get<Student[]>('/personal/clients');
      return response || [];
    },
    enabled: user?.role === 'PersonalTrainer',
  });

  const onSubmit = async (data: GroupFormData) => {
    setIsSubmitting(true);
    try {
      // Create the group
      const createResponse = await apiClient.post<{ id: string }>('/personal/groups', {
        name: data.name,
        description: data.description || null,
        tags: data.tags || null,
      });

      const groupId = createResponse.id;

      // Add members if any selected
      if (selectedStudentIds.length > 0) {
        await apiClient.post(`/personal/groups/${groupId}/members`, {
          studentIds: selectedStudentIds,
        });
      }

      toast({
        title: 'Grupo criado!',
        description: `Grupo "${data.name}" criado com ${selectedStudentIds.length} ${selectedStudentIds.length === 1 ? 'aluno' : 'alunos'}.`,
      });

      router.push('/instructor/groups');
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao criar grupo',
        description: error.response?.data?.message || 'Não foi possível criar o grupo.',
      });
    } finally {
      setIsSubmitting(false);
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

        <div className="flex items-center gap-3">
          <div className="p-3 rounded-lg bg-primary/10">
            <Users className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-3xl font-bold">Criar Novo Grupo</h1>
            <p className="text-muted-foreground">
              Organize seus alunos em um grupo
            </p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Group Details */}
        <Card>
          <CardHeader>
            <CardTitle>Informações do Grupo</CardTitle>
            <CardDescription>Dados básicos do grupo de alunos</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nome do Grupo *</Label>
              <Input
                id="name"
                placeholder="Ex: Grupo Hipertrofia Avançado"
                {...register('name')}
              />
              {errors.name && (
                <p className="text-sm text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Descrição</Label>
              <Textarea
                id="description"
                placeholder="Descreva o objetivo ou características deste grupo..."
                rows={3}
                {...register('description')}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="tags">Tags</Label>
              <Input
                id="tags"
                placeholder="Ex: Avançado, Hipertrofia, Segunda e Quarta"
                {...register('tags')}
              />
              <p className="text-xs text-muted-foreground">
                Tags separadas por vírgula para facilitar a organização
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Select Students */}
        <Card>
          <CardHeader>
            <CardTitle>Selecionar Alunos</CardTitle>
            <CardDescription>
              Escolha os alunos que farão parte deste grupo
            </CardDescription>
          </CardHeader>
          <CardContent>
            {isLoadingStudents ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
              </div>
            ) : students && students.length > 0 ? (
              <>
                <div className="flex items-center justify-between mb-4">
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
                      Limpar seleção
                    </Button>
                  )}
                </div>

                <div className="border rounded-lg max-h-96 overflow-y-auto">
                  <div className="divide-y">
                    {students.map((student) => (
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
            ) : (
              <div className="text-center py-8 text-sm text-muted-foreground">
                <p>Você ainda não tem alunos cadastrados.</p>
                <p className="mt-2">Adicione alunos primeiro para criar grupos.</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Submit Buttons */}
        <div className="flex gap-2">
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Criando...
              </>
            ) : (
              <>
                <Save className="mr-2 h-4 w-4" />
                Criar Grupo
              </>
            )}
          </Button>
          <Button
            type="button"
            variant="outline"
            size="lg"
            onClick={() => router.back()}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
        </div>
      </form>
    </div>
  );
}
