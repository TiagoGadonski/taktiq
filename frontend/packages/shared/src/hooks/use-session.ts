import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ApiClient } from '../api/client';
import type { WorkoutSession } from '../types';
import type { StartSessionInput, CompleteSessionInput } from '../validation/workout';

export function createUseSession(api: ReturnType<any>) {
  return function useSession() {
    const queryClient = useQueryClient();

    const { data: currentSession, isLoading } = useQuery({
      queryKey: ['sessions', 'current'],
      queryFn: () => api.sessions.getCurrent(),
      refetchInterval: 30000, // Refresh every 30 seconds
    });

    const startSessionMutation = useMutation({
      mutationFn: ({ workoutPlanId, workoutId }: { workoutPlanId?: string; workoutId?: string }) =>
        api.sessions.start(workoutPlanId, workoutId),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
        queryClient.invalidateQueries({ queryKey: ['progress', 'dashboard'] });
      },
    });

    const completeSessionMutation = useMutation({
      mutationFn: ({ sessionId, notes }: { sessionId: string; notes?: string }) =>
        api.sessions.complete(sessionId, notes),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions'] });
        queryClient.invalidateQueries({ queryKey: ['progress'] });
      },
    });

    const cancelSessionMutation = useMutation({
      mutationFn: (sessionId: string) => api.sessions.cancel(sessionId),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions'] });
      },
    });

    return {
      currentSession,
      isLoading,
      hasActiveSession: !!currentSession,
      startSession: startSessionMutation.mutateAsync,
      completeSession: completeSessionMutation.mutateAsync,
      cancelSession: cancelSessionMutation.mutateAsync,
      isStarting: startSessionMutation.isPending,
      isCompleting: completeSessionMutation.isPending,
    };
  };
}
