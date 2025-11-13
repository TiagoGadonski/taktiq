'use client';

import { useState, useEffect, useRef } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Mail, Save, Moon, MapPin, Dumbbell, Phone, Calendar, Ruler, Weight as WeightIcon, Upload, Camera, Lock, Loader2, AlertCircle, Eye, Home } from 'lucide-react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

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
  healthConditions: z.string().optional(),
  exerciseGoal: z.string().optional(),
  preferredWorkoutLocation: z.string().optional(),
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
  const { user, logout, refreshUser, isLoading: isAuthLoading } = useAuth();
  const { toast } = useToast();
  const { theme } = useTheme();
  const router = useRouter();
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
    control,
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
          healthConditions: data.healthConditions || '',
          exerciseGoal: data.exerciseGoal || '',
          preferredWorkoutLocation: data.preferredWorkoutLocation?.toString() || '0',
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
        healthConditions: data.healthConditions || null,
        exerciseGoal: data.exerciseGoal || null,
        preferredWorkoutLocation: data.preferredWorkoutLocation ? parseInt(data.preferredWorkoutLocation) : 0,
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
    <div className="space-y-6 pb-8">
      {/* Header with Profile Hero Section */}
      <div className="relative overflow-hidden rounded-lg bg-gradient-to-br from-primary/10 via-primary/5 to-background border">
        <div className="absolute inset-0 bg-gym-pattern opacity-5"></div>
        <div className="relative p-6 md:p-8">
          <div className="flex flex-col md:flex-row items-center md:items-center gap-6">
            {/* Profile Picture with Upload */}
            <div className="relative group">
              <Avatar className="h-28 w-28 md:h-32 md:w-32 border-4 border-background shadow-xl ring-2 ring-primary/20 transition-all group-hover:ring-primary/40">
                <AvatarImage
                  src={
                    profilePicture
                      ? `${env.apiHost}${profilePicture}?t=${Date.now()}`
                      : undefined
                  }
                />
                <AvatarFallback className="text-3xl md:text-4xl font-bold bg-gradient-to-br from-primary/20 to-primary/10">
                  {getInitials(profileData?.name || user?.name || 'User')}
                </AvatarFallback>
              </Avatar>
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
                size="sm"
                className="absolute -bottom-2 -right-2 h-10 w-10 rounded-full p-0 shadow-lg"
              >
                {uploading ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Camera className="h-4 w-4" />
                )}
              </Button>
            </div>

            {/* User Info */}
            <div className="flex-1 space-y-3 text-center md:text-left w-full md:w-auto">
              <div>
                <h1 className="text-2xl sm:text-3xl md:text-4xl font-bold tracking-tight">
                  {profileData?.name || user?.name || 'Carregando...'}
                </h1>
                <p className="text-muted-foreground text-sm md:text-base mt-1 break-all">
                  {profileData?.email || user?.email}
                </p>
              </div>

              {/* Quick Stats */}
              <div className="flex flex-wrap justify-center md:justify-start gap-3 md:gap-6 text-xs sm:text-sm">
                {profileData?.location && (
                  <div className="flex items-center gap-1.5 bg-background/50 px-2 py-1 rounded-md">
                    <MapPin className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-muted-foreground" />
                    <span>{profileData.location}</span>
                  </div>
                )}
                {profileData?.gymName && (
                  <div className="flex items-center gap-1.5 bg-background/50 px-2 py-1 rounded-md">
                    <Dumbbell className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-muted-foreground" />
                    <span>{profileData.gymName}</span>
                  </div>
                )}
                {user?.createdAt && (
                  <div className="flex items-center gap-1.5 bg-background/50 px-2 py-1 rounded-md">
                    <Calendar className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-muted-foreground" />
                    <span className="whitespace-nowrap">Desde {new Date(user.createdAt).toLocaleDateString('pt-BR', { month: 'short', year: 'numeric' })}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="flex flex-col sm:flex-row md:flex-col gap-2 w-full md:w-auto">
              {!isEditing && (
                <Button
                  onClick={() => setIsEditing(true)}
                  className="w-full sm:flex-1 md:w-auto shadow-lg"
                >
                  <User className="mr-2 h-4 w-4" />
                  Editar Perfil
                </Button>
              )}
              {!isAuthLoading && user && (
                <Button
                  variant="outline"
                  onClick={() => router.push(`/users/${user.id}`)}
                  className="w-full sm:flex-1 md:w-auto"
                >
                  <Eye className="mr-2 h-4 w-4" />
                  Ver Perfil Público
                </Button>
              )}
            </div>
          </div>

          {/* Bio Preview */}
          {!isEditing && profileData?.bio && (
            <div className="mt-6 pt-6 border-t">
              <p className="text-sm text-muted-foreground italic max-w-2xl text-center md:text-left mx-auto md:mx-0">
                &ldquo;{profileData.bio}&rdquo;
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Profile Information */}
        <div className="lg:col-span-2 space-y-6">
          <Card className="shadow-md">
            <CardHeader className="pb-4">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                <div className="flex-1">
                  <CardTitle className="text-lg sm:text-xl">Informações Pessoais</CardTitle>
                  <CardDescription className="mt-1 text-xs sm:text-sm">
                    Suas informações básicas de perfil
                  </CardDescription>
                </div>
                {isEditing && (
                  <div className="flex items-center gap-2 text-xs text-muted-foreground bg-primary/10 px-3 py-1.5 rounded-full w-fit">
                    <div className="h-2 w-2 bg-primary rounded-full animate-pulse"></div>
                    Editando
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                {/* Basic Info Section */}
                <div className="space-y-4">
                  <h3 className="text-xs sm:text-sm font-semibold text-muted-foreground uppercase tracking-wide">Informações Básicas</h3>
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
                  </div>
                </div>

                {/* Location & Gym Section */}
                <div className="space-y-4 pt-4 border-t">
                  <h3 className="text-xs sm:text-sm font-semibold text-muted-foreground uppercase tracking-wide">Localização & Academia</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="preferredWorkoutLocation" className="flex items-center gap-2">
                      <Home className="h-4 w-4 text-primary" />
                      Local Preferido de Treino
                    </Label>
                    <Controller
                      name="preferredWorkoutLocation"
                      control={control}
                      render={({ field }) => (
                        <Select
                          value={field.value || '0'}
                          onValueChange={field.onChange}
                          disabled={!isEditing}
                        >
                          <SelectTrigger id="preferredWorkoutLocation" className="glass">
                            <SelectValue placeholder="Selecione o local" />
                          </SelectTrigger>
                          <SelectContent className="glass">
                            <SelectItem value="0">🏋️ Academia</SelectItem>
                            <SelectItem value="1">🏠 Casa</SelectItem>
                            <SelectItem value="2">🔄 Ambos</SelectItem>
                          </SelectContent>
                        </Select>
                      )}
                    />
                    <p className="text-xs text-muted-foreground">
                      Isso ajudará a IA a gerar treinos mais adequados aos equipamentos disponíveis.
                    </p>
                  </div>
                </div>

                {/* Physical Metrics Section */}
                <div className="space-y-4 pt-4 border-t">
                  <h3 className="text-xs sm:text-sm font-semibold text-muted-foreground uppercase tracking-wide">Métricas Físicas</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                </div>

                {/* Bio Section */}
                <div className="space-y-4 pt-4 border-t">
                  <h3 className="text-xs sm:text-sm font-semibold text-muted-foreground uppercase tracking-wide">Sobre Você</h3>
                  <div className="space-y-2">
                    <Label htmlFor="bio">Biografia</Label>
                    <Textarea
                      id="bio"
                      {...register('bio')}
                      disabled={!isEditing}
                      placeholder="Conte um pouco sobre você..."
                      rows={4}
                      className="resize-none"
                    />
                  </div>
                </div>

                {/* Health & Fitness Section */}
                <div className="space-y-4 pt-4 border-t">
                  <h3 className="text-xs sm:text-sm font-semibold text-muted-foreground uppercase tracking-wide">Saúde & Fitness</h3>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label htmlFor="exerciseGoal" className="flex items-center gap-2">
                        <Dumbbell className="h-4 w-4 text-primary" />
                        Objetivo de Treino
                      </Label>
                      <Textarea
                        id="exerciseGoal"
                        {...register('exerciseGoal')}
                        disabled={!isEditing}
                        placeholder="Ex: perder peso, ganhar massa muscular, melhorar condicionamento físico"
                        rows={2}
                        className="resize-none"
                      />
                      <p className="text-xs text-muted-foreground">
                        Descreva seu objetivo principal com os exercícios para treinos mais direcionados.
                      </p>
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
                        rows={2}
                        className="resize-none"
                      />
                      <p className="text-xs text-muted-foreground">
                        Informe lesões ou limitações físicas para treinos mais seguros e personalizados.
                      </p>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="healthConditions" className="flex items-center gap-2">
                        <AlertCircle className="h-4 w-4 text-red-500" />
                        Condições de Saúde
                      </Label>
                      <Textarea
                        id="healthConditions"
                        {...register('healthConditions')}
                        disabled={!isEditing}
                        placeholder="Ex: diabetes, hipertensão, asma, problemas cardíacos"
                        rows={2}
                        className="resize-none"
                      />
                      <p className="text-xs text-muted-foreground">
                        Informe condições de saúde relevantes para treinos mais seguros.
                      </p>
                    </div>
                  </div>
                </div>

                {/* Action Buttons */}
                {isEditing && (
                  <div className="flex flex-col sm:flex-row gap-3 pt-4 border-t">
                    <Button type="submit" className="flex-1 shadow-md">
                      <Save className="mr-2 h-4 w-4" />
                      Salvar Alterações
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => setIsEditing(false)}
                      className="flex-1"
                    >
                      Cancelar
                    </Button>
                  </div>
                )}
              </form>
            </CardContent>
          </Card>
        </div>

        {/* Right Column - Preferences & Actions */}
        <div className="space-y-6">
          {/* Preferences Card */}
          <Card className="shadow-md">
            <CardHeader>
              <CardTitle className="text-base sm:text-lg">Preferências</CardTitle>
              <CardDescription className="text-xs sm:text-sm">Configure suas preferências</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <Moon className="h-4 w-4 text-muted-foreground" />
                  <Label className="text-sm font-medium">Tema</Label>
                </div>
                <ThemeSwitcher />
                <p className="text-xs text-muted-foreground">
                  {mounted ? (
                    theme === 'system'
                      ? 'Usando tema do sistema'
                      : theme === 'dark'
                      ? 'Tema escuro ativado'
                      : 'Tema claro ativado'
                  ) : 'Carregando...'}
                </p>
              </div>

              <div className="pt-3 border-t space-y-2">
                <div className="flex items-center justify-between">
                  <div>
                    <Label className="text-sm font-medium">Idioma</Label>
                    <p className="text-xs text-muted-foreground">Interface</p>
                  </div>
                  <Button variant="outline" size="sm" className="text-xs">
                    PT-BR
                  </Button>
                </div>
              </div>

              <div className="pt-3 border-t space-y-2">
                <div className="flex items-center justify-between">
                  <div>
                    <Label className="text-sm font-medium">Unidades</Label>
                    <p className="text-xs text-muted-foreground">Sistema de medidas</p>
                  </div>
                  <Button variant="outline" size="sm" className="text-xs">
                    Métrico
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Account Actions Card */}
          <Card className="shadow-md">
            <CardHeader>
              <CardTitle className="text-base sm:text-lg">Segurança</CardTitle>
              <CardDescription className="text-xs sm:text-sm">Gerencie sua conta</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button
                variant="outline"
                className="w-full justify-start"
                onClick={() => setIsChangePasswordDialogOpen(true)}
              >
                <Lock className="mr-2 h-4 w-4" />
                Alterar Senha
              </Button>
              <Button
                variant="outline"
                onClick={() => logout()}
                className="w-full justify-start"
              >
                Sair da Conta
              </Button>
              <div className="pt-3 border-t">
                <Button
                  variant="ghost"
                  className="w-full justify-start text-destructive hover:text-destructive hover:bg-destructive/10"
                >
                  Excluir Conta
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Stats Card */}
          <Card className="shadow-md bg-gradient-to-br from-primary/5 to-background">
            <CardHeader>
              <CardTitle className="text-base sm:text-lg flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Estatísticas
              </CardTitle>
              <CardDescription className="text-xs sm:text-sm">Resumo da sua atividade</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between p-3 rounded-lg bg-background/50 border">
                  <div className="space-y-1">
                    <p className="text-xs text-muted-foreground">Membro desde</p>
                    <p className="text-sm font-semibold">
                      {user?.createdAt
                        ? new Date(user.createdAt).toLocaleDateString('pt-BR', {
                            day: 'numeric',
                            month: 'long',
                            year: 'numeric'
                          })
                        : 'N/A'}
                    </p>
                  </div>
                </div>

                <div className="flex items-center justify-between p-3 rounded-lg bg-background/50 border">
                  <div className="space-y-1">
                    <p className="text-xs text-muted-foreground">Último acesso</p>
                    <p className="text-sm font-semibold">Hoje</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Change Password Dialog */}
      <Dialog open={isChangePasswordDialogOpen} onOpenChange={setIsChangePasswordDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-xl">
              <div className="p-2 rounded-lg bg-primary/10">
                <Lock className="h-5 w-5 text-primary" />
              </div>
              Alterar Senha
            </DialogTitle>
            <DialogDescription className="text-base">
              Digite sua senha atual e escolha uma nova senha segura
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmitPassword(onChangePassword)} className="space-y-5 mt-2">
            <div className="space-y-2">
              <Label htmlFor="currentPassword" className="text-sm font-medium">
                Senha Atual
              </Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="currentPassword"
                  type="password"
                  {...registerPassword('currentPassword')}
                  placeholder="Digite sua senha atual"
                  className="pl-9"
                />
              </div>
              {passwordErrors.currentPassword && (
                <p className="text-sm text-destructive flex items-center gap-1">
                  <AlertCircle className="h-3 w-3" />
                  {passwordErrors.currentPassword.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="newPassword" className="text-sm font-medium">
                Nova Senha
              </Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="newPassword"
                  type="password"
                  {...registerPassword('newPassword')}
                  placeholder="Mínimo 6 caracteres"
                  className="pl-9"
                />
              </div>
              {passwordErrors.newPassword && (
                <p className="text-sm text-destructive flex items-center gap-1">
                  <AlertCircle className="h-3 w-3" />
                  {passwordErrors.newPassword.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword" className="text-sm font-medium">
                Confirmar Nova Senha
              </Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="confirmPassword"
                  type="password"
                  {...registerPassword('confirmPassword')}
                  placeholder="Digite a senha novamente"
                  className="pl-9"
                />
              </div>
              {passwordErrors.confirmPassword && (
                <p className="text-sm text-destructive flex items-center gap-1">
                  <AlertCircle className="h-3 w-3" />
                  {passwordErrors.confirmPassword.message}
                </p>
              )}
            </div>

            <div className="flex gap-3 pt-2">
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
              <Button
                type="submit"
                className="flex-1 shadow-md"
                disabled={isChangingPassword}
              >
                {isChangingPassword ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Alterando...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    Confirmar
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
