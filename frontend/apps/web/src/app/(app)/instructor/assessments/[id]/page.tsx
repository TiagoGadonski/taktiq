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
  Edit,
  Calendar,
  User,
  Activity,
  TrendingUp,
  AlertCircle,
  ClipboardCheck,
  FileText
} from 'lucide-react';
import { useToast } from '@/hooks/use-toast';

interface AssessmentDetail {
  id: string;
  studentId: string;
  studentName: string;
  trainerId: string;
  trainerName: string;
  assessmentType: string;
  assessmentDate: string;
  isActive: boolean;

  // Postural fields
  forwardHead?: string;
  roundedShoulders?: string;
  anteriorPelvicTilt?: string;
  posteriorPelvicTilt?: string;
  kneeValgus?: string;
  kneeVarus?: string;
  flatFeet?: string;
  scoliosis?: string;

  // Physical fields
  bodyFatPercentage?: number;
  muscleMass?: number;
  flexibilityScore?: number;
  strengthScore?: number;
  cardioScore?: number;

  // Custom fields
  customFields?: Array<{
    fieldName: string;
    fieldValue: string;
    fieldType: string;
  }>;

  trainerNotes?: string;
  recommendations?: string;
}

export default function AssessmentDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const assessmentId = params?.id as string;

  const [assessment, setAssessment] = useState<AssessmentDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchAssessment();
  }, [assessmentId]);

  const fetchAssessment = async () => {
    try {
      setIsLoading(true);
      const data = await apiClient.get<AssessmentDetail>(`/assessments/${assessmentId}`);
      setAssessment(data);
    } catch (error) {
      console.error('Failed to fetch assessment:', error);
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar a avaliação.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  const getSeverityColor = (severity?: string) => {
    switch (severity) {
      case 'Severe':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'Moderate':
        return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200';
      case 'Mild':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      case 'None':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  };

  const getSeverityLabel = (severity?: string) => {
    switch (severity) {
      case 'Severe':
        return 'Severo';
      case 'Moderate':
        return 'Moderado';
      case 'Mild':
        return 'Leve';
      case 'None':
        return 'Ausente';
      default:
        return severity || 'N/A';
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

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Activity className="h-12 w-12 animate-pulse mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando avaliação...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!assessment) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">
          <AlertCircle className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">Avaliação não encontrada</h3>
          <Button onClick={() => router.back()}>Voltar</Button>
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
            <h1 className="text-3xl font-bold">Detalhes da Avaliação</h1>
            <p className="text-muted-foreground">
              {getAssessmentTypeLabel(assessment.assessmentType)}
            </p>
          </div>
        </div>

        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => router.push(`/instructor/assessments/${assessmentId}/edit`)}
          >
            <Edit className="h-4 w-4 mr-2" />
            Editar
          </Button>
        </div>
      </div>

      {/* Metadata Card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Informações Gerais
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground flex items-center gap-2">
              <User className="h-4 w-4" />
              Aluno
            </p>
            <p className="font-medium">{assessment.studentName}</p>
          </div>

          <div className="space-y-1">
            <p className="text-sm text-muted-foreground flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Data da Avaliação
            </p>
            <p className="font-medium">
              {format(new Date(assessment.assessmentDate), "dd 'de' MMMM 'de' yyyy", { locale: ptBR })}
            </p>
          </div>

          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">Tipo de Avaliação</p>
            <Badge variant="outline">
              {getAssessmentTypeLabel(assessment.assessmentType)}
            </Badge>
          </div>

          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">Status</p>
            {assessment.isActive ? (
              <Badge className="bg-green-600">
                <ClipboardCheck className="h-3 w-3 mr-1" />
                Ativa
              </Badge>
            ) : (
              <Badge variant="secondary">Histórico</Badge>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Postural Assessment Details */}
      {assessment.assessmentType === 'Postural' && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5" />
              Avaliação Postural
            </CardTitle>
            <CardDescription>Desvios posturais identificados</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Forward Head */}
              {assessment.forwardHead && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Cabeça Anteriorizada</span>
                  <Badge className={getSeverityColor(assessment.forwardHead)}>
                    {getSeverityLabel(assessment.forwardHead)}
                  </Badge>
                </div>
              )}

              {/* Rounded Shoulders */}
              {assessment.roundedShoulders && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Ombros Protusos</span>
                  <Badge className={getSeverityColor(assessment.roundedShoulders)}>
                    {getSeverityLabel(assessment.roundedShoulders)}
                  </Badge>
                </div>
              )}

              {/* Anterior Pelvic Tilt */}
              {assessment.anteriorPelvicTilt && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Inclinação Pélvica Anterior</span>
                  <Badge className={getSeverityColor(assessment.anteriorPelvicTilt)}>
                    {getSeverityLabel(assessment.anteriorPelvicTilt)}
                  </Badge>
                </div>
              )}

              {/* Posterior Pelvic Tilt */}
              {assessment.posteriorPelvicTilt && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Inclinação Pélvica Posterior</span>
                  <Badge className={getSeverityColor(assessment.posteriorPelvicTilt)}>
                    {getSeverityLabel(assessment.posteriorPelvicTilt)}
                  </Badge>
                </div>
              )}

              {/* Knee Valgus */}
              {assessment.kneeValgus && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Joelhos Valgos</span>
                  <Badge className={getSeverityColor(assessment.kneeValgus)}>
                    {getSeverityLabel(assessment.kneeValgus)}
                  </Badge>
                </div>
              )}

              {/* Knee Varus */}
              {assessment.kneeVarus && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Joelhos Varos</span>
                  <Badge className={getSeverityColor(assessment.kneeVarus)}>
                    {getSeverityLabel(assessment.kneeVarus)}
                  </Badge>
                </div>
              )}

              {/* Flat Feet */}
              {assessment.flatFeet && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Pés Planos</span>
                  <Badge className={getSeverityColor(assessment.flatFeet)}>
                    {getSeverityLabel(assessment.flatFeet)}
                  </Badge>
                </div>
              )}

              {/* Scoliosis */}
              {assessment.scoliosis && (
                <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                  <span className="font-medium">Escoliose</span>
                  <Badge className={getSeverityColor(assessment.scoliosis)}>
                    {getSeverityLabel(assessment.scoliosis)}
                  </Badge>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Physical Assessment Details */}
      {assessment.assessmentType === 'Physical' && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Avaliação Física
            </CardTitle>
            <CardDescription>Medições e scores de condicionamento</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {/* Body Fat */}
              {assessment.bodyFatPercentage !== undefined && (
                <div className="text-center p-4 bg-muted/50 rounded-lg">
                  <p className="text-3xl font-bold text-primary">{assessment.bodyFatPercentage}%</p>
                  <p className="text-sm text-muted-foreground mt-1">Gordura Corporal</p>
                </div>
              )}

              {/* Muscle Mass */}
              {assessment.muscleMass !== undefined && (
                <div className="text-center p-4 bg-muted/50 rounded-lg">
                  <p className="text-3xl font-bold text-primary">{assessment.muscleMass} kg</p>
                  <p className="text-sm text-muted-foreground mt-1">Massa Muscular</p>
                </div>
              )}

              {/* Flexibility */}
              {assessment.flexibilityScore !== undefined && (
                <div className="text-center p-4 bg-muted/50 rounded-lg">
                  <p className="text-3xl font-bold text-primary">{assessment.flexibilityScore}/10</p>
                  <p className="text-sm text-muted-foreground mt-1">Flexibilidade</p>
                </div>
              )}

              {/* Strength */}
              {assessment.strengthScore !== undefined && (
                <div className="text-center p-4 bg-muted/50 rounded-lg">
                  <p className="text-3xl font-bold text-primary">{assessment.strengthScore}/10</p>
                  <p className="text-sm text-muted-foreground mt-1">Força</p>
                </div>
              )}

              {/* Cardio */}
              {assessment.cardioScore !== undefined && (
                <div className="text-center p-4 bg-muted/50 rounded-lg">
                  <p className="text-3xl font-bold text-primary">{assessment.cardioScore}/10</p>
                  <p className="text-sm text-muted-foreground mt-1">Cardio</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Custom Fields */}
      {assessment.customFields && assessment.customFields.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Campos Personalizados</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {assessment.customFields.map((field, index) => (
                <div key={index} className="p-3 bg-muted/50 rounded-lg">
                  <p className="text-sm text-muted-foreground">{field.fieldName}</p>
                  <p className="font-medium mt-1">{field.fieldValue}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Trainer Notes */}
      {assessment.trainerNotes && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5" />
              Observações do PT (Privado)
            </CardTitle>
            <CardDescription>
              Estas anotações são visíveis apenas para você
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="p-4 bg-muted/50 rounded-lg">
              <p className="whitespace-pre-wrap">{assessment.trainerNotes}</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Recommendations */}
      {assessment.recommendations && (
        <Card className="bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-900">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-blue-900 dark:text-blue-100">
              <TrendingUp className="h-5 w-5" />
              Recomendações
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="p-4 bg-white/50 dark:bg-blue-950/30 rounded-lg">
              <p className="whitespace-pre-wrap text-blue-900 dark:text-blue-100">
                {assessment.recommendations}
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
