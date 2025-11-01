'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { useToast } from '@/components/ui/use-toast';
import { ArrowLeft } from 'lucide-react';

interface ForgotPasswordForm {
  email: string;
}

export default function ForgotPasswordPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordForm>();

  const onSubmit = async (data: ForgotPasswordForm) => {
    setIsLoading(true);
    try {
      await api.auth.forgotPassword({ email: data.email });

      toast({
        title: 'Email enviado!',
        description: 'Se o email existir, você receberá um código de recuperação.',
      });

      // Redirect to reset password page
      router.push(`/reset-password?email=${encodeURIComponent(data.email)}`);
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro',
        description: error.message || 'Ocorreu um erro. Tente novamente.',
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="relative flex min-h-screen items-center justify-center overflow-hidden p-4 bg-background">
      {/* Modern gradient background */}
      <div className="absolute inset-0 bg-gradient-to-br from-background via-background to-primary/5" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-primary/20 via-transparent to-transparent" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,_var(--tw-gradient-stops))] from-primary/10 via-transparent to-transparent" />

      {/* Gym pattern overlay */}
      <div className="absolute inset-0 opacity-5 gym-pattern" />

      {/* Forgot Password card */}
      <Card className="relative w-full max-w-md glass-card border-primary/20 shadow-2xl">
        <CardHeader className="space-y-3 pb-6">
          <div className="flex justify-center">
            <TaktIQLogo width={200} height={57} className="drop-shadow-lg" />
          </div>
          <CardTitle className="text-center text-2xl font-bold">Esqueceu a senha?</CardTitle>
          <CardDescription className="text-center">
            Digite seu email e enviaremos um código de recuperação
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="seu@email.com"
                {...register('email', {
                  required: 'O email é obrigatório',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Email inválido',
                  },
                })}
                aria-invalid={errors.email ? 'true' : 'false'}
                className="h-11"
              />
              {errors.email && (
                <p className="text-sm text-destructive">{errors.email.message}</p>
              )}
            </div>

            <Button
              type="submit"
              className="w-full h-11 font-semibold hover-lift tap-scale"
              disabled={isLoading}
            >
              {isLoading ? 'Enviando...' : 'Enviar código'}
            </Button>
          </form>

          <div className="text-center text-sm">
            <Link
              href="/login"
              className="text-primary hover:underline font-medium inline-flex items-center gap-1"
            >
              <ArrowLeft className="h-3.5 w-3.5" />
              Voltar para login
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
