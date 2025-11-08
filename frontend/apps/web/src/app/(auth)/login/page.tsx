'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { loginSchema, type LoginInput } from '@gymhero/shared';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { Dumbbell, Zap, Trophy, Loader2 } from 'lucide-react';

export default function LoginPage() {
  const { login, isLoginPending } = useAuth();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginInput) => {
    await login(data);
  };

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden p-4 bg-background">
      {/* Animated modern gradient background */}
      <div className="absolute inset-0 gradient-animated opacity-30" />
      <div className="absolute inset-0 bg-gradient-to-br from-background via-background to-primary/10" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-primary/30 via-transparent to-transparent" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,_var(--tw-gradient-stops))] from-primary/20 via-transparent to-transparent" />

      {/* Gym pattern overlay */}
      <div className="absolute inset-0 opacity-5 gym-pattern" />

      {/* Floating icons decoration */}
      <div className="absolute top-20 left-10 opacity-10 hidden lg:block">
        <Dumbbell className="h-24 w-24 text-primary animate-pulse" />
      </div>
      <div className="absolute bottom-20 right-10 opacity-10 hidden lg:block">
        <Trophy className="h-20 w-20 text-primary animate-pulse" style={{ animationDelay: '1s' }} />
      </div>
      <div className="absolute top-1/2 right-20 opacity-10 hidden lg:block">
        <Zap className="h-16 w-16 text-primary animate-pulse" style={{ animationDelay: '0.5s' }} />
      </div>

      {/* Login card */}
      <Card className="relative w-full max-w-md glass-strong border-primary/30 shadow-2xl hover-lift">
        <CardHeader className="space-y-4 pb-6">
          <div className="flex justify-center relative">
            <div className="absolute inset-0 blur-2xl opacity-20 bg-primary rounded-full" />
            <TaktIQLogo width={200} height={57} className="drop-shadow-lg relative z-10" />
          </div>
          <div className="space-y-2">
            <CardTitle className="text-center text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text">
              Bem-vindo de volta!
            </CardTitle>
            <CardDescription className="text-center text-base">
              Entre com sua conta e continue sua jornada fitness
            </CardDescription>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email" className="text-sm font-medium">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="seu@email.com"
                {...register('email')}
                aria-invalid={errors.email ? 'true' : 'false'}
                className="h-12 border-border/50 focus:border-primary/50 transition-colors"
                disabled={isLoginPending}
              />
              {errors.email && (
                <p className="text-sm text-destructive flex items-center gap-1">
                  <span className="text-xs">⚠</span> {errors.email.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password" className="text-sm font-medium">Senha</Label>
                <Link
                  href="/forgot-password"
                  className="text-sm text-primary hover:text-primary/80 transition-colors font-medium"
                >
                  Esqueceu a senha?
                </Link>
              </div>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                {...register('password')}
                aria-invalid={errors.password ? 'true' : 'false'}
                className="h-12 border-border/50 focus:border-primary/50 transition-colors"
                disabled={isLoginPending}
              />
              {errors.password && (
                <p className="text-sm text-destructive flex items-center gap-1">
                  <span className="text-xs">⚠</span> {errors.password.message}
                </p>
              )}
            </div>

            <Button
              type="submit"
              className="w-full h-12 text-base font-semibold hover-lift tap-scale shadow-lg hover:shadow-xl transition-all"
              disabled={isLoginPending}
            >
              {isLoginPending ? (
                <>
                  <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                  Entrando...
                </>
              ) : (
                <>
                  <Zap className="mr-2 h-5 w-5" />
                  Entrar
                </>
              )}
            </Button>
          </form>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t border-border/50" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-card px-2 text-muted-foreground">ou</span>
            </div>
          </div>

          <div className="text-center">
            <p className="text-sm text-muted-foreground mb-2">
              Novo no TaktIQ?
            </p>
            <Link href="/signup">
              <Button variant="outline" className="w-full h-11 font-medium hover-lift tap-scale border-primary/30 hover:border-primary/50 hover:bg-primary/5">
                Criar uma Conta Gratuita
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
