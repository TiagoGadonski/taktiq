import { View, Text, ScrollView, TouchableOpacity, RefreshControl } from 'react-native';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Ionicons } from '@expo/vector-icons';
import { api } from '@/lib/api';
import type { Challenge, ChallengeStatus } from '@gymhero/shared';

export default function ChallengesScreen() {
  const [refreshing, setRefreshing] = useState(false);
  const [filter, setFilter] = useState<ChallengeStatus | 'all'>('all');

  const { data: challenges, refetch } = useQuery({
    queryKey: ['challenges', filter],
    queryFn: () =>
      filter === 'all'
        ? api.challenges.getAll()
        : api.challenges.getAll({ status: filter as ChallengeStatus }),
  });

  const onRefresh = async () => {
    setRefreshing(true);
    await refetch();
    setRefreshing(false);
  };

  const getProgressPercentage = (challenge: Challenge) => {
    return Math.min(100, (challenge.current / challenge.target) * 100);
  };

  const getStatusColor = (status: ChallengeStatus) => {
    switch (status) {
      case 'active':
        return 'bg-blue-500';
      case 'completed':
        return 'bg-green-500';
      case 'failed':
        return 'bg-red-500';
      default:
        return 'bg-muted';
    }
  };

  return (
    <ScrollView
      className="flex-1 bg-background"
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <View className="px-6 pt-12 pb-6">
        <Text className="text-3xl font-bold text-foreground">Desafios</Text>
        <Text className="text-muted-foreground mt-2">Acompanhe suas metas</Text>
      </View>

      {/* Filter Tabs */}
      <View className="px-6 mb-4">
        <ScrollView horizontal showsHorizontalScrollIndicator={false} className="flex-row gap-2">
          <TouchableOpacity
            className={`px-4 py-2 rounded-lg ${filter === 'all' ? 'bg-primary' : 'bg-card border border-border'}`}
            onPress={() => setFilter('all')}
          >
            <Text
              className={`font-medium ${filter === 'all' ? 'text-primary-foreground' : 'text-foreground'}`}
            >
              Todos
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            className={`px-4 py-2 rounded-lg ${filter === 'active' ? 'bg-primary' : 'bg-card border border-border'}`}
            onPress={() => setFilter('active')}
          >
            <Text
              className={`font-medium ${filter === 'active' ? 'text-primary-foreground' : 'text-foreground'}`}
            >
              Ativos
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            className={`px-4 py-2 rounded-lg ${filter === 'completed' ? 'bg-primary' : 'bg-card border border-border'}`}
            onPress={() => setFilter('completed')}
          >
            <Text
              className={`font-medium ${filter === 'completed' ? 'text-primary-foreground' : 'text-foreground'}`}
            >
              Concluídos
            </Text>
          </TouchableOpacity>
        </ScrollView>
      </View>

      {/* Challenges List */}
      <View className="px-6 space-y-4 pb-6">
        {challenges && challenges.length > 0 ? (
          challenges.map((challenge) => {
            const progress = getProgressPercentage(challenge);
            const daysRemaining = Math.ceil(
              (new Date(challenge.endDate).getTime() - new Date().getTime()) /
                (1000 * 60 * 60 * 24)
            );

            return (
              <View key={challenge.id} className="bg-card rounded-xl p-4 border border-border">
                <View className="flex-row items-start justify-between mb-3">
                  <View className="flex-1">
                    <View className="flex-row items-center gap-2 mb-1">
                      <Ionicons name="trophy" size={20} color="#3b82f6" />
                      <Text className="text-foreground text-lg font-bold">
                        {challenge.name}
                      </Text>
                    </View>
                    {challenge.description && (
                      <Text className="text-muted-foreground text-sm">
                        {challenge.description}
                      </Text>
                    )}
                  </View>
                </View>

                {/* Progress Bar */}
                <View className="mb-4">
                  <View className="flex-row justify-between mb-2">
                    <Text className="text-muted-foreground text-sm">Progresso</Text>
                    <Text className="text-foreground font-medium text-sm">
                      {progress.toFixed(0)}%
                    </Text>
                  </View>
                  <View className="h-2 bg-muted rounded-full overflow-hidden">
                    <View
                      className="h-full bg-primary"
                      style={{ width: `${progress}%` }}
                    />
                  </View>
                </View>

                {/* Stats */}
                <View className="flex-row justify-between mb-3">
                  <View>
                    <Text className="text-muted-foreground text-xs">Atual</Text>
                    <Text className="text-foreground font-bold">
                      {challenge.current} {challenge.unit}
                    </Text>
                  </View>
                  <View>
                    <Text className="text-muted-foreground text-xs">Meta</Text>
                    <Text className="text-foreground font-bold">
                      {challenge.target} {challenge.unit}
                    </Text>
                  </View>
                  {challenge.status === 'active' && (
                    <View>
                      <Text className="text-muted-foreground text-xs">Dias restantes</Text>
                      <Text className="text-foreground font-bold">{daysRemaining}</Text>
                    </View>
                  )}
                </View>

                {/* Status Badge */}
                <View className={`${getStatusColor(challenge.status)} rounded-full px-3 py-1 self-start`}>
                  <Text className="text-white text-xs font-medium">
                    {challenge.status === 'active' && 'Ativo'}
                    {challenge.status === 'completed' && 'Concluído'}
                    {challenge.status === 'failed' && 'Falhou'}
                  </Text>
                </View>
              </View>
            );
          })
        ) : (
          <View className="bg-card rounded-xl p-8 items-center">
            <Ionicons name="trophy-outline" size={48} color="#64748b" />
            <Text className="text-foreground text-lg font-bold mt-4">
              Nenhum desafio encontrado
            </Text>
            <Text className="text-muted-foreground text-center mt-2">
              Crie desafios no app web para acompanhá-los aqui
            </Text>
          </View>
        )}
      </View>
    </ScrollView>
  );
}
