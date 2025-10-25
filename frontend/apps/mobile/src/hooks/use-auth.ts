import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter, useSegments } from 'expo-router';
import { api, tokenStorage } from '@/lib/api';
import type { LoginInput, SignupInput } from '@gymhero/shared';
import { useEffect } from 'react';

export function useAuth() {
  const router = useRouter();
  const segments = useSegments();
  const queryClient = useQueryClient();

  const { data: user, isLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => api.auth.getMe(),
    retry: false,
    staleTime: 5 * 60 * 1000,
  });

  const loginMutation = useMutation({
    mutationFn: (data: LoginInput) => api.auth.login(data),
    onSuccess: async (tokens) => {
      await tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken);
      await queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
    },
  });

  const signupMutation = useMutation({
    mutationFn: (data: SignupInput) => api.auth.signup(data),
    onSuccess: async (tokens) => {
      await tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken);
      await queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
    },
  });

  const logoutMutation = useMutation({
    mutationFn: () => api.auth.logout(),
    onSuccess: async () => {
      await tokenStorage.clearTokens();
      queryClient.clear();
    },
  });

  // Auto-redirect based on auth state
  useEffect(() => {
    if (isLoading) return;

    const inAuthGroup = segments[0] === '(auth)';

    if (!user && !inAuthGroup) {
      router.replace('/(auth)/login');
    } else if (user && inAuthGroup) {
      router.replace('/(tabs)');
    }
  }, [user, segments, isLoading]);

  return {
    user,
    isLoading,
    isAuthenticated: !!user,
    login: loginMutation.mutateAsync,
    signup: signupMutation.mutateAsync,
    logout: logoutMutation.mutateAsync,
    isLoginPending: loginMutation.isPending,
    isSignupPending: signupMutation.isPending,
  };
}
