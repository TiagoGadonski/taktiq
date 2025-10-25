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
  Target
} from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';

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

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await apiClient.get(`/users/${userId}/profile`);
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

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <div className="space-y-6">
      {/* Profile Header */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage src={profile.profilePictureUrl ? `http://localhost:5001${profile.profilePictureUrl}` : undefined} />
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
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {profile.completedChallenges.map((challenge) => (
                <div
                  key={challenge.challengeId}
                  className="p-4 rounded-lg border bg-card hover:bg-accent/50 transition-colors"
                >
                  <div className="flex items-start gap-3">
                    <div className="p-2 rounded-full bg-yellow-500/10">
                      <Trophy className="h-5 w-5 text-yellow-500" />
                    </div>
                    <div className="flex-1 space-y-2">
                      <h3 className="font-semibold">{challenge.title}</h3>
                      <Badge variant="secondary" className="text-xs">
                        {challenge.type}
                      </Badge>
                      <div className="flex items-center gap-2 text-sm">
                        <Target className="h-4 w-4 text-muted-foreground" />
                        <span className="text-muted-foreground">
                          {challenge.currentValue} / {challenge.targetValue}
                        </span>
                      </div>
                      <p className="text-xs text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        Completado em {new Date(challenge.completedAt).toLocaleDateString('pt-BR')}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-6">
              Nenhum desafio completado ainda
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
