'use client';

import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Shield, Mail } from 'lucide-react';

/**
 * Signup page - DISABLED for public registration
 *
 * Only administrators can create new user accounts through the admin panel.
 * This ensures controlled access and better security for the application.
 */
export default function SignupPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-500 to-purple-600 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="flex justify-center mb-4">
            <Shield className="h-16 w-16 text-primary" />
          </div>
          <CardTitle className="text-3xl font-bold">Cadastro Desabilitado</CardTitle>
          <CardDescription>
            O registro público de contas está desabilitado
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="bg-muted/50 p-4 rounded-lg space-y-2">
            <h3 className="font-semibold text-sm flex items-center gap-2">
              <Mail className="h-4 w-4 text-primary" />
              Como obter acesso?
            </h3>
            <p className="text-sm text-muted-foreground">
              Para criar uma conta no TaktIQ, entre em contato com um administrador do sistema.
              Somente administradores podem criar novas contas de usuário.
            </p>
          </div>

          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">
              Já tem uma conta?
            </p>
            <Link href="/login" className="w-full block">
              <Button className="w-full" variant="default">
                Fazer Login
              </Button>
            </Link>
          </div>

          <div className="pt-4 border-t">
            <p className="text-xs text-center text-muted-foreground">
              Este é um sistema de acesso controlado para garantir a segurança e qualidade do serviço.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
