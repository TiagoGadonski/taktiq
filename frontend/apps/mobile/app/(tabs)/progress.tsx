import { View, Text, ScrollView, RefreshControl } from 'react-native';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Ionicons } from '@expo/vector-icons';
import { api } from '@/lib/api';

export default function ProgressScreen() {
  const [refreshing, setRefreshing] = useState(false);

  const { data: dashboard, refetch } = useQuery({
    queryKey: ['progress', 'dashboard'],
    queryFn: () => api.progress.getDashboard(),
  });

  const onRefresh = async () => {
    setRefreshing(true);
    await refetch();
    setRefreshing(false);
  };

  return (
    <ScrollView
      className="flex-1 bg-background"
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <View className="px-6 pt-12 pb-6">
        <Text className="text-3xl font-bold text-foreground">Progresso</Text>
        <Text className="text-muted-foreground mt-2">Acompanhe sua evolução</Text>
      </View>

      {/* Stats Grid */}
      <View className="px-6 space-y-4">
        <View className="flex-row gap-4">
          <View className="flex-1 bg-card rounded-xl p-4 border border-border">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Treinos</Text>
              <Ionicons name="barbell" size={16} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {dashboard?.totalWorkouts || 0}
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">
              {dashboard?.totalSets || 0} séries
            </Text>
          </View>

          <View className="flex-1 bg-card rounded-xl p-4 border border-border">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Volume</Text>
              <Ionicons name="trending-up" size={16} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {(dashboard?.totalVolume || 0).toFixed(0)}
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">kg total</Text>
          </View>
        </View>

        <View className="flex-row gap-4">
          <View className="flex-1 bg-card rounded-xl p-4 border border-border">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Sequência</Text>
              <Ionicons name="flame" size={16} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {dashboard?.currentStreak || 0}
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">dias</Text>
          </View>

          <View className="flex-1 bg-card rounded-xl p-4 border border-border">
            <View className="flex-row items-center justify-between mb-2">
              <Text className="text-muted-foreground text-sm">Maior</Text>
              <Ionicons name="flame" size={16} color="#64748b" />
            </View>
            <Text className="text-foreground text-2xl font-bold">
              {dashboard?.longestStreak || 0}
            </Text>
            <Text className="text-muted-foreground text-xs mt-1">dias</Text>
          </View>
        </View>
      </View>

      {/* Recent PRs */}
      {dashboard?.recentPRs && dashboard.recentPRs.length > 0 && (
        <View className="px-6 mt-6">
          <Text className="text-foreground text-xl font-bold mb-4">Recordes Recentes</Text>
          <View className="space-y-3">
            {dashboard.recentPRs.map((pr) => (
              <View key={pr.id} className="bg-card rounded-xl p-4 border border-border">
                <View className="flex-row items-center justify-between">
                  <View className="flex-row items-center gap-3 flex-1">
                    <View className="bg-primary/10 rounded-full h-10 w-10 items-center justify-center">
                      <Ionicons name="trophy" size={20} color="#3b82f6" />
                    </View>
                    <View className="flex-1">
                      <Text className="text-foreground font-bold">{pr.exercise.name}</Text>
                      <Text className="text-muted-foreground text-sm">
                        {new Date(pr.achievedAt).toLocaleDateString('pt-BR')}
                      </Text>
                    </View>
                  </View>
                  <View className="items-end">
                    <Text className="text-primary text-lg font-bold">
                      {pr.weight} kg
                    </Text>
                    <Text className="text-muted-foreground text-sm">{pr.reps} reps</Text>
                  </View>
                </View>
              </View>
            ))}
          </View>
        </View>
      )}

      {/* Weekly Volume */}
      {dashboard?.weeklyVolume && dashboard.weeklyVolume.length > 0 && (
        <View className="px-6 mt-6 pb-6">
          <Text className="text-foreground text-xl font-bold mb-4">Volume Semanal</Text>
          <View className="bg-card rounded-xl p-4 border border-border">
            {dashboard.weeklyVolume.slice(-4).map((week, index) => (
              <View
                key={week.week}
                className={`flex-row items-center justify-between py-3 ${index !== 3 ? 'border-b border-border' : ''}`}
              >
                <Text className="text-muted-foreground">{week.week}</Text>
                <View className="items-end">
                  <Text className="text-foreground font-bold">{week.volume.toFixed(0)} kg</Text>
                  <Text className="text-muted-foreground text-xs">{week.sets} séries</Text>
                </View>
              </View>
            ))}
          </View>
        </View>
      )}

      {/* Volume by Muscle Group */}
      {dashboard?.volumeByMuscle && dashboard.volumeByMuscle.length > 0 && (
        <View className="px-6 pb-6">
          <Text className="text-foreground text-xl font-bold mb-4">Volume por Músculo</Text>
          <View className="bg-card rounded-xl p-4 border border-border">
            {dashboard.volumeByMuscle
              .sort((a, b) => b.volume - a.volume)
              .slice(0, 5)
              .map((muscle, index) => {
                const maxVolume = Math.max(...dashboard.volumeByMuscle.map((m) => m.volume));
                const percentage = (muscle.volume / maxVolume) * 100;

                const muscleLabels: Record<string, string> = {
                  chest: 'Peito',
                  back: 'Costas',
                  legs: 'Pernas',
                  shoulders: 'Ombros',
                  arms: 'Braços',
                  core: 'Core',
                  full_body: 'Corpo Todo',
                };

                return (
                  <View
                    key={muscle.muscleGroup}
                    className={`py-3 ${index !== 4 && index !== dashboard.volumeByMuscle.length - 1 ? 'border-b border-border' : ''}`}
                  >
                    <View className="flex-row items-center justify-between mb-2">
                      <Text className="text-foreground font-medium">
                        {muscleLabels[muscle.muscleGroup] || muscle.muscleGroup}
                      </Text>
                      <Text className="text-muted-foreground text-sm">
                        {muscle.volume.toFixed(0)} kg
                      </Text>
                    </View>
                    <View className="h-2 bg-muted rounded-full overflow-hidden">
                      <View
                        className="h-full bg-primary"
                        style={{ width: `${percentage}%` }}
                      />
                    </View>
                  </View>
                );
              })}
          </View>
        </View>
      )}
    </ScrollView>
  );
}
