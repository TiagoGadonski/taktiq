'use client';

import { useState, useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Mail, Save, Moon, MapPin, Dumbbell, Phone, Calendar, Ruler, Weight as WeightIcon, Upload, Camera, Lock, Loader2, AlertCircle } from 'lucide-react';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { env } from '@/lib/env';
import { ThemeSwitcher } from '@/components/theme-switcher';
import { useTheme } from 'next-themes';

const profileSchema = z.object({
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres'),
  email: z.string().email('Email inválido'),
  dateOfBirth: z.string().optional(),
  location: z.string().optional(),
  bio: z.string().optional(),
  height: z.string().optional(),
  weight: z.string().optional(),
  gymName: z.string().optional(),
  phoneNumber: z.string().optional(),
  injuries: z.string().optional(),
});

const changePasswordSchema = z.object({
  currentPassword: z.string().min(1, 'Senha atual é obrigatória'),
  newPassword: z.string().min(6, 'A nova senha deve ter pelo menos 6 caracteres'),
  confirmPassword: z.string().min(1, 'Confirmação de senha é obrigatória'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'As senhas não coincidem',
  path: ['confirmPassword'],
});

type ProfileInput = z.infer<typeof profileSchema>;
type ChangePasswordInput = z.infer<typeof changePasswordSchema>;

export default function ProfilePage() {
  const { user, logout, refreshUser } = useAuth();
  const { toast } = useToast();
  const { theme } = useTheme();
  const [isEditing, setIsEditing] = useState(false);
  const [profileData, setProfileData] = useState<any>(null);
  const [profilePicture, setProfilePicture] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [mounted, setMounted] = useState(false);
  const [isChangePasswordDialogOpen, setIsChangePasswordDialogOpen] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    setMounted(true);
  }, []);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProfileInput>({
    resolver: zodResolver(profileSchema),
  });

  const {
    register: registerPassword,
    handleSubmit: handleSubmitPassword,
    reset: resetPassword,
    formState: { errors: passwordErrors },
  } = useForm<ChangePasswordInput>({
    resolver: zodResolver(changePasswordSchema),
  });

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await apiClient.get<any>('/me');

        // The apiClient already unwraps the data
        const data = response.data || response;

        if (!data) {
          throw new Error('No data received from server');
        }

        setProfileData(data);
        setProfilePicture(data.profilePictureUrl || null);

        // Format date for input
        const formattedDate = data.dateOfBirth
          ? new Date(data.dateOfBirth).toISOString().split('T')[0]
          : '';

        reset({
          name: data.name || '',
          email: data.email || '',
          dateOfBirth: formattedDate,
          location: data.location || '',
          bio: data.bio || '',
          height: data.height?.toString() || '',
          weight: data.weight?.toString() || '',
          gymName: data.gymName || '',
          phoneNumber: data.phoneNumber || '',
          injuries: data.injuries || '',
        });
      } catch (error: any) {
        toast({
          variant: 'destructive',
          title: 'Erro ao carregar perfil',
          description: error.response?.data?.message || error.message || 'Não foi possível carregar suas informações',
        });
      }
    };

    fetchProfile();
  }, [reset, toast]);

  const onSubmit = async (data: ProfileInput) => {
    try {
      await apiClient.put('/me', {
        name: data.name,
        email: data.email,
        dateOfBirth: data.dateOfBirth ? new Date(data.dateOfBirth).toISOString() : null,
        location: data.location || null,
        bio: data.bio || null,
        height: data.height ? parseFloat(data.height) : null,
        weight: data.weight ? parseFloat(data.weight) : null,
        gymName: data.gymName || null,
        phoneNumber: data.phoneNumber || null,
        injuries: data.injuries || null,
      });

      toast({
        title: 'Perfil atualizado!',
        description: 'Suas informações foram salvas com sucesso.',
      });
      setIsEditing(false);

      // Refresh user data in the auth context
      await refreshUser();
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao atualizar perfil',
        description: error.response?.data?.message || 'Não foi possível atualizar o perfil',
      });
    }
  };

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!['image/jpeg', 'image/jpg', 'image/png', 'image/gif'].includes(file.type)) {
      toast({
        variant: 'destructive',
        title: 'Formato inválido',
        description: 'Por favor, selecione uma imagem (JPG, PNG ou GIF)',
      });
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast({
        variant: 'destructive',
        title: 'Arquivo muito grande',
        description: 'A imagem deve ter no máximo 5MB',
      });
      return;
    }

    setUploading(true);

    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await apiClient.post<any>('/me/profile-picture', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      // Extract profilePictureUrl from response
      const responseData = response.data || response;
      const newProfilePictureUrl = responseData.profilePictureUrl;

      // Update state with cache-busting parameter to force image reload
      setProfilePicture(newProfilePictureUrl);

      // Force image reload by updating the src
      const avatarImage = document.querySelector('img[src*="uploads/profiles"]') as HTMLImageElement;
      if (avatarImage && newProfilePictureUrl) {
        avatarImage.src = `${env.apiHost}${newProfilePictureUrl}?t=${Date.now()}`;
      }

      toast({
        title: 'Foto atualizada!',
        description: 'Sua foto de perfil foi alterada com sucesso.',
      });

      // Refresh user data in the auth context
      await refreshUser();
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao enviar foto',
        description: error.response?.data?.message || 'Não foi possível atualizar a foto',
      });
    } finally {
      setUploading(false);
    }
  };

  const getInitials = (name: string) => {
    return name
      ?.split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2) || 'U';
  };

  const onChangePassword = async (data: ChangePasswordInput) => {
    setIsChangingPassword(true);
    try {
      await apiClient.post('/auth/change-password', {
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      });

      toast({
        title: 'Senha alterada!',
        description: 'Sua senha foi alterada com sucesso.',
      });

      setIsChangePasswordDialogOpen(false);
      resetPassword();
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao alterar senha',
        description: error.response?.data?.message || error.message || 'Não foi possível alterar a senha',
      });
    } finally {
      setIsChangingPassword(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Perfil</h1>
        <p className="text-muted-foreground">Gerencie suas informações e preferências</p>
      </div>

      {/* Profile Picture */}
      <Card>
        <CardHeader>
          <CardTitle>Foto de Perfil</CardTitle>
          <CardDescription>Adicione ou atualize sua foto de perfil</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage
                src={
                  profilePicture
                    ? `${env.apiHost}${profilePicture}?t=${Date.now()}`
                    : undefined
                }
              />
              <AvatarFallback className="text-2xl">
                {getInitials(profileData?.name || user?.name || 'User')}
              </AvatarFallback>
            </Avatar>
            <div className="space-y-2">
              <input
                ref={fileInputRef}
                type="file"
                accept="image/jpeg,image/jpg,image/png,image/gif"
                onChange={handleFileChange}
                className="hidden"
              />
              <Button
                onClick={() => fileInputRef.current?.click()}
                disabled={uploading}
                variant="outline"
              >
                <Camera className="mr-2 h-4 w-4" />
                {uploading ? 'Enviando...' : 'Alterar Foto'}
              </Button>
              <p className="text-xs text-muted-foreground">
                JPG, PNG ou GIF. Máximo 5MB.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Profile Information */}
      <Card>
        <CardHeader>
          <CardTitle>Informações Pessoais</CardTitle>
          <CardDescription>
            Suas informações básicas de perfil
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="name">Nome *</Label>
                <div className="relative">
                  <User className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="name"
                    {...register('name')}
                    disabled={!isEditing}
                    className="pl-9"
                  />
                </div>
                {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">Email *</Label>
                <div className="relative">
                  <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="email"
                    type="email"
                    {...register('email')}
                    disabled={!isEditing}
                    className="pl-9"
                  />
                </div>
                {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
              </div>

              <div className="space-y-2">
                <Label htmlFor="phoneNumber">Telefone</Label>
                <div className="relative">
                  <Phone className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="phoneNumber"
                    {...register('phoneNumber')}
                    disabled={!isEditing}
                    className="pl-9"
                    placeholder="(11) 99999-9999"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="dateOfBirth">Data de Nascimento</Label>
                <div className="relative">
                  <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="dateOfBirth"
                    type="date"
                    {...register('dateOfBirth')}
                    disabled={!isEditing}
                    className="pl-9"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="location">Localização</Label>
                <div className="relative">
                  <MapPin className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="location"
                    {...register('location')}
                    disabled={!isEditing}
                    className="pl-9"
                    placeholder="Cidade, Estado"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="gymName">Academia</Label>
                <div className="relative">
                  <Dumbbell className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="gymName"
                    {...register('gymName')}
                    disabled={!isEditing}
                    className="pl-9"
                    placeholder="Nome da sua academia"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="height">Altura (cm)</Label>
                <div className="relative">
                  <Ruler className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="height"
                    type="number"
                    step="0.1"
                    {...register('height')}
                    disabled={!isEditing}
                    className="pl-9"
                    placeholder="170"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="weight">Peso (kg)</Label>
                <div className="relative">
                  <WeightIcon className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="weight"
                    type="number"
                    step="0.1"
                    {...register('weight')}
                    disabled={!isEditing}
                    className="pl-9"
                    placeholder="70"
                  />
                </div>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="bio">Biografia</Label>
              <Textarea
                id="bio"
                {...register('bio')}
                disabled={!isEditing}
                placeholder="Conte um pouco sobre você..."
                rows={4}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="injuries" className="flex items-center gap-2">
                <AlertCircle className="h-4 w-4 text-orange-500" />
                Lesões/Limitações
              </Label>
              <Textarea
                id="injuries"
                {...register('injuries')}
                disabled={!isEditing}
                placeholder="Ex: ombro, joelho, lombar (separe por vírgula)"
                rows={3}
              />
              <p className="text-xs text-muted-foreground">
                Informe lesões ou limitações físicas para treinos mais seguros e personalizados.
                Exemplos: ombro, rotator cuff, impingement, lombar, joelho, etc.
              </p>
            </div>

            {isEditing && (
              <div className="flex gap-2">
                <Button type="submit">
                  <Save className="mr-2 h-4 w-4" />
                  Salvar Alterações
                </Button>
                <Button type="button" variant="outline" onClick={() => setIsEditing(false)}>
                  Cancelar
                </Button>
              </div>
            )}
          </form>

          {!isEditing && (
            <div className="flex gap-2 mt-4">
              <Button type="button" onClick={(e) => {
                e.preventDefault();
                setIsEditing(true);
              }}>
                Editar Perfil
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Preferences */}
      <Card>
        <CardHeader>
          <CardTitle>Preferências</CardTitle>
          <CardDescription>Configure suas preferências de uso</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <div className="flex items-center gap-2">
                <Moon className="h-4 w-4" />
                <Label>Tema</Label>
              </div>
              <p className="text-sm text-muted-foreground">
                {mounted ? (
                  theme === 'system'
                    ? 'Usando tema do sistema'
                    : theme === 'dark'
                    ? 'Tema escuro ativado'
                    : 'Tema claro ativado'
                ) : 'Carregando...'}
              </p>
            </div>
            <ThemeSwitcher />
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Idioma</Label>
              <p className="text-sm text-muted-foreground">Idioma da interface</p>
            </div>
            <Button variant="outline" size="sm">
              Português (BR)
            </Button>
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Unidades</Label>
              <p className="text-sm text-muted-foreground">Sistema de medidas preferido</p>
            </div>
            <Button variant="outline" size="sm">
              Métrico (kg, km)
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Account Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Ações da Conta</CardTitle>
          <CardDescription>Gerencie sua conta</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Button
            variant="outline"
            className="w-full justify-start"
            onClick={() => setIsChangePasswordDialogOpen(true)}
          >
            <Lock className="mr-2 h-4 w-4" />
            Alterar Senha
          </Button>
          <Button variant="outline" className="w-full justify-start text-destructive">
            Excluir Conta
          </Button>
          <div className="pt-4">
            <Button variant="outline" onClick={() => logout()} className="w-full">
              Sair da Conta
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stats Summary */}
      <Card>
        <CardHeader>
          <CardTitle>Estatísticas da Conta</CardTitle>
          <CardDescription>Resumo da sua atividade</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Membro desde:</span>
            <span className="font-medium">
              {user?.createdAt
                ? new Date(user.createdAt).toLocaleDateString('pt-BR')
                : 'N/A'}
            </span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Último acesso:</span>
            <span className="font-medium">Hoje</span>
          </div>
        </CardContent>
      </Card>

      {/* Change Password Dialog */}
      <Dialog open={isChangePasswordDialogOpen} onOpenChange={setIsChangePasswordDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Lock className="h-5 w-5" />
              Alterar Senha
            </DialogTitle>
            <DialogDescription>
              Digite sua senha atual e escolha uma nova senha
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmitPassword(onChangePassword)} className="space-y-4 mt-4">
            <div className="space-y-2">
              <Label htmlFor="currentPassword">Senha Atual</Label>
              <Input
                id="currentPassword"
                type="password"
                {...registerPassword('currentPassword')}
                placeholder="Digite sua senha atual"
              />
              {passwordErrors.currentPassword && (
                <p className="text-sm text-destructive">{passwordErrors.currentPassword.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="newPassword">Nova Senha</Label>
              <Input
                id="newPassword"
                type="password"
                {...registerPassword('newPassword')}
                placeholder="Digite sua nova senha (mínimo 6 caracteres)"
              />
              {passwordErrors.newPassword && (
                <p className="text-sm text-destructive">{passwordErrors.newPassword.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirmar Nova Senha</Label>
              <Input
                id="confirmPassword"
                type="password"
                {...registerPassword('confirmPassword')}
                placeholder="Digite sua nova senha novamente"
              />
              {passwordErrors.confirmPassword && (
                <p className="text-sm text-destructive">{passwordErrors.confirmPassword.message}</p>
              )}
            </div>

            <div className="flex gap-2 pt-4">
              <Button
                type="button"
                variant="outline"
                className="flex-1"
                onClick={() => {
                  setIsChangePasswordDialogOpen(false);
                  resetPassword();
                }}
                disabled={isChangingPassword}
              >
                Cancelar
              </Button>
              <Button type="submit" className="flex-1" disabled={isChangingPassword}>
                {isChangingPassword ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Alterando...
                  </>
                ) : (
                  <>
                    <Lock className="mr-2 h-4 w-4" />
                    Alterar Senha
                  </>
                )}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
