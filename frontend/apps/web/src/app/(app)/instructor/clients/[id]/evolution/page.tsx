'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar
} from 'recharts';
import {
  ArrowLeft,
  TrendingUp,
  Activity,
  BarChart3
} from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { useToast } from '@/hooks/use-toast';

interface Assessment {
  id: string;
  assessmentDate: string;
  assessmentType: string;
  bodyFatPercentage?: number;
  muscleMass?: number;
  flexibilityScore?: number;
  strengthScore?: number;
  cardioScore?: number;
}

export default function StudentEvolutionPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const clientId = params?.id as string;

  const [assessments, setAssessments] = useState<Assessment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [studentName, setStudentName] = useState('');

  useEffect(() => {
    fetchAssessments();
  }, [clientId]);

  const fetchAssessments = async () => {
    try {
      setIsLoading(true);
      const response = await apiClient.get<Assessment[]>(`/assessments/student/${clientId}`);
      const physicalAssessments = response.data.filter(a => a.assessmentType === 'Physical');

      // Sort by date (oldest first for timeline)
      physicalAssessments.sort((a, b) =>
        new Date(a.assessmentDate).getTime() - new Date(b.assessmentDate).getTime()
      );

      setAssessments(physicalAssessments);

      if (physicalAssessments.length > 0) {
        // Get student name from first assessment (would be better from a separate API call)
        const firstAssessment = physicalAssessments[0] as any;
        setStudentName(firstAssessment.studentName || '');
      }
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

  // Prepare data for body composition chart
  const bodyCompositionData = assessments.map(a => ({
    date: format(new Date(a.assessmentDate), 'dd/MM/yy', { locale: ptBR }),
    'Gordura (%)': a.bodyFatPercentage || 0,
    'Massa Muscular (kg)': a.muscleMass || 0
  })).filter(d => d['Gordura (%)'] > 0 || d['Massa Muscular (kg)'] > 0);

  // Prepare data for performance scores chart
  const performanceData = assessments.map(a => ({
    date: format(new Date(a.assessmentDate), 'dd/MM/yy', { locale: ptBR }),
    'Flexibilidade': a.flexibilityScore || 0,
    'Força': a.strengthScore || 0,
    'Cardio': a.cardioScore || 0
  })).filter(d => d['Flexibilidade'] > 0 || d['Força'] > 0 || d['Cardio'] > 0);

  // Prepare data for radar chart (latest assessment)
  const latestAssessment = assessments[assessments.length - 1];
  const radarData = latestAssessment ? [
    {
      metric: 'Flexibilidade',
      value: latestAssessment.flexibilityScore || 0,
      fullMark: 10
    },
    {
      metric: 'Força',
      value: latestAssessment.strengthScore || 0,
      fullMark: 10
    },
    {
      metric: 'Cardio',
      value: latestAssessment.cardioScore || 0,
      fullMark: 10
    }
  ] : [];

  // Calculate evolution percentages
  const calculateEvolution = (field: keyof Assessment) => {
    if (assessments.length < 2) return null;
    const first = assessments[0][field] as number;
    const latest = assessments[assessments.length - 1][field] as number;
    if (!first || !latest) return null;
    return ((latest - first) / first * 100).toFixed(1);
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Activity className="h-12 w-12 animate-pulse mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando evolução...</p>
          </div>
        </div>
      </div>
    );
  }

  if (assessments.length === 0) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Evolução - {studentName}</h1>
            <p className="text-muted-foreground">Acompanhamento temporal das avaliações</p>
          </div>
        </div>

        <Card>
          <CardContent className="py-12 text-center">
            <BarChart3 className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold mb-2">Sem Avaliações Físicas</h3>
            <p className="text-muted-foreground max-w-md mx-auto">
              É necessário ter pelo menos uma avaliação física para visualizar gráficos de evolução.
            </p>
            <Button
              className="mt-4"
              onClick={() => router.push(`/instructor/clients/${clientId}/assessments/new`)}
            >
              Criar Avaliação Física
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Evolução - {studentName}</h1>
            <p className="text-muted-foreground">
              {assessments.length} avaliações físicas registradas
            </p>
          </div>
        </div>
      </div>

      {/* Evolution Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Gordura Corporal</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {latestAssessment?.bodyFatPercentage?.toFixed(1) || '—'}%
            </div>
            {calculateEvolution('bodyFatPercentage') && (
              <p className={`text-xs ${parseFloat(calculateEvolution('bodyFatPercentage')!) < 0 ? 'text-green-600' : 'text-red-600'} mt-1 flex items-center gap-1`}>
                <TrendingUp className="h-3 w-3" />
                {calculateEvolution('bodyFatPercentage')}% desde primeira avaliação
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Massa Muscular</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {latestAssessment?.muscleMass?.toFixed(1) || '—'} kg
            </div>
            {calculateEvolution('muscleMass') && (
              <p className={`text-xs ${parseFloat(calculateEvolution('muscleMass')!) > 0 ? 'text-green-600' : 'text-red-600'} mt-1 flex items-center gap-1`}>
                <TrendingUp className="h-3 w-3" />
                {calculateEvolution('muscleMass')}% desde primeira avaliação
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Flexibilidade</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {latestAssessment?.flexibilityScore?.toFixed(1) || '—'}/10
            </div>
            {calculateEvolution('flexibilityScore') && (
              <p className={`text-xs ${parseFloat(calculateEvolution('flexibilityScore')!) > 0 ? 'text-green-600' : 'text-red-600'} mt-1 flex items-center gap-1`}>
                <TrendingUp className="h-3 w-3" />
                {calculateEvolution('flexibilityScore')}% desde primeira avaliação
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Força</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {latestAssessment?.strengthScore?.toFixed(1) || '—'}/10
            </div>
            {calculateEvolution('strengthScore') && (
              <p className={`text-xs ${parseFloat(calculateEvolution('strengthScore')!) > 0 ? 'text-green-600' : 'text-red-600'} mt-1 flex items-center gap-1`}>
                <TrendingUp className="h-3 w-3" />
                {calculateEvolution('strengthScore')}% desde primeira avaliação
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Body Composition Chart */}
        {bodyCompositionData.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Composição Corporal</CardTitle>
              <CardDescription>Evolução ao longo do tempo</CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={bodyCompositionData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="Gordura (%)"
                    stroke="#ef4444"
                    strokeWidth={2}
                    dot={{ fill: '#ef4444', r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Massa Muscular (kg)"
                    stroke="#22c55e"
                    strokeWidth={2}
                    dot={{ fill: '#22c55e', r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        )}

        {/* Performance Scores Chart */}
        {performanceData.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Performance</CardTitle>
              <CardDescription>Scores de condicionamento físico</CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={performanceData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis domain={[0, 10]} />
                  <Tooltip />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="Flexibilidade"
                    stroke="#3b82f6"
                    strokeWidth={2}
                    dot={{ fill: '#3b82f6', r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Força"
                    stroke="#f59e0b"
                    strokeWidth={2}
                    dot={{ fill: '#f59e0b', r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Cardio"
                    stroke="#10b981"
                    strokeWidth={2}
                    dot={{ fill: '#10b981', r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        )}

        {/* Radar Chart - Current State */}
        {radarData.length > 0 && (
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Perfil Atual de Condicionamento</CardTitle>
              <CardDescription>
                Baseado na avaliação mais recente - {format(new Date(latestAssessment.assessmentDate), "dd/MM/yyyy", { locale: ptBR })}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <RadarChart data={radarData}>
                  <PolarGrid />
                  <PolarAngleAxis dataKey="metric" />
                  <PolarRadiusAxis angle={90} domain={[0, 10]} />
                  <Radar
                    name="Score"
                    dataKey="value"
                    stroke="#8884d8"
                    fill="#8884d8"
                    fillOpacity={0.6}
                  />
                  <Tooltip />
                </RadarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Insights Card */}
      {assessments.length >= 2 && (
        <Card className="bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-900">
          <CardHeader>
            <CardTitle className="text-blue-900 dark:text-blue-100">Insights da Evolução</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <p className="text-sm text-blue-900 dark:text-blue-100">
              <strong>Período de acompanhamento:</strong> {assessments.length} avaliações em{' '}
              {Math.floor((new Date(latestAssessment.assessmentDate).getTime() - new Date(assessments[0].assessmentDate).getTime()) / (1000 * 60 * 60 * 24))} dias
            </p>

            {calculateEvolution('bodyFatPercentage') && parseFloat(calculateEvolution('bodyFatPercentage')!) < 0 && (
              <p className="text-sm text-green-700 dark:text-green-300">
                ✅ Redução significativa no percentual de gordura ({Math.abs(parseFloat(calculateEvolution('bodyFatPercentage')!))}%)
              </p>
            )}

            {calculateEvolution('muscleMass') && parseFloat(calculateEvolution('muscleMass')!) > 0 && (
              <p className="text-sm text-green-700 dark:text-green-300">
                ✅ Ganho de massa muscular ({calculateEvolution('muscleMass')}%)
              </p>
            )}

            {calculateEvolution('strengthScore') && parseFloat(calculateEvolution('strengthScore')!) > 10 && (
              <p className="text-sm text-green-700 dark:text-green-300">
                ✅ Melhora expressiva na força ({calculateEvolution('strengthScore')}%)
              </p>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
