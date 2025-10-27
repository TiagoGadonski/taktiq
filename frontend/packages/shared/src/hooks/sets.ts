import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { ApiEndpoints } from '../api/endpoints';
import type { CreateSetInput } from '../types';

export interface UseSetsHookResult {
  createSet: (input: CreateSetInput) => Promise<void>;
  deleteSet: (setId: string) => Promise<void>;
  isCreating: boolean;
  isDeleting: boolean;
}

export const createUseSets = (api: ApiEndpoints) => (): UseSetsHookResult => {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (input: CreateSetInput) => api.sets.create(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (setId: string) => api.sets.delete(setId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
    },
  });

  return {
    createSet: async (input: CreateSetInput) => {
      await createMutation.mutateAsync(input);
    },
    deleteSet: async (setId: string) => {
      await deleteMutation.mutateAsync(setId);
    },
    isCreating: createMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
};
