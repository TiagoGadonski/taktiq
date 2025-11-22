'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Dumbbell, Trophy, TrendingUp, Zap, Check, ChevronDown, ChevronUp, FileText, MessageCircle, Calendar, Sparkles } from 'lucide-react';
import { api } from '@/lib/api';
import { apiClient } from '@/lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { useAuth } from '@/hooks/use-auth';
import { PostFeed } from '@/components/posts/post-feed';
import { InstructorDashboard } from '@/components/dashboard/instructor-dashboard';
import { InstructorCard } from '@/components/dashboard/instructor-card';
import { getDailyQuote } from '@/components/dashboard/motivational-quotes';

interface Post {
  id: string;
  title: string;
  content: string;
  imageUrl?: string;
  authorId: string;
  authorName: string;
  authorProfilePictureUrl?: string;
  authorProfileSlug?: string;
  isPublished: boolean;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

// Helper function to get time-based greeting
function getTimeBasedGreeting() {
  const hour = new Date().getHours();
  if (hour >= 5 && hour < 12) return 'Bom dia';
  if (hour >= 12 && hour < 18) return 'Boa tarde';
  return 'Boa noite';
}

export default function DashboardPage() {
  const [isCalendarExpanded, setIsCalendarExpanded] = useState(false);
  const { user } = useAuth();
  const dailyQuote = getDailyQuote();

  const { data: progress, isLoading: isLoadingProgress } = useQuery({
    queryKey: ['progress', 'dashboard'],
    queryFn: () => api.progress.getDashboard(),
  });

  const { data: currentSession } = useQuery({
    queryKey: ['sessions', 'current'],
    queryFn: () => api.sessions.getCurrent(),
  });

  const { data: challenges } = useQuery({
    queryKey: ['challenges', 'active'],
    queryFn: () => api.challenges.getAll({ status: 'active' }),
  });

  // Fetch posts from the user's personal trainer OR all public posts if no PT assigned
  const { data: posts } = useQuery({
    queryKey: ['posts', user?.personalTrainerId ? 'trainer' : 'all', user?.personalTrainerId],
    queryFn: async () => {
      if (user?.personalTrainerId) {
        // Fetch posts from user's assigned personal trainer
        const response = await apiClient.get<Post[]>(`/posts/trainer/${user.personalTrainerId}`);
        return Array.isArray(response) ? response : [];
      } else {
        // Fetch all published posts from all trainers
        const response = await apiClient.get<Post[]>('/posts?page=1&pageSize=10');
        return Array.isArray(response) ? response : [];
      }
    },
    enabled: !!user,
  });

  // Generate full calendar from account creation to today
  const generateFullCalendar = () => {
    if (!progress?.weeklyWorkouts || progress.weeklyWorkouts.length === 0) return {};

    const today = new Date();
    const workoutDatesSet = new Set(
      progress.weeklyWorkouts
        .filter(w => w.completed)
        .map(w => new Date(w.date).toDateString())
    );

    // Use the account creation date from the backend
    const startDate = progress.accountCreatedAt
      ? new Date(progress.accountCreatedAt)
      : new Date(today.getFullYear(), today.getMonth() - 2, 1); // Fallback to 3 months ago

    // Group days by month
    const monthsData: { [key: string]: any[] } = {};
    const currentDate = new Date(startDate);

    while (currentDate <= today) {
      const dateObj = new Date(currentDate);
      const monthKey = `${dateObj.getFullYear()}-${String(dateObj.getMonth() + 1).padStart(2, '0')}`;

      if (!monthsData[monthKey]) {
        monthsData[monthKey] = [];
      }

      monthsData[monthKey].push({
        date: dateObj.toISOString(),
        dayOfWeek: dateObj.toLocaleDateString('pt-BR', { weekday: 'long' }),
        completed: workoutDatesSet.has(dateObj.toDateString()),
        setsCompleted: 0,
        dayNum: dateObj.getDate(),
        isToday: dateObj.toDateString() === today.toDateString(),
      });

      currentDate.setDate(currentDate.getDate() + 1);
    }

    return monthsData;
  };

  // Show instructor dashboard for Personal Trainers
  if (user?.role === 'PersonalTrainer') {
    return <InstructorDashboard />;
  }

  // Show student dashboard for regular users
  return (
    <div className="space-y-4 sm:space-y-6">
      {/* Personalized Greeting */}
      <div className="space-y-2">
        <h1 className="text-3xl font-bold sm:text-4xl">
          {getTimeBasedGreeting()}, {user?.name?.split(' ')[0] || 'Atleta'}!
        </h1>
        <p className="text-base text-muted-foreground sm:text-lg">
          {currentSession
            ? 'Você tem um treino em andamento. Continue e finalize com força!'
            : 'Pronto para superar seus limites hoje?'
          }
        </p>
      </div>

      {/* Motivational Quote Card */}
      <Card className="border-primary/20 bg-gradient-to-br from-primary/10 via-primary/5 to-background">
        <CardContent className="pt-6">
          <div className="flex gap-4">
            <div className="flex-shrink-0">
              <div className="w-12 h-12 rounded-full bg-primary/20 flex items-center justify-center">
                <Sparkles className="h-6 w-6 text-primary" />
              </div>
            </div>
            <div className="flex-1">
              <p className="text-lg font-medium leading-relaxed mb-2 italic">
                &ldquo;{dailyQuote.text}&rdquo;
              </p>
              <p className="text-sm text-muted-foreground">
                — {dailyQuote.author}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <div className="grid gap-3 sm:gap-4 sm:grid-cols-3">
        <Card className="border-primary bg-gradient-to-br from-primary/5 to-primary/10 hover:shadow-lg transition-shadow cursor-pointer">
          <Link href="/workout">
            <CardContent className="pt-6">
              <div className="flex flex-col items-center text-center gap-3">
                <div className="w-14 h-14 rounded-full bg-primary/20 flex items-center justify-center">
                  <Dumbbell className="h-7 w-7 text-primary" />
                </div>
                <div>
                  <h3 className="text-lg font-bold mb-1">
                    {currentSession ? 'Continuar Treino' : 'Iniciar Treino'}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    {currentSession ? 'Retome seu treino' : 'Comece agora'}
                  </p>
                </div>
              </div>
            </CardContent>
          </Link>
        </Card>

        <Card className="hover:shadow-lg transition-shadow cursor-pointer">
          <Link href="/plans">
            <CardContent className="pt-6">
              <div className="flex flex-col items-center text-center gap-3">
                <div className="w-14 h-14 rounded-full bg-primary/10 flex items-center justify-center">
                  <Calendar className="h-7 w-7 text-primary" />
                </div>
                <div>
                  <h3 className="text-lg font-bold mb-1">Ver Meu Plano</h3>
                  <p className="text-sm text-muted-foreground">
                    Seus treinos programados
                  </p>
                </div>
              </div>
            </CardContent>
          </Link>
        </Card>

        <Card className="hover:shadow-lg transition-shadow cursor-pointer">
          <Link href={user?.personalTrainerId ? `/instructor/${user.personalTrainerId}` : '/trainers'}>
            <CardContent className="pt-6">
              <div className="flex flex-col items-center text-center gap-3">
                <div className="w-14 h-14 rounded-full bg-primary/10 flex items-center justify-center">
                  <MessageCircle className="h-7 w-7 text-primary" />
                </div>
                <div>
                  <h3 className="text-lg font-bold mb-1">Falar com Personal</h3>
                  <p className="text-sm text-muted-foreground">
                    {user?.personalTrainerId ? 'Entre em contato' : 'Encontrar personal'}
                  </p>
                </div>
              </div>
            </CardContent>
          </Link>
        </Card>
      </div>

      {/* Stats Cards with Progress */}
      <div className="grid gap-3 sm:gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Treinos Totais</CardTitle>
            <Dumbbell className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoadingProgress ? '...' : progress?.totalWorkouts || 0}
            </div>
            <p className="text-xs text-muted-foreground mb-2">
              {progress?.totalSets || 0} exercícios diferentes
            </p>
            {progress?.totalWorkouts && progress.totalWorkouts > 0 && (
              <div className="w-full bg-muted rounded-full h-2">
                <div
                  className="bg-primary h-2 rounded-full transition-all"
                  style={{ width: `${Math.min((progress.totalWorkouts / 50) * 100, 100)}%` }}
                />
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Volume Total</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoadingProgress ? '...' : `${(progress?.totalVolume || 0).toFixed(0)} kg`}
            </div>
            <p className="text-xs text-muted-foreground mb-2">Peso total levantado</p>
            {progress?.totalVolume && progress.totalVolume > 0 && (
              <div className="w-full bg-muted rounded-full h-2">
                <div
                  className="bg-primary h-2 rounded-full transition-all"
                  style={{ width: `${Math.min((progress.totalVolume / 10000) * 100, 100)}%` }}
                />
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Sequência Atual</CardTitle>
            <Zap className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {isLoadingProgress ? '...' : `${progress?.currentStreak || 0} dias`}
            </div>
            <p className="text-xs text-muted-foreground mb-2">
              Maior: {progress?.longestStreak || 0} dias
            </p>
            {progress?.currentStreak !== undefined && progress?.longestStreak && progress.longestStreak > 0 && (
              <div className="w-full bg-muted rounded-full h-2">
                <div
                  className="bg-primary h-2 rounded-full transition-all"
                  style={{ width: `${Math.min((progress.currentStreak / progress.longestStreak) * 100, 100)}%` }}
                />
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Desafios Ativos</CardTitle>
            <Trophy className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{challenges?.length || 0}</div>
            <p className="text-xs text-muted-foreground mb-2">Em andamento</p>
            {challenges && challenges.length > 0 && (
              <div className="w-full bg-muted rounded-full h-2">
                <div
                  className="bg-primary h-2 rounded-full transition-all"
                  style={{ width: `${Math.min((challenges.length / 5) * 100, 100)}%` }}
                />
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Instructor Profile Card - Only show if student has a personal trainer */}
      {user?.personalTrainerId && (
        <InstructorCard trainerId={user.personalTrainerId} />
      )}

      {/* Posts from Personal Trainer or all trainers */}
      {posts && posts.length > 0 && (
        <Card>
          <CardHeader>
            <div className="flex items-center gap-3">
              <FileText className="h-5 w-5 text-primary" />
              <div>
                <CardTitle>
                  {user?.personalTrainerId
                    ? 'Dicas do seu Personal Trainer'
                    : 'Dicas de Personal Trainers'}
                </CardTitle>
                <CardDescription>
                  {user?.personalTrainerId
                    ? 'Conteúdo exclusivo compartilhado pelo seu instrutor'
                    : 'Artigos e dicas compartilhadas por personal trainers da comunidade'}
                </CardDescription>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <PostFeed posts={posts} showAuthor={true} compact={true} />
          </CardContent>
        </Card>
      )}

      {/* Weekly Training Tracker */}
      {progress?.weeklyWorkouts && progress.weeklyWorkouts.length > 0 && (
        <Card>
          <CardHeader className="cursor-pointer" onClick={() => setIsCalendarExpanded(!isCalendarExpanded)}>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Atividade de Treino</CardTitle>
                <CardDescription>
                  {isCalendarExpanded
                    ? 'Histórico completo desde a criação da conta'
                    : 'Últimos 7 dias de treino'}
                </CardDescription>
              </div>
              {isCalendarExpanded ? (
                <ChevronUp className="h-5 w-5 text-muted-foreground" />
              ) : (
                <ChevronDown className="h-5 w-5 text-muted-foreground" />
              )}
            </div>
          </CardHeader>
          <CardContent>
            {!isCalendarExpanded ? (
              // Weekly view (last 7 days)
              <div className="grid grid-cols-7 gap-2">
                {(progress?.weeklyWorkouts || []).map((day, index) => {
                  const dayShort = day.dayOfWeek.substring(0, 3);
                  const dateObj = new Date(day.date);
                  const dayNum = dateObj.getDate();

                  return (
                    <div key={index} className="flex flex-col items-center gap-1">
                      <span className="text-xs text-muted-foreground">{dayShort}</span>
                      <div
                        className={cn(
                          'w-12 h-12 rounded-lg border-2 flex items-center justify-center relative',
                          day.completed
                            ? 'bg-primary border-primary text-primary-foreground'
                            : 'bg-muted border-muted-foreground/20'
                        )}
                      >
                        {day.completed ? (
                          <Check className="h-6 w-6" />
                        ) : (
                          <span className="text-sm font-medium">{dayNum}</span>
                        )}
                      </div>
                      {day.completed && day.setsCompleted > 0 && (
                        <span className="text-xs text-muted-foreground">
                          {day.setsCompleted} séries
                        </span>
                      )}
                    </div>
                  );
                })}
              </div>
            ) : (
              // Full calendar view with monthly separation
              <div className="space-y-6 max-h-[500px] overflow-y-auto pr-2">
                {(() => {
                  const monthsData = generateFullCalendar();
                  const monthKeys = Object.keys(monthsData).reverse(); // Most recent first

                  return monthKeys.map((monthKey) => {
                    const days = monthsData[monthKey];
                    const firstDay = new Date(days[0].date);
                    const monthName = firstDay.toLocaleDateString('pt-BR', {
                      month: 'long',
                      year: 'numeric',
                    });

                    // Get the day of week for the first day (0 = Sunday)
                    const firstDayOfWeek = firstDay.getDay();

                    // Calculate total workouts in this month
                    const monthWorkouts = days.filter(d => d.completed).length;

                    // Add empty cells for days before the first day of the month
                    const calendarCells: Array<any> = [...Array(firstDayOfWeek)].map((_, i) => ({
                      isEmpty: true,
                      key: `empty-${i}`,
                    }));

                    // Add actual days
                    days.forEach((day) => {
                      calendarCells.push({ ...day, isEmpty: false });
                    });

                    // Organize into weeks
                    const weeks = [];
                    for (let i = 0; i < calendarCells.length; i += 7) {
                      weeks.push(calendarCells.slice(i, i + 7));
                    }

                    return (
                      <div key={monthKey} className="space-y-2">
                        {/* Month header */}
                        <div className="flex items-center justify-between pb-2 border-b">
                          <h3 className="text-sm font-semibold capitalize">
                            {monthName}
                          </h3>
                          <span className="text-xs text-muted-foreground">
                            {monthWorkouts} {monthWorkouts === 1 ? 'treino' : 'treinos'}
                          </span>
                        </div>

                        {/* Day headers */}
                        <div className="grid grid-cols-7 gap-1 sm:gap-2">
                          {['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'].map((day) => (
                            <div
                              key={day}
                              className="text-xs text-center text-muted-foreground font-medium py-1"
                            >
                              {day}
                            </div>
                          ))}
                        </div>

                        {/* Calendar grid */}
                        <div className="space-y-1">
                          {weeks.map((week, weekIndex) => (
                            <div key={weekIndex} className="grid grid-cols-7 gap-1 sm:gap-2">
                              {week.map((cell, cellIndex) => {
                                if (cell.isEmpty) {
                                  return <div key={cell.key} className="w-8 h-8 sm:w-10 sm:h-10" />;
                                }

                                return (
                                  <div key={cellIndex} className="flex justify-center">
                                    <div
                                      className={cn(
                                        'w-8 h-8 sm:w-10 sm:h-10 rounded-md border flex items-center justify-center relative text-xs transition-colors',
                                        cell.completed
                                          ? 'bg-primary border-primary text-primary-foreground font-semibold'
                                          : 'border-muted-foreground/20 hover:border-muted-foreground/40',
                                        cell.isToday && !cell.completed && 'border-primary border-2 font-semibold'
                                      )}
                                      title={cell.completed ? `Treino completo em ${new Date(cell.date).toLocaleDateString('pt-BR')}` : ''}
                                    >
                                      {cell.completed ? (
                                        <Check className="h-3 w-3 sm:h-4 sm:w-4" />
                                      ) : (
                                        <span className={cn('font-medium', cell.isToday && 'text-primary')}>
                                          {cell.dayNum}
                                        </span>
                                      )}
                                    </div>
                                  </div>
                                );
                              })}
                            </div>
                          ))}
                        </div>
                      </div>
                    );
                  });
                })()}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Current Session or Start Workout */}
      <Card>
        <CardHeader>
          <CardTitle>Treino de Hoje</CardTitle>
          <CardDescription>
            {currentSession
              ? 'Você tem um treino em andamento'
              : 'Comece seu treino agora'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {currentSession ? (
            <div className="space-y-4">
              <p>
                Treino iniciado há{' '}
                {Math.floor(
                  (new Date().getTime() - new Date(currentSession.startedAt).getTime()) / 60000
                )}{' '}
                minutos
              </p>
              <Link href="/workout">
                <Button>Continuar Treino</Button>
              </Link>
            </div>
          ) : (
            <Link href="/workout">
              <Button>Iniciar Treino</Button>
            </Link>
          )}
        </CardContent>
      </Card>

      {/* Recent PRs */}
      {progress?.recentPRs && progress.recentPRs.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Recordes Pessoais Recentes</CardTitle>
            <CardDescription>Seus últimos PRs conquistados</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {(progress.recentPRs || []).slice(0, 5).map((pr, index) => (
                <div key={index} className="flex items-center justify-between border-b pb-2">
                  <div>
                    <p className="font-medium">
                      {(pr as any).exerciseName || (pr as any).exercise?.name || 'Exercício'}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {new Date((pr as any).dateAchieved || pr.achievedAt).toLocaleDateString(
                        'pt-BR'
                      )}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="font-bold">
                      {(pr as any).maxLoad || pr.weight} kg × {pr.reps}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
