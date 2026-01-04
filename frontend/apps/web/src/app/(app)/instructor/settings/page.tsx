'use client';

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Switch } from '@/components/ui/switch';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { Alert, AlertDescription } from '@/components/ui/alert';
import {
  User,
  Mail,
  Phone,
  MapPin,
  FileText,
  Bell,
  Lock,
  CreditCard,
  CheckCircle2,
  AlertCircle,
  Upload,
} from 'lucide-react';
import { apiClient } from '@/lib/api';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/hooks/use-auth';

export default function SettingsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const { user, refreshUser } = useAuth();
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState(searchParams?.get('tab') || 'profile');
  const [isUploadingPhoto, setIsUploadingPhoto] = useState(false);

  // Profile form state
  const [profileData, setProfileData] = useState({
    name: user?.name || '',
    email: user?.email || '',
    phoneNumber: user?.phoneNumber || '',
    bio: user?.bio || '',
    location: user?.location || '',
    specialization: user?.specialization || '',
  });

  // Notification preferences
  const [notifications, setNotifications] = useState({
    emailWorkoutCompleted: true,
    emailNewClient: true,
    emailPaymentReceived: true,
    pushWorkoutCompleted: false,
    pushNewClient: true,
    pushPaymentReceived: true,
  });

  // Privacy settings
  const [privacy, setPrivacy] = useState({
    profilePublic: true,
    showEmail: false,
    showPhone: false,
    allowMarketplaceListings: true,
  });

  // Update profile when user changes
  useEffect(() => {
    if (user) {
      setProfileData({
        name: user.name || '',
        email: user.email || '',
        phoneNumber: user.phoneNumber || '',
        bio: user.bio || '',
        location: user.location || '',
        specialization: user.specialization || '',
      });
    }
  }, [user]);

  // Save profile mutation
  const saveProfileMutation = useMutation({
    mutationFn: async (data: typeof profileData) => {
      await apiClient.put('/users/profile', data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user'] });
      refreshUser?.();
      toast({
        title: 'Perfil atualizado',
        description: 'Suas informações foram salvas com sucesso.',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao salvar',
        description: error.response?.data?.message || 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Save notifications mutation
  const saveNotificationsMutation = useMutation({
    mutationFn: async (data: typeof notifications) => {
      await apiClient.put('/users/notification-preferences', data);
    },
    onSuccess: () => {
      toast({
        title: 'Preferências salvas',
        description: 'Suas preferências de notificação foram atualizadas.',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao salvar',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Save privacy mutation
  const savePrivacyMutation = useMutation({
    mutationFn: async (data: typeof privacy) => {
      await apiClient.put('/users/privacy-settings', data);
    },
    onSuccess: () => {
      toast({
        title: 'Privacidade atualizada',
        description: 'Suas configurações de privacidade foram salvas.',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao salvar',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  const handleSaveProfile = () => {
    saveProfileMutation.mutate(profileData);
  };

  const handleSaveNotifications = () => {
    saveNotificationsMutation.mutate(notifications);
  };

  const handleSavePrivacy = () => {
    savePrivacyMutation.mutate(privacy);
  };

  const handleUploadPhoto = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: 'Arquivo muito grande',
        description: 'O arquivo deve ter no máximo 5MB.',
        variant: 'destructive',
      });
      return;
    }

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
      toast({
        title: 'Tipo de arquivo inválido',
        description: 'Apenas arquivos JPG, PNG ou GIF são permitidos.',
        variant: 'destructive',
      });
      return;
    }

    setIsUploadingPhoto(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch('/api/users/profile-picture', {
        method: 'POST',
        body: formData,
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to upload photo');
      }

      const data = await response.json();

      toast({
        title: 'Foto atualizada',
        description: 'Sua foto de perfil foi atualizada com sucesso.',
      });

      // Refresh user data
      refreshUser?.();
      queryClient.invalidateQueries({ queryKey: ['user'] });
    } catch (error: any) {
      toast({
        title: 'Erro ao fazer upload',
        description: 'Não foi possível atualizar a foto de perfil.',
        variant: 'destructive',
      });
    } finally {
      setIsUploadingPhoto(false);
    }
  };

  const getInitials = (name: string) => {
    return name
      ?.split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <div className="container mx-auto p-6 max-w-5xl space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Configurações</h1>
        <p className="text-muted-foreground mt-1">
          Gerencie seu perfil público e configurações da conta
        </p>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full max-w-2xl grid-cols-2 md:grid-cols-4">
          <TabsTrigger value="profile">
            <User className="h-4 w-4 mr-2" />
            Perfil
          </TabsTrigger>
          <TabsTrigger value="personal">
            <FileText className="h-4 w-4 mr-2" />
            Dados
          </TabsTrigger>
          <TabsTrigger value="notifications">
            <Bell className="h-4 w-4 mr-2" />
            Notificações
          </TabsTrigger>
          <TabsTrigger value="privacy">
            <Lock className="h-4 w-4 mr-2" />
            Privacidade
          </TabsTrigger>
        </TabsList>

        {/* Public Profile Tab */}
        <TabsContent value="profile" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Perfil Público</CardTitle>
              <CardDescription>
                Estas informações serão exibidas no seu perfil público e marketplace
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Profile Picture */}
              <div className="flex items-center gap-6">
                <Avatar className="h-24 w-24">
                  <AvatarImage src={user?.profilePictureUrl} alt={user?.name} />
                  <AvatarFallback className="text-2xl">
                    {getInitials(user?.name || 'PT')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <input
                    type="file"
                    id="profile-picture-upload"
                    className="hidden"
                    accept="image/jpeg,image/jpg,image/png,image/gif"
                    onChange={handleUploadPhoto}
                    disabled={isUploadingPhoto}
                  />
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => document.getElementById('profile-picture-upload')?.click()}
                    disabled={isUploadingPhoto}
                  >
                    <Upload className="h-4 w-4 mr-2" />
                    {isUploadingPhoto ? 'Enviando...' : 'Alterar Foto'}
                  </Button>
                  <p className="text-xs text-muted-foreground mt-2">
                    JPG, PNG ou GIF. Máximo 5MB.
                  </p>
                </div>
              </div>

              <Separator />

              {/* Name */}
              <div className="grid gap-2">
                <Label htmlFor="name">Nome Completo</Label>
                <Input
                  id="name"
                  value={profileData.name}
                  onChange={(e) =>
                    setProfileData({ ...profileData, name: e.target.value })
                  }
                  placeholder="Seu nome completo"
                />
              </div>

              {/* Bio */}
              <div className="grid gap-2">
                <Label htmlFor="bio">Biografia</Label>
                <Textarea
                  id="bio"
                  value={profileData.bio}
                  onChange={(e) =>
                    setProfileData({ ...profileData, bio: e.target.value })
                  }
                  placeholder="Conte um pouco sobre você e sua experiência..."
                  rows={4}
                />
                <p className="text-xs text-muted-foreground">
                  Será exibida no seu perfil público
                </p>
              </div>

              {/* Specialization */}
              <div className="grid gap-2">
                <Label htmlFor="specialization">Especialidade</Label>
                <Input
                  id="specialization"
                  value={profileData.specialization}
                  onChange={(e) =>
                    setProfileData({ ...profileData, specialization: e.target.value })
                  }
                  placeholder="Ex: Hipertrofia, Emagrecimento, Performance"
                />
              </div>

              {/* Location */}
              <div className="grid gap-2">
                <Label htmlFor="location">Localização</Label>
                <div className="relative">
                  <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="location"
                    value={profileData.location}
                    onChange={(e) =>
                      setProfileData({ ...profileData, location: e.target.value })
                    }
                    placeholder="Cidade, Estado"
                    className="pl-9"
                  />
                </div>
              </div>

              <Button
                onClick={handleSaveProfile}
                disabled={saveProfileMutation.isPending}
                className="w-full md:w-auto"
              >
                {saveProfileMutation.isPending ? 'Salvando...' : 'Salvar Alterações'}
              </Button>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Personal Data Tab */}
        <TabsContent value="personal" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Dados Pessoais</CardTitle>
              <CardDescription>
                Informações de contato e dados privados
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Email */}
              <div className="grid gap-2">
                <Label htmlFor="email">Email</Label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="email"
                    type="email"
                    value={profileData.email}
                    onChange={(e) =>
                      setProfileData({ ...profileData, email: e.target.value })
                    }
                    className="pl-9"
                    disabled
                  />
                </div>
                <p className="text-xs text-muted-foreground">
                  Email não pode ser alterado. Entre em contato com o suporte se necessário.
                </p>
              </div>

              {/* Phone */}
              <div className="grid gap-2">
                <Label htmlFor="phoneNumber">Telefone</Label>
                <div className="relative">
                  <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    id="phoneNumber"
                    type="tel"
                    value={profileData.phoneNumber}
                    onChange={(e) =>
                      setProfileData({ ...profileData, phoneNumber: e.target.value })
                    }
                    placeholder="(00) 00000-0000"
                    className="pl-9"
                  />
                </div>
              </div>

              <Separator />

              {/* Change Password */}
              <div className="space-y-4">
                <h3 className="text-sm font-semibold">Segurança</h3>
                <Button variant="outline" onClick={() => router.push('/change-password')}>
                  <Lock className="h-4 w-4 mr-2" />
                  Alterar Senha
                </Button>
              </div>

              <Button
                onClick={handleSaveProfile}
                disabled={saveProfileMutation.isPending}
                className="w-full md:w-auto"
              >
                {saveProfileMutation.isPending ? 'Salvando...' : 'Salvar Alterações'}
              </Button>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Notifications Tab */}
        <TabsContent value="notifications" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Preferências de Notificação</CardTitle>
              <CardDescription>
                Configure como deseja receber atualizações
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Email Notifications */}
              <div>
                <h3 className="text-sm font-semibold mb-4">Notificações por Email</h3>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Treinos Completados</div>
                      <div className="text-sm text-muted-foreground">
                        Receba email quando um aluno completar um treino
                      </div>
                    </div>
                    <Switch
                      checked={notifications.emailWorkoutCompleted}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, emailWorkoutCompleted: checked })
                      }
                    />
                  </div>

                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Novos Clientes</div>
                      <div className="text-sm text-muted-foreground">
                        Receba email quando um novo aluno aceitar seu convite
                      </div>
                    </div>
                    <Switch
                      checked={notifications.emailNewClient}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, emailNewClient: checked })
                      }
                    />
                  </div>

                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Pagamentos Recebidos</div>
                      <div className="text-sm text-muted-foreground">
                        Receba email quando receber um pagamento
                      </div>
                    </div>
                    <Switch
                      checked={notifications.emailPaymentReceived}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, emailPaymentReceived: checked })
                      }
                    />
                  </div>
                </div>
              </div>

              <Separator />

              {/* Push Notifications */}
              <div>
                <h3 className="text-sm font-semibold mb-4">Notificações Push</h3>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Treinos Completados</div>
                      <div className="text-sm text-muted-foreground">
                        Notificações em tempo real de treinos
                      </div>
                    </div>
                    <Switch
                      checked={notifications.pushWorkoutCompleted}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, pushWorkoutCompleted: checked })
                      }
                    />
                  </div>

                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Novos Clientes</div>
                      <div className="text-sm text-muted-foreground">
                        Notificações de novos alunos
                      </div>
                    </div>
                    <Switch
                      checked={notifications.pushNewClient}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, pushNewClient: checked })
                      }
                    />
                  </div>

                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">Pagamentos Recebidos</div>
                      <div className="text-sm text-muted-foreground">
                        Notificações de pagamentos
                      </div>
                    </div>
                    <Switch
                      checked={notifications.pushPaymentReceived}
                      onCheckedChange={(checked) =>
                        setNotifications({ ...notifications, pushPaymentReceived: checked })
                      }
                    />
                  </div>
                </div>
              </div>

              <Button
                onClick={handleSaveNotifications}
                disabled={saveNotificationsMutation.isPending}
                className="w-full md:w-auto"
              >
                {saveNotificationsMutation.isPending ? 'Salvando...' : 'Salvar Preferências'}
              </Button>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Privacy Tab */}
        <TabsContent value="privacy" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Configurações de Privacidade</CardTitle>
              <CardDescription>
                Controle quem pode ver suas informações
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium">Perfil Público</div>
                  <div className="text-sm text-muted-foreground">
                    Permitir que seu perfil seja encontrado no marketplace
                  </div>
                </div>
                <Switch
                  checked={privacy.profilePublic}
                  onCheckedChange={(checked) =>
                    setPrivacy({ ...privacy, profilePublic: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium">Exibir Email</div>
                  <div className="text-sm text-muted-foreground">
                    Mostrar seu email no perfil público
                  </div>
                </div>
                <Switch
                  checked={privacy.showEmail}
                  onCheckedChange={(checked) =>
                    setPrivacy({ ...privacy, showEmail: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium">Exibir Telefone</div>
                  <div className="text-sm text-muted-foreground">
                    Mostrar seu telefone no perfil público
                  </div>
                </div>
                <Switch
                  checked={privacy.showPhone}
                  onCheckedChange={(checked) =>
                    setPrivacy({ ...privacy, showPhone: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <div className="font-medium">Permitir Listagens no Marketplace</div>
                  <div className="text-sm text-muted-foreground">
                    Permitir que seus planos sejam vendidos no marketplace
                  </div>
                </div>
                <Switch
                  checked={privacy.allowMarketplaceListings}
                  onCheckedChange={(checked) =>
                    setPrivacy({ ...privacy, allowMarketplaceListings: checked })
                  }
                />
              </div>

              <Button
                onClick={handleSavePrivacy}
                disabled={savePrivacyMutation.isPending}
                className="w-full md:w-auto"
              >
                {savePrivacyMutation.isPending ? 'Salvando...' : 'Salvar Configurações'}
              </Button>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
