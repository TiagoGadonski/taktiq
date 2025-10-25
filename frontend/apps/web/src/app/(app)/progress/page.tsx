'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { TrendingUp, Award, Dumbbell, Flame } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import type { VolumeByWeek, VolumeByMuscle } from '@gymhero/shared';

export default function ProgressPage() {
  const { data: dashboard } = useQuery({
    queryKey: ['progress', 'dashboard'],
    queryFn: () => api.progress.getDashboard(),
  });

  const muscleGroupLabels: Record<string, string> = {
    chest: 'Peito',
    back: 'Costas',
    legs: 'Pernas',
    shoulders: 'Ombros',
    arms: 'Braços',
    core: 'Core',
    full_body: 'Corpo Todo',
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Progresso</h1>
        <p className="text-muted-foreground">
          Acompanhe sua evolução e visualize suas conquistas
        </p>
      </div>

      {/* Stats Overview */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total de Treinos</CardTitle>
            <Dumbbell className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboard?.totalWorkouts || 0}</div>
            <p className="text-xs text-muted-foreground">
              {dashboard?.totalSets || 0} séries totais
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Volume Total</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {(dashboard?.totalVolume || 0).toFixed(0)} kg
            </div>
            <p className="text-xs text-muted-foreground">Peso total levantado</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Sequência Atual</CardTitle>
            <Flame className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboard?.currentStreak || 0} dias</div>
            <p className="text-xs text-muted-foreground">
              Maior: {dashboard?.longestStreak || 0} dias
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Recordes Pessoais</CardTitle>
            <Award className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboard?.recentPRs?.length || 0}</div>
            <p className="text-xs text-muted-foreground">Últimos 30 dias</p>
          </CardContent>
        </Card>
      </div>

      {/* Volume Over Time Chart */}
      {dashboard?.weeklyVolume && dashboard.weeklyVolume.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Volume por Semana</CardTitle>
            <CardDescription>Acompanhe a evolução do seu volume de treino</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={dashboard.weeklyVolume}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="week" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="volume"
                  stroke="#3b82f6"
                  strokeWidth={2}
                  name="Volume (kg)"
                />
                <Line
                  type="monotone"
                  dataKey="sets"
                  stroke="#10b981"
                  strokeWidth={2}
                  name="Séries"
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Volume by Muscle Group */}
      {dashboard?.volumeByMuscle && dashboard.volumeByMuscle.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Volume por Grupo Muscular</CardTitle>
            <CardDescription>Distribuição do volume entre grupos musculares</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart
                data={dashboard.volumeByMuscle.map((item) => ({
                  ...item,
                  muscleGroupLabel: muscleGroupLabels[item.muscleGroup] || item.muscleGroup,
                }))}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="muscleGroupLabel" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="volume" fill="#3b82f6" name="Volume (kg)" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Recent PRs */}
      {dashboard?.recentPRs && dashboard.recentPRs.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Recordes Pessoais Recentes</CardTitle>
            <CardDescription>Suas últimas conquistas</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {dashboard.recentPRs.map((pr, index) => (
                <div
                  key={`${pr.exerciseId}-${pr.reps}-${index}`}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                      <Award className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium">{pr.exerciseName}</p>
                      <p className="text-sm text-muted-foreground">
                        {new Date(pr.achievedAt || pr.dateAchieved || '').toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold">
                      {pr.weight || pr.maxLoad} kg × {pr.reps}
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
