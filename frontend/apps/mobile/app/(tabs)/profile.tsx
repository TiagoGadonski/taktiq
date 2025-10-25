import { View, Text, ScrollView, TouchableOpacity, Alert } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useAuth } from '@/hooks/use-auth';

export default function ProfileScreen() {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    Alert.alert('Sair', 'Deseja sair da sua conta?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Sair',
        style: 'destructive',
        onPress: async () => {
          await logout();
        },
      },
    ]);
  };

  return (
    <ScrollView className="flex-1 bg-background">
      <View className="px-6 pt-12 pb-6">
        <Text className="text-3xl font-bold text-foreground">Perfil</Text>
        <Text className="text-muted-foreground mt-2">Suas informações e configurações</Text>
      </View>

      {/* User Info */}
      <View className="px-6 mb-6">
        <View className="bg-card rounded-xl p-6 border border-border items-center">
          <View className="bg-primary/10 rounded-full h-20 w-20 items-center justify-center mb-4">
            <Text className="text-primary text-3xl font-bold">
              {user?.name?.charAt(0).toUpperCase()}
            </Text>
          </View>
          <Text className="text-foreground text-xl font-bold">{user?.name}</Text>
          <Text className="text-muted-foreground mt-1">{user?.email}</Text>
          {user?.createdAt && (
            <Text className="text-muted-foreground text-sm mt-2">
              Membro desde {new Date(user.createdAt).toLocaleDateString('pt-BR')}
            </Text>
          )}
        </View>
      </View>

      {/* Settings Sections */}
      <View className="px-6 space-y-6">
        {/* Account Settings */}
        <View>
          <Text className="text-foreground text-lg font-bold mb-3">Conta</Text>
          <View className="bg-card rounded-xl border border-border overflow-hidden">
            <TouchableOpacity className="flex-row items-center justify-between p-4 border-b border-border active:bg-muted">
              <View className="flex-row items-center gap-3">
                <Ionicons name="person-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Editar Perfil</Text>
              </View>
              <Ionicons name="chevron-forward" size={20} color="#94a3b8" />
            </TouchableOpacity>

            <TouchableOpacity className="flex-row items-center justify-between p-4 active:bg-muted">
              <View className="flex-row items-center gap-3">
                <Ionicons name="key-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Alterar Senha</Text>
              </View>
              <Ionicons name="chevron-forward" size={20} color="#94a3b8" />
            </TouchableOpacity>
          </View>
        </View>

        {/* Preferences */}
        <View>
          <Text className="text-foreground text-lg font-bold mb-3">Preferências</Text>
          <View className="bg-card rounded-xl border border-border overflow-hidden">
            <View className="flex-row items-center justify-between p-4 border-b border-border">
              <View className="flex-row items-center gap-3">
                <Ionicons name="moon-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Tema Escuro</Text>
              </View>
              <View className="bg-primary rounded-full px-3 py-1">
                <Text className="text-primary-foreground text-xs font-medium">Ativado</Text>
              </View>
            </View>

            <View className="flex-row items-center justify-between p-4 border-b border-border">
              <View className="flex-row items-center gap-3">
                <Ionicons name="language-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Idioma</Text>
              </View>
              <Text className="text-muted-foreground">Português (BR)</Text>
            </View>

            <View className="flex-row items-center justify-between p-4">
              <View className="flex-row items-center gap-3">
                <Ionicons name="speedometer-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Unidades</Text>
              </View>
              <Text className="text-muted-foreground">Métrico</Text>
            </View>
          </View>
        </View>

        {/* Notifications */}
        <View>
          <Text className="text-foreground text-lg font-bold mb-3">Notificações</Text>
          <View className="bg-card rounded-xl border border-border overflow-hidden">
            <View className="flex-row items-center justify-between p-4 border-b border-border">
              <View className="flex-row items-center gap-3">
                <Ionicons name="notifications-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Lembretes de Treino</Text>
              </View>
              <View className="bg-muted rounded-full px-3 py-1">
                <Text className="text-muted-foreground text-xs font-medium">Desativado</Text>
              </View>
            </View>

            <View className="flex-row items-center justify-between p-4">
              <View className="flex-row items-center gap-3">
                <Ionicons name="trophy-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Conquistas</Text>
              </View>
              <View className="bg-primary rounded-full px-3 py-1">
                <Text className="text-primary-foreground text-xs font-medium">Ativado</Text>
              </View>
            </View>
          </View>
        </View>

        {/* About */}
        <View>
          <Text className="text-foreground text-lg font-bold mb-3">Sobre</Text>
          <View className="bg-card rounded-xl border border-border overflow-hidden">
            <TouchableOpacity className="flex-row items-center justify-between p-4 border-b border-border active:bg-muted">
              <View className="flex-row items-center gap-3">
                <Ionicons name="help-circle-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Ajuda e Suporte</Text>
              </View>
              <Ionicons name="chevron-forward" size={20} color="#94a3b8" />
            </TouchableOpacity>

            <TouchableOpacity className="flex-row items-center justify-between p-4 border-b border-border active:bg-muted">
              <View className="flex-row items-center gap-3">
                <Ionicons name="document-text-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Termos de Uso</Text>
              </View>
              <Ionicons name="chevron-forward" size={20} color="#94a3b8" />
            </TouchableOpacity>

            <View className="flex-row items-center justify-between p-4">
              <View className="flex-row items-center gap-3">
                <Ionicons name="information-circle-outline" size={20} color="#94a3b8" />
                <Text className="text-foreground">Versão</Text>
              </View>
              <Text className="text-muted-foreground">1.0.0</Text>
            </View>
          </View>
        </View>

        {/* Logout Button */}
        <TouchableOpacity
          className="bg-destructive rounded-xl p-4 items-center active:opacity-80 mb-6"
          onPress={handleLogout}
        >
          <View className="flex-row items-center gap-2">
            <Ionicons name="log-out-outline" size={20} color="white" />
            <Text className="text-white font-bold">Sair da Conta</Text>
          </View>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
}
