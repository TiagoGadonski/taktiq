'use client';

import { useState, useEffect, Suspense } from 'react';
import { useForm } from 'react-hook-form';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { useToast } from '@/components/ui/use-toast';
import { ArrowLeft, CheckCircle2 } from 'lucide-react';

interface ResetPasswordForm {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [email, setEmail] = useState('');

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<ResetPasswordForm>();

  const newPassword = watch('newPassword');

  useEffect(() => {
    const emailParam = searchParams.get('email');
    if (emailParam) {
      setEmail(emailParam);
    }
  }, [searchParams]);

  const onSubmit = async (data: ResetPasswordForm) => {
    if (data.newPassword !== data.confirmPassword) {
      toast({
        variant: 'destructive',
        title: 'Erro',
        description: 'As senhas não coincidem',
      });
      return;
    }

    setIsLoading(true);
    try {
      await api.auth.resetPassword({
        token: data.token,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      });

      setIsSuccess(true);
      toast({
        title: 'Senha redefinida!',
        description: 'Sua senha foi alterada com sucesso.',
      });

      // Redirect to login after 2 seconds
      setTimeout(() => {
        router.push('/login');
      }, 2000);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro',
        description: error.message || 'Código inválido ou expirado. Tente novamente.',
      });
    } finally {
      setIsLoading(false);
    }
  };

  if (isSuccess) {
    return (
      <div className="relative flex min-h-screen items-center justify-center overflow-hidden p-4 bg-background">
        {/* Modern gradient background */}
        <div className="absolute inset-0 bg-gradient-to-br from-background via-background to-primary/5" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-primary/20 via-transparent to-transparent" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,_var(--tw-gradient-stops))] from-primary/10 via-transparent to-transparent" />

        {/* Gym pattern overlay */}
        <div className="absolute inset-0 opacity-5 gym-pattern" />

        {/* Success card */}
        <Card className="relative w-full max-w-md glass-card border-primary/20 shadow-2xl">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center space-y-4">
              <div className="rounded-full bg-primary/10 p-3">
                <CheckCircle2 className="h-12 w-12 text-primary" />
              </div>
              <div className="space-y-2">
                <h3 className="text-2xl font-bold">Senha redefinida!</h3>
                <p className="text-muted-foreground">
                  Sua senha foi alterada com sucesso. Redirecionando para o login...
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden p-4 bg-background">
      {/* Modern gradient background */}
      <div className="absolute inset-0 bg-gradient-to-br from-background via-background to-primary/5" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-primary/20 via-transparent to-transparent" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,_var(--tw-gradient-stops))] from-primary/10 via-transparent to-transparent" />

      {/* Gym pattern overlay */}
      <div className="absolute inset-0 opacity-5 gym-pattern" />

      {/* Reset Password card */}
      <Card className="relative w-full max-w-md glass-card border-primary/20 shadow-2xl">
        <CardHeader className="space-y-3 pb-6">
          <div className="flex justify-center">
            <TaktIQLogo width={200} height={57} className="drop-shadow-lg" />
          </div>
          <CardTitle className="text-center text-2xl font-bold">Redefinir senha</CardTitle>
          <CardDescription className="text-center">
            {email && (
              <>
                Código enviado para <span className="font-medium">{email}</span>
                <br />
              </>
            )}
            Digite o código de 6 dígitos e sua nova senha
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="token">Código de recuperação</Label>
              <Input
                id="token"
                type="text"
                placeholder="123456"
                maxLength={6}
                {...register('token', {
                  required: 'O código é obrigatório',
                  pattern: {
                    value: /^\d{6}$/,
                    message: 'O código deve ter 6 dígitos',
                  },
                })}
                aria-invalid={errors.token ? 'true' : 'false'}
                className="h-11 text-center text-2xl tracking-widest"
              />
              {errors.token && (
                <p className="text-sm text-destructive">{errors.token.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="newPassword">Nova senha</Label>
              <Input
                id="newPassword"
                type="password"
                placeholder="••••••••"
                {...register('newPassword', {
                  required: 'A nova senha é obrigatória',
                  minLength: {
                    value: 6,
                    message: 'A senha deve ter pelo menos 6 caracteres',
                  },
                })}
                aria-invalid={errors.newPassword ? 'true' : 'false'}
                className="h-11"
              />
              {errors.newPassword && (
                <p className="text-sm text-destructive">{errors.newPassword.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirmar senha</Label>
              <Input
                id="confirmPassword"
                type="password"
                placeholder="••••••••"
                {...register('confirmPassword', {
                  required: 'A confirmação de senha é obrigatória',
                  validate: (value) =>
                    value === newPassword || 'As senhas não coincidem',
                })}
                aria-invalid={errors.confirmPassword ? 'true' : 'false'}
                className="h-11"
              />
              {errors.confirmPassword && (
                <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
              )}
            </div>

            <Button
              type="submit"
              className="w-full h-11 font-semibold hover-lift tap-scale"
              disabled={isLoading}
            >
              {isLoading ? 'Redefinindo...' : 'Redefinir senha'}
            </Button>
          </form>

          <div className="flex items-center justify-between text-sm">
            <Link
              href="/login"
              className="text-primary hover:underline font-medium inline-flex items-center gap-1"
            >
              <ArrowLeft className="h-3.5 w-3.5" />
              Voltar para login
            </Link>
            <Link href="/forgot-password" className="text-muted-foreground hover:text-primary">
              Reenviar código
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<div>Carregando...</div>}>
      <ResetPasswordForm />
    </Suspense>
  );
}
