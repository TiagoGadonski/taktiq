'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import {
  ArrowLeft,
  Plus,
  FileText,
  CheckCircle2,
  Activity,
  Calendar,
  Trash2
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
import { useToast } from '@/hooks/use-toast';

interface Assessment {
  id: string;
  studentId: string;
  studentName: string;
  assessmentType: string;
  assessmentDate: string;
  isActive: boolean;
  summary: string | null;
}

export default function AssessmentsListPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const studentId = params?.id as string;

  const [assessments, setAssessments] = useState<Assessment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  useEffect(() => {
    fetchAssessments();
  }, [studentId]);

  const fetchAssessments = async () => {
    try {
      setIsLoading(true);
      const data = await apiClient.get<Assessment[]>(`/assessments/student/${studentId}`);
      setAssessments(data);
    } catch (error) {
      console.error('Failed to fetch assessments:', error);
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar as avaliações.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await apiClient.delete(`/assessments/${id}`);
      toast({
        title: 'Avaliação excluída',
        description: 'A avaliação foi removida com sucesso.',
      });
      setAssessments(assessments.filter(a => a.id !== id));
      setDeleteId(null);
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível excluir a avaliação.',
        variant: 'destructive',
      });
    }
  };

  const getAssessmentTypeLabel = (type: string) => {
    const labels: Record<string, string> = {
      'Postural': 'Postural',
      'Physical': 'Física',
      'Custom': 'Personalizada'
    };
    return labels[type] || type;
  };

  const getAssessmentTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      'Postural': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
      'Physical': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
      'Custom': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200'
    };
    return colors[type] || 'bg-gray-100 text-gray-800';
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Activity className="h-12 w-12 animate-pulse mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando avaliações...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-5xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Avaliações</h1>
            <p className="text-muted-foreground">
              {assessments.length > 0 && `Histórico de avaliações de ${assessments[0].studentName}`}
            </p>
          </div>
        </div>

        <Button onClick={() => router.push(`/instructor/clients/${studentId}/assessments/new`)}>
          <Plus className="h-4 w-4 mr-2" />
          Nova Avaliação
        </Button>
      </div>

      {/* Assessments List */}
      {assessments.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <FileText className="h-16 w-16 text-muted-foreground mb-4" />
            <h3 className="text-lg font-semibold mb-2">Nenhuma avaliação ainda</h3>
            <p className="text-muted-foreground text-center mb-6 max-w-md">
              Crie a primeira avaliação para este aluno e comece a personalizar os treinos.
            </p>
            <Button onClick={() => router.push(`/instructor/clients/${studentId}/assessments/new`)}>
              <Plus className="h-4 w-4 mr-2" />
              Criar Primeira Avaliação
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {assessments.map((assessment) => (
            <Card key={assessment.id} className="hover:shadow-md transition-shadow">
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="space-y-1 flex-1">
                    <div className="flex items-center gap-3">
                      <CardTitle className="text-xl">
                        Avaliação {getAssessmentTypeLabel(assessment.assessmentType)}
                      </CardTitle>
                      <Badge
                        className={getAssessmentTypeColor(assessment.assessmentType)}
                      >
                        {getAssessmentTypeLabel(assessment.assessmentType)}
                      </Badge>
                      {assessment.isActive && (
                        <Badge variant="default" className="bg-green-600">
                          <CheckCircle2 className="h-3 w-3 mr-1" />
                          Ativa
                        </Badge>
                      )}
                    </div>
                    <CardDescription className="flex items-center gap-2 text-base">
                      <Calendar className="h-4 w-4" />
                      {format(new Date(assessment.assessmentDate), "dd 'de' MMMM 'de' yyyy", { locale: ptBR })}
                    </CardDescription>
                  </div>

                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => router.push(`/instructor/assessments/${assessment.id}`)}
                    >
                      Ver Detalhes
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => setDeleteId(assessment.id)}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                </div>
              </CardHeader>

              {assessment.summary && (
                <CardContent>
                  <div className="bg-muted/50 p-4 rounded-lg">
                    <p className="text-sm">
                      <span className="font-semibold">Resumo: </span>
                      {assessment.summary}
                    </p>
                  </div>
                </CardContent>
              )}
            </Card>
          ))}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteId !== null} onOpenChange={(open) => !open && setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Excluir avaliação?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta ação não pode ser desfeita. A avaliação será permanentemente removida.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteId && handleDelete(deleteId)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Excluir
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
