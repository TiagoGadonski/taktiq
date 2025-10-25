import { View, Text, ScrollView, TouchableOpacity, RefreshControl } from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { Ionicons } from '@expo/vector-icons';
import { api } from '@/lib/api';
import { useAuth } from '@/hooks/use-auth';
import { useState } from 'react';

export default function HomeScreen() {
  const { user } = useAuth();
  const [refreshing, setRefreshing] = useState(false);

  const { data: progress, refetch: refetchProgress } = useQuery({
    queryKey: ['progress', 'dashboard'],
    queryFn: () => api.progress.getDashboard(),
  });

  const { data: currentSession, refetch: refetchSession } = useQuery({
    queryKey: ['sessions', 'current'],
    queryFn: () => api.sessions.getCurrent(),
  });

  const onRefresh = async () => {
    setRefreshing(true);
    await Promise.all([refetchProgress(), refetchSession()]);
    setRefreshing(false);
  };

  return (
    <ScrollView
      className="flex-1 bg-background"
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <View className="px-6 pt-12 pb-6">
        <Text className="text-3xl font-bold text-foreground">Olá, {user?.name}!</Text>
        <Text className="text-muted-foreground mt-1">Pronto para treinar hoje?</Text>
      </View>

      {/* Stats Cards */}
      <View className="px-6 space-y-4">
        <View className="flex-row space-x-4">
          <View className="flex-1 bg-card rounded-xl p-4">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Treinos</Text>
              <Ionicons name="barbell" size={20} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {progress?.totalWorkouts || 0}
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">
              {progress?.totalSets || 0} séries
            </Text>
          </View>

          <View className="flex-1 bg-card rounded-xl p-4">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Volume</Text>
              <Ionicons name="trending-up" size={20} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {(progress?.totalVolume || 0).toFixed(0)} kg
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">Total levantado</Text>
          </View>
        </View>

        <View className="flex-row space-x-4">
          <View className="flex-1 bg-card rounded-xl p-4">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Sequência</Text>
              <Ionicons name="flame" size={20} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {progress?.currentStreak || 0} dias
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">
              Maior: {progress?.longestStreak || 0}
            </Text>
          </View>

          <View className="flex-1 bg-card rounded-xl p-4">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Desafios</Text>
              <Ionicons name="trophy" size={20} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">3</Text>
            <Text className="text-muted-foreground text-xs mt-1">Em andamento</Text>
          </View>
        </View>

        {/* Workout CTA */}
        <View className="bg-gradient-to-r from-blue-500 to-purple-600 rounded-xl p-6 mt-4">
          <Text className="text-white text-xl font-bold mb-2">
            {currentSession ? 'Treino em Andamento' : 'Treino de Hoje'}
          </Text>
          <Text className="text-white/80 mb-4">
            {currentSession
              ? 'Continue de onde parou'
              : 'Comece seu treino e alcance seus objetivos'}
          </Text>
          <TouchableOpacity className="bg-white rounded-lg py-3 items-center">
            <Text className="text-blue-600 font-semibold">
              {currentSession ? 'Continuar Treino' : 'Iniciar Treino'}
            </Text>
          </TouchableOpacity>
        </View>

        {/* Recent PRs */}
        {progress?.recentPRs && progress.recentPRs.length > 0 && (
          <View className="bg-card rounded-xl p-4 mt-4">
            <Text className="text-foreground text-lg font-bold mb-4">Recordes Recentes</Text>
            {progress.recentPRs.slice(0, 3).map((pr) => (
              <View
                key={pr.id}
                className="flex-row items-center justify-between py-3 border-b border-muted"
              >
                <View className="flex-1">
                  <Text className="text-foreground font-medium">{pr.exercise.name}</Text>
                  <Text className="text-muted-foreground text-sm">
                    {new Date(pr.achievedAt).toLocaleDateString('pt-BR')}
                  </Text>
                </View>
                <Text className="text-primary font-bold">
                  {pr.weight} kg × {pr.reps}
                </Text>
              </View>
            ))}
          </View>
        )}
      </View>
    </ScrollView>
  );
}
