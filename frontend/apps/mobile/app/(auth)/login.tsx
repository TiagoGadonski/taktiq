import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform } from 'react-native';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from 'expo-router';
import { loginSchema, type LoginInput } from '@gymhero/shared';
import { useAuth } from '@/hooks/use-auth';

export default function LoginScreen() {
  const { login, isLoginPending } = useAuth();
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginInput) => {
    try {
      await login(data);
    } catch (error) {
      // Error is handled by the auth hook
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      className="flex-1 bg-gradient-to-br from-blue-500 to-purple-600"
    >
      <View className="flex-1 justify-center px-6">
        <View className="bg-card rounded-2xl p-6 shadow-xl">
          <Text className="text-3xl font-bold text-foreground mb-2">GymHero</Text>
          <Text className="text-muted-foreground mb-6">Entre com sua conta para continuar</Text>

          <View className="space-y-4">
            <View>
              <Text className="text-sm font-medium text-foreground mb-2">Email</Text>
              <Controller
                control={control}
                name="email"
                render={({ field: { onChange, onBlur, value } }) => (
                  <TextInput
                    className="bg-background border border-muted rounded-lg px-4 py-3 text-foreground"
                    placeholder="seu@email.com"
                    placeholderTextColor="#64748b"
                    keyboardType="email-address"
                    autoCapitalize="none"
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                )}
              />
              {errors.email && (
                <Text className="text-destructive text-sm mt-1">{errors.email.message}</Text>
              )}
            </View>

            <View>
              <Text className="text-sm font-medium text-foreground mb-2">Senha</Text>
              <Controller
                control={control}
                name="password"
                render={({ field: { onChange, onBlur, value } }) => (
                  <TextInput
                    className="bg-background border border-muted rounded-lg px-4 py-3 text-foreground"
                    placeholder="••••••••"
                    placeholderTextColor="#64748b"
                    secureTextEntry
                    onBlur={onBlur}
                    onChangeText={onChange}
                    value={value}
                  />
                )}
              />
              {errors.password && (
                <Text className="text-destructive text-sm mt-1">{errors.password.message}</Text>
              )}
            </View>

            <TouchableOpacity
              className="bg-primary rounded-lg py-4 items-center active:opacity-80"
              onPress={handleSubmit(onSubmit)}
              disabled={isLoginPending}
            >
              <Text className="text-primary-foreground font-semibold text-base">
                {isLoginPending ? 'Entrando...' : 'Entrar'}
              </Text>
            </TouchableOpacity>
          </View>

          <View className="mt-6 flex-row justify-center">
            <Text className="text-muted-foreground text-sm">Não tem uma conta? </Text>
            <Link href="/(auth)/signup" asChild>
              <TouchableOpacity>
                <Text className="text-primary font-medium text-sm">Criar conta</Text>
              </TouchableOpacity>
            </Link>
          </View>
        </View>
      </View>
    </KeyboardAvoidingView>
  );
}
