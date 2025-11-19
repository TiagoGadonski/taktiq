'use client';

import { useEffect, useState, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { Dumbbell, Zap, Trophy, Loader2, CheckCircle, Mail, UserPlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { useToast } from '@/hooks/use-toast';
import { apiClient } from '@/lib/api';

function ActivateContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const { toast } = useToast();
  const [token, setToken] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [workoutLocation, setWorkoutLocation] = useState('0');
  const [isActivating, setIsActivating] = useState(false);
  const [isValidating, setIsValidating] = useState(true);
  const [isValidToken, setIsValidToken] = useState(false);

  useEffect(() => {
    const tokenParam = searchParams.get('token');
    if (tokenParam) {
      setToken(tokenParam);
      setIsValidToken(true);
      setIsValidating(false);
    } else {
      setIsValidating(false);
      toast({
        title: 'Link inválido',
        description: 'O link de ativação está incompleto ou inválido.',
        variant: 'destructive',
      });
    }
  }, [searchParams, toast]);

  const handleActivate = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!token) {
      toast({
        title: 'Erro',
        description: 'Token de ativação não encontrado.',
        variant: 'destructive',
      });
      return;
    }

    if (!name.trim()) {
      toast({
        title: 'Erro',
        description: 'Por favor, insira seu nome.',
        variant: 'destructive',
      });
      return;
    }

    if (password.length < 6) {
      toast({
        title: 'Erro',
        description: 'A senha deve ter pelo menos 6 caracteres.',
        variant: 'destructive',
      });
      return;
    }

    if (password !== confirmPassword) {
      toast({
        title: 'Erro',
        description: 'As senhas não coincidem.',
        variant: 'destructive',
      });
      return;
    }

    setIsActivating(true);

    try {
      const response = await apiClient.post<any>('/auth/activate', {
        Token: token,
        Name: name.trim(),
        Password: password,
        ConfirmPassword: confirmPassword,
        PreferredWorkoutLocation: parseInt(workoutLocation),
      });

      // Store the auth token
      if (response.token) {
        localStorage.setItem('authToken', response.token);
      }

      toast({
        title: 'Conta ativada com sucesso!',
        description: 'Bem-vindo ao TaktIQ. Redirecionando...',
      });

      // Redirect to dashboard
      setTimeout(() => {
        router.push('/dashboard');
      }, 1500);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message ||
        error.response?.data?.detail ||
        'Não foi possível ativar sua conta. O convite pode ter expirado.';

      toast({
        title: 'Erro na ativação',
        description: errorMessage,
        variant: 'destructive',
      });
      setIsActivating(false);
    }
  };

  if (isValidating) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
          <p className="text-muted-foreground">Validando convite...</p>
        </div>
      </div>
    );
  }

  if (!isValidToken) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4">
        <Card className="w-full max-w-md glass-strong border-destructive/30">
          <CardHeader className="text-center">
            <div className="flex justify-center mb-4">
              <div className="p-4 bg-destructive/20 rounded-full">
                <Mail className="h-12 w-12 text-destructive" />
              </div>
            </div>
            <CardTitle className="text-2xl">Link Inválido</CardTitle>
            <CardDescription className="text-base">
              O link de ativação está incompleto, inválido ou expirado.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground text-center mb-4">
              Entre em contato com seu personal trainer para receber um novo convite.
            </p>
            <Button
              variant="outline"
              className="w-full"
              onClick={() => router.push('/login')}
            >
              Ir para Login
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

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

      {/* Activation card */}
      <Card className="relative w-full max-w-md glass-strong border-primary/30 shadow-2xl hover-lift">
        <CardHeader className="space-y-4 pb-6">
          <div className="flex justify-center relative">
            <div className="absolute inset-0 blur-2xl opacity-20 bg-primary rounded-full" />
            <TaktIQLogo width={200} height={57} className="drop-shadow-lg relative z-10" />
          </div>
          <div className="space-y-2">
            <div className="flex justify-center mb-2">
              <div className="p-3 bg-primary/20 rounded-full">
                <UserPlus className="h-8 w-8 text-primary" />
              </div>
            </div>
            <CardTitle className="text-center text-3xl font-bold bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text">
              Ative sua Conta
            </CardTitle>
            <CardDescription className="text-center text-base">
              Complete seu cadastro e comece a treinar com seu personal trainer
            </CardDescription>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <form onSubmit={handleActivate} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name" className="text-sm font-medium">
                Nome Completo <span className="text-destructive">*</span>
              </Label>
              <Input
                id="name"
                type="text"
                placeholder="Seu nome completo"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="h-12 border-border/50 focus:border-primary/50 transition-colors"
                disabled={isActivating}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="password" className="text-sm font-medium">
                Senha <span className="text-destructive">*</span>
              </Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="h-12 border-border/50 focus:border-primary/50 transition-colors"
                disabled={isActivating}
                required
              />
              <p className="text-xs text-muted-foreground">Mínimo de 6 caracteres</p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword" className="text-sm font-medium">
                Confirmar Senha <span className="text-destructive">*</span>
              </Label>
              <Input
                id="confirmPassword"
                type="password"
                placeholder="••••••••"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="h-12 border-border/50 focus:border-primary/50 transition-colors"
                disabled={isActivating}
                required
              />
            </div>

            <div className="space-y-2">
              <Label className="text-sm font-medium">Onde você prefere treinar?</Label>
              <RadioGroup
                value={workoutLocation}
                onValueChange={setWorkoutLocation}
                disabled={isActivating}
                className="space-y-2"
              >
                <div className="flex items-center space-x-2 p-3 rounded-lg border border-border/50 hover:border-primary/50 transition-colors cursor-pointer">
                  <RadioGroupItem value="0" id="gym" />
                  <Label htmlFor="gym" className="flex-1 cursor-pointer font-normal">
                    <div className="font-medium">Academia</div>
                    <div className="text-xs text-muted-foreground">Treino com equipamentos</div>
                  </Label>
                </div>
                <div className="flex items-center space-x-2 p-3 rounded-lg border border-border/50 hover:border-primary/50 transition-colors cursor-pointer">
                  <RadioGroupItem value="1" id="home" />
                  <Label htmlFor="home" className="flex-1 cursor-pointer font-normal">
                    <div className="font-medium">Casa</div>
                    <div className="text-xs text-muted-foreground">Treino em casa ou ao ar livre</div>
                  </Label>
                </div>
                <div className="flex items-center space-x-2 p-3 rounded-lg border border-border/50 hover:border-primary/50 transition-colors cursor-pointer">
                  <RadioGroupItem value="2" id="both" />
                  <Label htmlFor="both" className="flex-1 cursor-pointer font-normal">
                    <div className="font-medium">Ambos</div>
                    <div className="text-xs text-muted-foreground">Treino flexível</div>
                  </Label>
                </div>
              </RadioGroup>
            </div>

            <Button
              type="submit"
              className="w-full h-12 text-base font-semibold hover-lift tap-scale shadow-lg hover:shadow-xl transition-all"
              disabled={isActivating}
            >
              {isActivating ? (
                <>
                  <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                  Ativando...
                </>
              ) : (
                <>
                  <CheckCircle className="mr-2 h-5 w-5" />
                  Ativar Conta
                </>
              )}
            </Button>
          </form>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t border-border/50" />
            </div>
          </div>

          <div className="text-center">
            <p className="text-sm text-muted-foreground">
              Já tem uma conta?{' '}
              <a href="/login" className="text-primary hover:text-primary/80 font-medium transition-colors">
                Entrar
              </a>
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function ActivatePage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center bg-background">
          <div className="text-center">
            <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando...</p>
          </div>
        </div>
      }
    >
      <ActivateContent />
    </Suspense>
  );
}
