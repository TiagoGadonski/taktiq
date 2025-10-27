import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ApiEndpoints } from '../api/endpoints';
import type { CompleteSessionInput, WorkoutSession } from '../types';

export interface UseSessionHookResult {
  currentSession: WorkoutSession | null;
  hasActiveSession: boolean;
  isLoading: boolean;
  startSession: (input?: string | { workoutPlanId?: string | null; workoutId?: string | null }) => Promise<WorkoutSession>;
  completeSession: (input: CompleteSessionInput) => Promise<WorkoutSession>;
  isStarting: boolean;
  isCompleting: boolean;
}

export const createUseSession = (api: ApiEndpoints) => (): UseSessionHookResult => {
  const queryClient = useQueryClient();

  const { data: currentSession, isLoading } = useQuery({
    queryKey: ['sessions', 'current'],
    queryFn: () => api.sessions.getCurrent(),
    staleTime: 30 * 1000,
    refetchInterval: 60 * 1000,
  });

  const startMutation = useMutation({
    mutationFn: (input?: string | { workoutPlanId?: string | null; workoutId?: string | null }) =>
      api.sessions.start(input),
    onSuccess: (session) => {
      queryClient.setQueryData(['sessions', 'current'], session);
      queryClient.invalidateQueries({ queryKey: ['sessions', 'history'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
    },
  });

  const completeMutation = useMutation({
    mutationFn: (input: CompleteSessionInput) => api.sessions.complete(input),
    onSuccess: (session) => {
      queryClient.setQueryData(['sessions', 'current'], session);
      queryClient.invalidateQueries({ queryKey: ['sessions'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
    },
  });

  const hasActiveSession = Boolean(currentSession && !currentSession.completedAt);

  return {
    currentSession: currentSession ?? null,
    hasActiveSession,
    isLoading,
    startSession: (input) => startMutation.mutateAsync(input),
    completeSession: (input) => completeMutation.mutateAsync(input),
    isStarting: startMutation.isPending,
    isCompleting: completeMutation.isPending,
  };
};
