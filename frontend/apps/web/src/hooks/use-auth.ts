import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { api, tokenStorage } from '@/lib/api';
import { toast } from '@/components/ui/use-toast';
import type { LoginInput, SignupInput } from '@gymhero/shared';

export function useAuth() {
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: user, isLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: async () => {
      const token = await tokenStorage.getAccessToken();
      if (!token) {
        return null;
      }
      return api.auth.getMe();
    },
    retry: false,
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchOnWindowFocus: false,
    refetchOnMount: false,
  });

  const loginMutation = useMutation({
    mutationFn: (data: LoginInput) => api.auth.login(data),
    onSuccess: async (tokens) => {
      await tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken);
      await queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
      router.push('/dashboard');
      toast({
        title: 'Login realizado com sucesso!',
        description: 'Bem-vindo de volta!',
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao fazer login',
        description: error?.response?.data?.message || 'Credenciais inválidas',
      });
    },
  });

  const signupMutation = useMutation({
    mutationFn: (data: SignupInput) => api.auth.signup(data),
    onSuccess: async (tokens) => {
      await tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken);
      await queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
      router.push('/onboarding');
      toast({
        title: 'Conta criada com sucesso!',
        description: 'Bem-vindo ao TaktIQ!',
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao criar conta',
        description: error?.response?.data?.message || 'Erro ao processar sua solicitação',
      });
    },
  });

  const logoutMutation = useMutation({
    mutationFn: () => api.auth.logout(),
    onSuccess: async () => {
      await tokenStorage.clearTokens();
      queryClient.clear();
      router.push('/login');
      toast({
        title: 'Logout realizado com sucesso',
      });
    },
  });

  const refreshUser = async () => {
    await queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
  };

  return {
    user,
    isLoading,
    isAuthenticated: !!user,
    login: loginMutation.mutateAsync,
    signup: signupMutation.mutateAsync,
    logout: logoutMutation.mutateAsync,
    refreshUser,
    isLoginPending: loginMutation.isPending,
    isSignupPending: signupMutation.isPending,
  };
}
