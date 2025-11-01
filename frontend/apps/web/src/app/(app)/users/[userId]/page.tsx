'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  MapPin,
  Dumbbell,
  Phone,
  Mail,
  Trophy,
  Calendar,
  Target,
  Award,
  Star,
  Users,
  TrendingUp,
  Zap,
  CheckCircle2
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';
import { getChallengeIcon } from '@/components/challenge-icon-library';

interface WorkoutSummary {
  id: string;
  planName: string;
  completedAt: string;
}

interface CompletedChallenge {
  challengeId: string;
  title: string;
  type: string;
  targetValue: number;
  currentValue: number;
  completedAt: string;
  iconName?: string;
}

interface UserProfile {
  id: string;
  name: string;
  email: string;
  location?: string;
  bio?: string;
  profilePictureUrl?: string;
  gymName?: string;
  phoneNumber?: string;
  recentWorkouts?: WorkoutSummary[];
  completedChallenges?: CompletedChallenge[];
}

export default function UserProfilePage() {
  const params = useParams();
  const userId = params.userId as string;
  const { toast } = useToast();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  };

  const getChallengeIconComponent = (challenge: CompletedChallenge) => {
    // Use custom icon if available
    if (challenge.iconName) {
      const IconComponent = getChallengeIcon(challenge.iconName);
      return <IconComponent className="h-6 w-6" />;
    }

    // Fallback to type-based icons
    const typeMap: Record<string, React.ReactNode> = {
      'Setup': <CheckCircle2 className="h-6 w-6" />,
      'Planos': <Target className="h-6 w-6" />,
      'Exercícios': <Dumbbell className="h-6 w-6" />,
      'Treinos': <TrendingUp className="h-6 w-6" />,
      'PR': <Zap className="h-6 w-6" />,
      'Volume': <Award className="h-6 w-6" />,
      'Social': <Users className="h-6 w-6" />,
    };
    return typeMap[challenge.type] || <Trophy className="h-6 w-6" />;
  };

  const getChallengeColor = (type: string) => {
    const colorMap: Record<string, string> = {
      'Setup': 'from-blue-500 to-cyan-500',
      'Planos': 'from-purple-500 to-pink-500',
      'Exercícios': 'from-orange-500 to-red-500',
      'Treinos': 'from-green-500 to-emerald-500',
      'PR': 'from-yellow-500 to-amber-500',
      'Volume': 'from-indigo-500 to-violet-500',
      'Social': 'from-rose-500 to-pink-500',
    };
    return colorMap[type] || 'from-gray-500 to-slate-500';
  };

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await apiClient.get<any>(`/users/${userId}/profile`);
        // Handle both wrapped and unwrapped responses
        const data = response.data || response;
        setProfile(data);
      } catch (error: any) {
        toast({
          variant: 'destructive',
          title: 'Erro ao carregar perfil',
          description: error.response?.data?.message || 'Não foi possível carregar o perfil do usuário',
        });
      } finally {
        setLoading(false);
      }
    };

    if (userId) {
      fetchProfile();
    }
  }, [userId, toast]);

  if (loading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Perfil não encontrado</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Profile Header */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage
                src={getAssetUrl(profile.profilePictureUrl)}
              />
              <AvatarFallback className="text-2xl">
                {getInitials(profile.name)}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1 space-y-4">
              <div>
                <h1 className="text-3xl font-bold">{profile.name}</h1>
                {profile.bio && (
                  <p className="text-muted-foreground mt-2">{profile.bio}</p>
                )}
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {profile.location && (
                  <div className="flex items-center gap-2 text-sm">
                    <MapPin className="h-4 w-4 text-muted-foreground" />
                    <span>{profile.location}</span>
                  </div>
                )}
                {profile.gymName && (
                  <div className="flex items-center gap-2 text-sm">
                    <Dumbbell className="h-4 w-4 text-muted-foreground" />
                    <span>{profile.gymName}</span>
                  </div>
                )}
                {profile.email && (
                  <div className="flex items-center gap-2 text-sm">
                    <Mail className="h-4 w-4 text-muted-foreground" />
                    <span>{profile.email}</span>
                  </div>
                )}
                {profile.phoneNumber && (
                  <div className="flex items-center gap-2 text-sm">
                    <Phone className="h-4 w-4 text-muted-foreground" />
                    <span>{profile.phoneNumber}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Recent Workouts */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Dumbbell className="h-5 w-5" />
            <CardTitle>Treinos Recentes</CardTitle>
          </div>
          <CardDescription>Últimos treinos completados</CardDescription>
        </CardHeader>
        <CardContent>
          {profile.recentWorkouts && profile.recentWorkouts.length > 0 ? (
            <div className="space-y-3">
              {profile.recentWorkouts.map((workout) => (
                <div
                  key={workout.id}
                  className="flex items-center justify-between p-3 rounded-lg border"
                >
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-full bg-primary/10">
                      <Dumbbell className="h-4 w-4 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium">{workout.planName}</p>
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        {new Date(workout.completedAt).toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-6">
              Nenhum treino completado ainda
            </p>
          )}
        </CardContent>
      </Card>

      {/* Completed Challenges */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Trophy className="h-5 w-5" />
            <CardTitle>Desafios Completados</CardTitle>
          </div>
          <CardDescription>Conquistas alcançadas</CardDescription>
        </CardHeader>
        <CardContent>
          {profile.completedChallenges && profile.completedChallenges.length > 0 ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
              {profile.completedChallenges.map((challenge) => (
                <div
                  key={challenge.challengeId}
                  className="group relative flex flex-col items-center p-4 rounded-xl border-2 bg-card hover:scale-105 transition-all cursor-pointer"
                  title={`${challenge.title} - Completado em ${new Date(challenge.completedAt).toLocaleDateString('pt-BR')}`}
                >
                  {/* Badge Icon with Gradient Background */}
                  <div className={`relative p-4 rounded-full bg-gradient-to-br ${getChallengeColor(challenge.type)} shadow-lg mb-3`}>
                    <div className="text-white">
                      {getChallengeIconComponent(challenge)}
                    </div>
                    {/* Shine effect */}
                    <div className="absolute inset-0 rounded-full bg-gradient-to-tr from-white/0 via-white/20 to-white/0 opacity-0 group-hover:opacity-100 transition-opacity" />
                  </div>

                  {/* Challenge Title */}
                  <h3 className="font-semibold text-sm text-center line-clamp-2 mb-1">
                    {challenge.title}
                  </h3>

                  {/* Progress Badge */}
                  <Badge variant="secondary" className="text-[10px] mb-2">
                    {challenge.type}
                  </Badge>

                  {/* Completion Date (shown on hover) */}
                  <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-2 bg-black/90 text-white text-xs rounded-lg opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-10">
                    <div className="font-medium mb-1">{challenge.title}</div>
                    <div className="flex items-center gap-1 text-xs text-gray-300">
                      <Calendar className="h-3 w-3" />
                      {new Date(challenge.completedAt).toLocaleDateString('pt-BR')}
                    </div>
                    <div className="flex items-center gap-1 text-xs text-gray-300 mt-1">
                      <Target className="h-3 w-3" />
                      {challenge.currentValue} / {challenge.targetValue}
                    </div>
                    {/* Tooltip arrow */}
                    <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-px border-4 border-transparent border-t-black/90" />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="p-4 rounded-full bg-muted mb-4">
                <Trophy className="h-8 w-8 text-muted-foreground" />
              </div>
              <p className="text-sm text-muted-foreground text-center">
                Nenhum desafio completado ainda
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
