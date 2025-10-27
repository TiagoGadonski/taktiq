import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { CreateSetInput, UpdateSetInput } from '../validation/workout';

export function createUseSets(api: ReturnType<any>) {
  return function useSets(sessionId?: string) {
    const queryClient = useQueryClient();

    const createSetMutation = useMutation({
      mutationFn: (data: CreateSetInput) => api.sets.create(data),
      onMutate: async (newSet) => {
        // Cancel outgoing refetches
        await queryClient.cancelQueries({ queryKey: ['sessions', 'current'] });

        // Snapshot previous value
        const previousSession = queryClient.getQueryData(['sessions', 'current']);

        // Optimistically update
        queryClient.setQueryData(['sessions', 'current'], (old: any) => {
          if (!old) return old;
          return {
            ...old,
            sets: [...(old.sets || []), { ...newSet, id: 'temp-' + Date.now() }],
          };
        });

        return { previousSession };
      },
      onError: (err, newSet, context) => {
        // Rollback on error
        if (context?.previousSession) {
          queryClient.setQueryData(['sessions', 'current'], context.previousSession);
        }
      },
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
        queryClient.invalidateQueries({ queryKey: ['progress'] });
      },
    });

    const updateSetMutation = useMutation({
      mutationFn: ({ id, data }: { id: string; data: UpdateSetInput }) =>
        api.sets.update(id, data),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
      },
    });

    const deleteSetMutation = useMutation({
      mutationFn: (id: string) => api.sets.delete(id),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
      },
    });

    return {
      createSet: createSetMutation.mutateAsync,
      updateSet: updateSetMutation.mutateAsync,
      deleteSet: deleteSetMutation.mutateAsync,
      isCreating: createSetMutation.isPending,
      isUpdating: updateSetMutation.isPending,
      isDeleting: deleteSetMutation.isPending,
    };
  };
}
