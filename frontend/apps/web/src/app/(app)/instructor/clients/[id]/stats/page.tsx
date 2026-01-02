'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line
} from 'recharts';
import {
  ArrowLeft,
  TrendingUp,
  TrendingDown,
  Activity,
  Heart,
  Zap,
  AlertCircle,
  Calendar,
  MessageSquare,
  FileDown
} from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { exportToPDF, preparePrintableStats } from '@/lib/pdf-export';

interface StudentStats {
  studentId: string;
  studentName: string;
  periodStart: string;
  periodEnd: string;
  totalWorkoutsScheduled: number;
  completedWorkouts: number;
  completionRate: number;
  averageDifficulty: number | null;
  averageEnergy: number | null;
  averageSatisfaction: number | null;
  frequentPainAreas: PainAreaFrequency[];
  frequencyByDay: WorkoutFrequencyByDay[];
  recentFeedback: RecentFeedbackSummary[];
}

interface PainAreaFrequency {
  area: string;
  count: number;
  percentage: number;
}

interface WorkoutFrequencyByDay {
  dayOfWeek: string;
  count: number;
  percentage: number;
}

interface RecentFeedbackSummary {
  date: string;
  workoutName: string;
  difficulty: number;
  satisfaction: number;
  comments: string | null;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D', '#FFC658'];

const DAY_ORDER: Record<string, number> = {
  'Sunday': 0,
  'Monday': 1,
  'Tuesday': 2,
  'Wednesday': 3,
  'Thursday': 4,
  'Friday': 5,
  'Saturday': 6
};

const DAY_NAMES_PT: Record<string, string> = {
  'Sunday': 'Domingo',
  'Monday': 'Segunda',
  'Tuesday': 'Terça',
  'Wednesday': 'Quarta',
  'Thursday': 'Quinta',
  'Friday': 'Sexta',
  'Saturday': 'Sábado'
};

export default function StudentStatsPage() {
  const params = useParams();
  const router = useRouter();
  const clientId = params?.id as string;

  const [stats, setStats] = useState<StudentStats | null>(null);
  const [period, setPeriod] = useState<'week' | 'month' | 'all'>('month');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetchStats();
  }, [clientId, period]);

  const fetchStats = async () => {
    try {
      setIsLoading(true);
      const now = new Date();
      let startDate: Date | undefined;

      if (period === 'week') {
        startDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      } else if (period === 'month') {
        startDate = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
      }

      const params: any = {};
      if (startDate) {
        params.startDate = startDate.toISOString();
      }

      const response = await apiClient.get<StudentStats>(`/personal/clients/${clientId}/stats`, { params });
      setStats(response.data);
    } catch (error) {
      console.error('Failed to fetch stats:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getRatingColor = (value: number, type: 'difficulty' | 'energy' | 'satisfaction') => {
    if (type === 'difficulty') {
      if (value <= 2) return 'text-green-600';
      if (value <= 3) return 'text-yellow-600';
      return 'text-red-600';
    }
    if (type === 'energy') {
      if (value >= 4) return 'text-green-600';
      if (value >= 3) return 'text-yellow-600';
      return 'text-red-600';
    }
    if (type === 'satisfaction') {
      if (value >= 4) return 'text-green-600';
      if (value >= 3) return 'text-yellow-600';
      return 'text-red-600';
    }
  };

  const sortedFrequencyByDay = stats?.frequencyByDay
    ?.slice()
    .sort((a, b) => DAY_ORDER[a.dayOfWeek] - DAY_ORDER[b.dayOfWeek])
    .map(day => ({
      ...day,
      dayOfWeek: DAY_NAMES_PT[day.dayOfWeek] || day.dayOfWeek
    })) || [];

  const handleExportPDF = () => {
    try {
      const periodLabel = period === 'week' ? 'Última Semana' : period === 'month' ? 'Último Mês' : 'Todo Período';
      const printableContent = preparePrintableStats(stats, stats.studentName, periodLabel);

      // Create hidden container
      const container = document.createElement('div');
      container.id = 'pdf-export-container';
      container.style.position = 'absolute';
      container.style.left = '-9999px';
      container.innerHTML = printableContent;
      document.body.appendChild(container);

      // Export
      exportToPDF('pdf-export-container', `Estatisticas_${stats.studentName}_${periodLabel}.pdf`);

      // Cleanup
      setTimeout(() => {
        document.body.removeChild(container);
      }, 1000);
    } catch (error) {
      console.error('Error exporting PDF:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Activity className="h-12 w-12 animate-pulse mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando estatísticas...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">
          <p className="text-muted-foreground">Não foi possível carregar as estatísticas.</p>
          <Button onClick={() => router.back()} className="mt-4">
            Voltar
          </Button>
        </div>
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
            <h1 className="text-3xl font-bold">Estatísticas - {stats.studentName}</h1>
            <p className="text-muted-foreground">
              Período: {format(new Date(stats.periodStart), 'dd/MM/yyyy', { locale: ptBR })} -{' '}
              {format(new Date(stats.periodEnd), 'dd/MM/yyyy', { locale: ptBR })}
            </p>
          </div>
        </div>

        {/* Period Filter and Export */}
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={handleExportPDF}>
            <FileDown className="h-4 w-4 mr-2" />
            Exportar PDF
          </Button>
          <Tabs value={period} onValueChange={(v) => setPeriod(v as any)}>
            <TabsList>
              <TabsTrigger value="week">Última Semana</TabsTrigger>
              <TabsTrigger value="month">Último Mês</TabsTrigger>
              <TabsTrigger value="all">Todo Período</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
      </div>

      {/* Key Metrics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Completion Rate */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Taxa de Conclusão</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{stats.completionRate.toFixed(1)}%</div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.completedWorkouts} de {stats.totalWorkoutsScheduled} treinos
            </p>
            {stats.completionRate >= 80 ? (
              <div className="flex items-center gap-1 mt-2 text-green-600">
                <TrendingUp className="h-4 w-4" />
                <span className="text-xs font-medium">Excelente!</span>
              </div>
            ) : stats.completionRate >= 60 ? (
              <div className="flex items-center gap-1 mt-2 text-yellow-600">
                <Activity className="h-4 w-4" />
                <span className="text-xs font-medium">Bom</span>
              </div>
            ) : (
              <div className="flex items-center gap-1 mt-2 text-red-600">
                <TrendingDown className="h-4 w-4" />
                <span className="text-xs font-medium">Precisa melhorar</span>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Average Difficulty */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Activity className="h-4 w-4" />
              Dificuldade Média
            </CardTitle>
          </CardHeader>
          <CardContent>
            {stats.averageDifficulty !== null ? (
              <>
                <div className={`text-3xl font-bold ${getRatingColor(stats.averageDifficulty, 'difficulty')}`}>
                  {stats.averageDifficulty.toFixed(1)}/5
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  {stats.averageDifficulty <= 2 && 'Treinos muito fáceis'}
                  {stats.averageDifficulty > 2 && stats.averageDifficulty <= 3 && 'Treinos adequados'}
                  {stats.averageDifficulty > 3 && 'Treinos desafiadores'}
                </p>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">Sem dados</p>
            )}
          </CardContent>
        </Card>

        {/* Average Energy */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Zap className="h-4 w-4" />
              Energia Média
            </CardTitle>
          </CardHeader>
          <CardContent>
            {stats.averageEnergy !== null ? (
              <>
                <div className={`text-3xl font-bold ${getRatingColor(stats.averageEnergy, 'energy')}`}>
                  {stats.averageEnergy.toFixed(1)}/5
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  {stats.averageEnergy >= 4 && 'Muito energizado'}
                  {stats.averageEnergy >= 3 && stats.averageEnergy < 4 && 'Energia normal'}
                  {stats.averageEnergy < 3 && 'Frequentemente exausto'}
                </p>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">Sem dados</p>
            )}
          </CardContent>
        </Card>

        {/* Average Satisfaction */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <Heart className="h-4 w-4" />
              Satisfação Média
            </CardTitle>
          </CardHeader>
          <CardContent>
            {stats.averageSatisfaction !== null ? (
              <>
                <div className={`text-3xl font-bold ${getRatingColor(stats.averageSatisfaction, 'satisfaction')}`}>
                  {stats.averageSatisfaction.toFixed(1)}/5
                </div>
                <p className="text-xs text-muted-foreground mt-1">
                  {stats.averageSatisfaction >= 4 && 'Muito satisfeito'}
                  {stats.averageSatisfaction >= 3 && stats.averageSatisfaction < 4 && 'Satisfeito'}
                  {stats.averageSatisfaction < 3 && 'Insatisfeito'}
                </p>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">Sem dados</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Frequency by Day */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Frequência por Dia da Semana
            </CardTitle>
            <CardDescription>Distribuição de treinos completados</CardDescription>
          </CardHeader>
          <CardContent>
            {sortedFrequencyByDay.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={sortedFrequencyByDay}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="dayOfWeek" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="count" fill="#8884d8" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-muted-foreground text-center py-12">Sem dados de frequência</p>
            )}
          </CardContent>
        </Card>

        {/* Pain Areas */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-orange-500" />
              Áreas de Dor Frequentes
            </CardTitle>
            <CardDescription>Regiões com desconforto relatado</CardDescription>
          </CardHeader>
          <CardContent>
            {stats.frequentPainAreas.length > 0 ? (
              <div className="space-y-3">
                {stats.frequentPainAreas.map((pain, index) => (
                  <div key={pain.area} className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div
                        className="w-3 h-3 rounded-full"
                        style={{ backgroundColor: COLORS[index % COLORS.length] }}
                      />
                      <span className="font-medium">{pain.area}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">{pain.count}x</Badge>
                      <span className="text-sm text-muted-foreground">{pain.percentage.toFixed(1)}%</span>
                    </div>
                  </div>
                ))}
                <div className="mt-4 p-3 bg-orange-50 dark:bg-orange-950/20 rounded-lg">
                  <p className="text-sm text-orange-800 dark:text-orange-200">
                    ⚠️ Considere ajustar os treinos para reduzir sobrecarga nessas áreas.
                  </p>
                </div>
              </div>
            ) : (
              <p className="text-muted-foreground text-center py-12">Nenhuma área de dor relatada</p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Recent Feedback */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MessageSquare className="h-5 w-5" />
            Feedback Recente
          </CardTitle>
          <CardDescription>Últimos feedbacks enviados pelo aluno</CardDescription>
        </CardHeader>
        <CardContent>
          {stats.recentFeedback.length > 0 ? (
            <div className="space-y-4">
              {stats.recentFeedback.map((feedback, index) => (
                <div
                  key={index}
                  className="border-b last:border-0 pb-4 last:pb-0"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <p className="font-medium">{feedback.workoutName}</p>
                        <Badge variant="outline" className="text-xs">
                          {format(new Date(feedback.date), 'dd/MM/yyyy', { locale: ptBR })}
                        </Badge>
                      </div>
                      <div className="flex items-center gap-4 text-sm">
                        <span className={getRatingColor(feedback.difficulty, 'difficulty')}>
                          Dificuldade: {feedback.difficulty}/5
                        </span>
                        <span className={getRatingColor(feedback.satisfaction, 'satisfaction')}>
                          Satisfação: {feedback.satisfaction}/5
                        </span>
                      </div>
                      {feedback.comments && (
                        <p className="text-sm text-muted-foreground mt-2 italic">
                          &ldquo;{feedback.comments}&rdquo;
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-muted-foreground text-center py-12">Nenhum feedback disponível</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
