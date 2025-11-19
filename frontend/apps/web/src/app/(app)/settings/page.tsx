'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Lock, Moon, Sun, Bell, ArrowLeft, Save, Loader2, Shield } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Switch } from '@/components/ui/switch';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { useTheme } from 'next-themes';
import { Separator } from '@/components/ui/separator';

const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, 'Senha atual é obrigatória'),
  newPassword: z.string().min(6, 'A nova senha deve ter pelo menos 6 caracteres'),
  confirmPassword: z.string().min(1, 'Confirmação de senha é obrigatória'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'As senhas não coincidem',
  path: ['confirmPassword'],
});

type ChangePasswordInput = z.infer<typeof changePasswordSchema>;

export default function SettingsPage() {
  const { toast } = useToast();
  const { theme, setTheme } = useTheme();
  const router = useRouter();
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [notificationSettings, setNotificationSettings] = useState({
    workoutReminders: true,
    challengeUpdates: true,
    friendRequests: true,
    newMessages: true,
    weeklyProgress: true,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ChangePasswordInput>({
    resolver: zodResolver(changePasswordSchema),
  });

  const onSubmitPassword = async (data: ChangePasswordInput) => {
    setIsChangingPassword(true);
    try {
      await apiClient.post('/me/change-password', {
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      });

      toast({
        title: 'Senha alterada com sucesso!',
        description: 'Sua senha foi atualizada.',
      });

      reset();
    } catch (error: any) {
      toast({
        title: 'Erro ao alterar senha',
        description: error.response?.data?.message || 'Não foi possível alterar a senha.',
        variant: 'destructive',
      });
    } finally {
      setIsChangingPassword(false);
    }
  };

  const handleNotificationChange = (key: string, value: boolean) => {
    setNotificationSettings(prev => ({
      ...prev,
      [key]: value,
    }));

    // TODO: Save to backend
    toast({
      title: 'Preferências atualizadas',
      description: 'Suas preferências de notificação foram salvas.',
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
            className="hover-lift tap-scale"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <Shield className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Configurações
          </h1>
        </div>
        <p className="text-muted-foreground ml-14">
          Gerencie suas preferências e segurança da conta
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Security Settings */}
        <Card className="glass border-primary/20 hover-lift">
          <CardHeader>
            <div className="flex items-center gap-2">
              <Lock className="h-5 w-5 text-primary" />
              <CardTitle>Segurança</CardTitle>
            </div>
            <CardDescription>Altere sua senha para manter sua conta segura</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmitPassword)} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="currentPassword">Senha Atual</Label>
                <Input
                  id="currentPassword"
                  type="password"
                  {...register('currentPassword')}
                  className="glass"
                />
                {errors.currentPassword && (
                  <p className="text-sm text-destructive">{errors.currentPassword.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="newPassword">Nova Senha</Label>
                <Input
                  id="newPassword"
                  type="password"
                  {...register('newPassword')}
                  className="glass"
                />
                {errors.newPassword && (
                  <p className="text-sm text-destructive">{errors.newPassword.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Confirmar Nova Senha</Label>
                <Input
                  id="confirmPassword"
                  type="password"
                  {...register('confirmPassword')}
                  className="glass"
                />
                {errors.confirmPassword && (
                  <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
                )}
              </div>

              <Button
                type="submit"
                disabled={isChangingPassword}
                className="w-full hover-lift tap-scale"
              >
                {isChangingPassword ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Alterando...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    Alterar Senha
                  </>
                )}
              </Button>
            </form>
          </CardContent>
        </Card>

        {/* Appearance Settings */}
        <Card className="glass border-primary/20 hover-lift">
          <CardHeader>
            <div className="flex items-center gap-2">
              {theme === 'dark' ? (
                <Moon className="h-5 w-5 text-primary" />
              ) : (
                <Sun className="h-5 w-5 text-primary" />
              )}
              <CardTitle>Aparência</CardTitle>
            </div>
            <CardDescription>Personalize a aparência do aplicativo</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Tema Escuro</Label>
                <p className="text-sm text-muted-foreground">
                  Alterne entre modo claro e escuro
                </p>
              </div>
              <Switch
                checked={theme === 'dark'}
                onCheckedChange={(checked) => setTheme(checked ? 'dark' : 'light')}
              />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Notification Preferences */}
      <Card className="glass border-primary/20 hover-lift">
        <CardHeader>
          <div className="flex items-center gap-2">
            <Bell className="h-5 w-5 text-primary" />
            <CardTitle>Notificações</CardTitle>
          </div>
          <CardDescription>Escolha quais notificações você deseja receber</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Lembretes de Treino</Label>
              <p className="text-sm text-muted-foreground">
                Receba lembretes quando for hora de treinar
              </p>
            </div>
            <Switch
              checked={notificationSettings.workoutReminders}
              onCheckedChange={(checked) => handleNotificationChange('workoutReminders', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Atualizações de Desafios</Label>
              <p className="text-sm text-muted-foreground">
                Notificações sobre progresso em desafios
              </p>
            </div>
            <Switch
              checked={notificationSettings.challengeUpdates}
              onCheckedChange={(checked) => handleNotificationChange('challengeUpdates', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Solicitações de Amizade</Label>
              <p className="text-sm text-muted-foreground">
                Seja notificado de novas solicitações de amizade
              </p>
            </div>
            <Switch
              checked={notificationSettings.friendRequests}
              onCheckedChange={(checked) => handleNotificationChange('friendRequests', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Novas Mensagens</Label>
              <p className="text-sm text-muted-foreground">
                Receba notificações de mensagens
              </p>
            </div>
            <Switch
              checked={notificationSettings.newMessages}
              onCheckedChange={(checked) => handleNotificationChange('newMessages', checked)}
            />
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Resumo Semanal</Label>
              <p className="text-sm text-muted-foreground">
                Receba um resumo do seu progresso semanal
              </p>
            </div>
            <Switch
              checked={notificationSettings.weeklyProgress}
              onCheckedChange={(checked) => handleNotificationChange('weeklyProgress', checked)}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
