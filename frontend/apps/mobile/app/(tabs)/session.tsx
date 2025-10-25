import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  TextInput,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Ionicons } from '@expo/vector-icons';
import * as Haptics from 'expo-haptics';
import { api } from '@/lib/api';
import { useSession } from '@/hooks/use-session';
import { useSets } from '@/hooks/use-sets';
import type { CreateSetInput, WorkoutExercise } from '@gymhero/shared';

export default function SessionScreen() {
  const { currentSession, hasActiveSession, startSession, completeSession, isStarting } =
    useSession();
  const { createSet, deleteSet } = useSets();

  const { data: activePlan } = useQuery({
    queryKey: ['workout-plans', 'active'],
    queryFn: async () => {
      const plans = await api.workoutPlans.getAll();
      return plans.find((p) => p.isActive);
    },
  });

  const handleStartSession = async (workoutId?: string) => {
    try {
      await startSession(workoutId);
      await Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
    } catch (error: any) {
      Alert.alert('Erro', error.message);
    }
  };

  const handleCompleteSession = async () => {
    if (!currentSession) return;

    Alert.alert('Concluir Treino', 'Deseja finalizar este treino?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Concluir',
        onPress: async () => {
          try {
            await completeSession({ sessionId: currentSession.id });
            await Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
          } catch (error: any) {
            Alert.alert('Erro', error.message);
          }
        },
      },
    ]);
  };

  const handleAddSet = async (
    exerciseId: string,
    data: { reps: number; weight?: number; rpe?: number }
  ) => {
    if (!currentSession) return;

    const setNumber =
      (currentSession.sets?.filter((s) => s.exerciseId === exerciseId).length || 0) + 1;

    const setData: CreateSetInput = {
      sessionId: currentSession.id,
      exerciseId,
      setNumber,
      ...data,
    };

    try {
      await createSet(setData);
      await Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);
    } catch (error: any) {
      Alert.alert('Erro', error.message);
    }
  };

  // No active session
  if (!hasActiveSession) {
    return (
      <ScrollView className="flex-1 bg-background">
        <View className="px-6 pt-12 pb-6">
          <Text className="text-3xl font-bold text-foreground">Treino do Dia</Text>
          <Text className="text-muted-foreground mt-2">Escolha um treino para começar</Text>
        </View>

        {activePlan?.workouts && activePlan.workouts.length > 0 ? (
          <View className="px-6 space-y-4">
            {activePlan.workouts.map((workout) => (
              <TouchableOpacity
                key={workout.id}
                className="bg-card rounded-xl p-4 border border-border active:opacity-70"
                onPress={() => handleStartSession(workout.id)}
                disabled={isStarting}
              >
                <Text className="text-foreground text-lg font-bold">{workout.name}</Text>
                <Text className="text-muted-foreground text-sm mt-1">
                  {workout.exercises?.length || 0} exercícios
                </Text>
                <View className="mt-4">
                  <View className="bg-primary rounded-lg py-3 items-center">
                    <Text className="text-primary-foreground font-semibold">
                      {isStarting ? 'Iniciando...' : 'Iniciar Treino'}
                    </Text>
                  </View>
                </View>
              </TouchableOpacity>
            ))}
          </View>
        ) : (
          <View className="px-6">
            <View className="bg-card rounded-xl p-6">
              <Text className="text-foreground text-lg font-bold">Nenhum plano ativo</Text>
              <Text className="text-muted-foreground mt-2">
                Crie um plano de treino no app web primeiro.
              </Text>
            </View>
          </View>
        )}
      </ScrollView>
    );
  }

  // Active session
  const sessionDuration = Math.floor(
    (new Date().getTime() - new Date(currentSession.startedAt).getTime()) / 60000
  );

  return (
    <ScrollView className="flex-1 bg-background">
      {/* Header */}
      <View className="px-6 pt-12 pb-4">
        <Text className="text-2xl font-bold text-foreground">
          {currentSession.workout?.name || 'Treino em Andamento'}
        </Text>
        <View className="flex-row items-center gap-4 mt-2">
          <Text className="text-muted-foreground text-sm">{sessionDuration} minutos</Text>
          <Text className="text-muted-foreground text-sm">•</Text>
          <Text className="text-muted-foreground text-sm">
            {currentSession.sets?.length || 0} séries
          </Text>
        </View>
      </View>

      {/* Exercises */}
      <View className="px-6 space-y-4">
        {currentSession.workout?.exercises?.map((exercise) => (
          <ExerciseCard
            key={exercise.id}
            exercise={exercise}
            sets={currentSession.sets || []}
            onAddSet={(data) => handleAddSet(exercise.exerciseId, data)}
          />
        ))}
      </View>

      {/* Complete Button */}
      <View className="px-6 py-6">
        <TouchableOpacity
          className="bg-primary rounded-lg py-4 items-center active:opacity-80"
          onPress={handleCompleteSession}
        >
          <Text className="text-primary-foreground font-bold text-base">Concluir Treino</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
}

// Exercise Card Component
function ExerciseCard({
  exercise,
  sets,
  onAddSet,
}: {
  exercise: WorkoutExercise;
  sets: any[];
  onAddSet: (data: { reps: number; weight?: number; rpe?: number }) => void;
}) {
  const [reps, setReps] = useState('');
  const [weight, setWeight] = useState('');
  const [rpe, setRpe] = useState('');

  const exerciseSets = sets.filter((s) => s.exerciseId === exercise.exerciseId);

  const handleAdd = () => {
    if (!reps) return;

    onAddSet({
      reps: parseInt(reps),
      weight: weight ? parseFloat(weight) : undefined,
      rpe: rpe ? parseInt(rpe) : undefined,
    });

    setReps('');
    setWeight('');
    setRpe('');
  };

  return (
    <View className="bg-card rounded-xl p-4 border border-border">
      <View className="flex-row items-center justify-between mb-4">
        <View className="flex-1">
          <Text className="text-foreground text-lg font-bold">
            {exercise.exercise?.name || 'Exercise'}
          </Text>
          <Text className="text-muted-foreground text-sm">
            {exercise.targetSets} séries × {exercise.targetReps || '?'} reps
          </Text>
        </View>
        <View className="items-end">
          <Text className="text-primary text-2xl font-bold">{exerciseSets.length}</Text>
          <Text className="text-muted-foreground text-xs">de {exercise.targetSets}</Text>
        </View>
      </View>

      {/* Completed Sets */}
      {exerciseSets.map((set, index) => (
        <View
          key={set.id}
          className="flex-row items-center justify-between bg-background rounded-lg p-3 mb-2"
        >
          <View className="flex-row items-center gap-3">
            <View className="bg-primary/10 rounded-full h-8 w-8 items-center justify-center">
              <Text className="text-primary font-bold text-sm">{index + 1}</Text>
            </View>
            <Text className="text-foreground font-medium">
              {set.reps} reps{set.weight && ` × ${set.weight} kg`}
            </Text>
          </View>
          {set.rpe && <Text className="text-muted-foreground text-sm">RPE {set.rpe}</Text>}
        </View>
      ))}

      {/* Add Set Form */}
      {exerciseSets.length < exercise.targetSets && (
        <View className="border-2 border-dashed border-muted rounded-lg p-4 mt-2">
          <Text className="text-foreground font-medium mb-3">
            Série {exerciseSets.length + 1}:
          </Text>
          <View className="flex-row gap-2 mb-3">
            <View className="flex-1">
              <Text className="text-muted-foreground text-xs mb-1">Reps *</Text>
              <TextInput
                className="bg-background border border-muted rounded-lg px-3 py-2 text-foreground"
                placeholder="12"
                placeholderTextColor="#64748b"
                keyboardType="number-pad"
                value={reps}
                onChangeText={setReps}
              />
            </View>
            <View className="flex-1">
              <Text className="text-muted-foreground text-xs mb-1">Peso (kg)</Text>
              <TextInput
                className="bg-background border border-muted rounded-lg px-3 py-2 text-foreground"
                placeholder="50"
                placeholderTextColor="#64748b"
                keyboardType="decimal-pad"
                value={weight}
                onChangeText={setWeight}
              />
            </View>
            <View className="flex-1">
              <Text className="text-muted-foreground text-xs mb-1">RPE</Text>
              <TextInput
                className="bg-background border border-muted rounded-lg px-3 py-2 text-foreground"
                placeholder="8"
                placeholderTextColor="#64748b"
                keyboardType="number-pad"
                value={rpe}
                onChangeText={setRpe}
              />
            </View>
          </View>
          <TouchableOpacity
            className="bg-primary rounded-lg py-3 items-center active:opacity-80"
            onPress={handleAdd}
            disabled={!reps}
          >
            <Text className="text-primary-foreground font-semibold">Adicionar Série</Text>
          </TouchableOpacity>
        </View>
      )}
    </View>
  );
}
