'use client';

import { useQuery } from '@tanstack/react-query';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  MapPin,
  Award,
  GraduationCap,
  Briefcase,
  Instagram,
  Facebook,
  Globe,
  ExternalLink,
  User
} from 'lucide-react';
import Link from 'next/link';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';

interface TrainerProfile {
  id: string;
  name: string;
  profileSlug: string | null;
  profilePictureUrl: string | null;
  bio: string | null;
  location: string | null;
  specialization: string | null;
  education: string | null;
  experience: string | null;
  pricingInfo: string | null;
  instagramUrl: string | null;
  facebookUrl: string | null;
  websiteUrl: string | null;
  clientCount: number;
}

interface InstructorCardProps {
  trainerId: string;
}

export function InstructorCard({ trainerId }: InstructorCardProps) {
  const { data: trainer, isLoading } = useQuery<TrainerProfile>({
    queryKey: ['trainer', trainerId],
    queryFn: () => apiClient.get(`/trainer/id/${trainerId}`),
    enabled: !!trainerId,
  });

  if (isLoading) {
    return (
      <Card className="border-primary/20 animate-pulse">
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <div className="h-20 w-20 rounded-full bg-muted" />
            <div className="flex-1 space-y-2">
              <div className="h-6 bg-muted rounded w-1/3" />
              <div className="h-4 bg-muted rounded w-1/2" />
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!trainer) {
    return null;
  }

  const profileUrl = trainer.profileSlug
    ? `/trainer/${trainer.profileSlug}`
    : null;

  return (
    <Card className="border-primary/20 bg-gradient-to-br from-primary/5 to-primary/10 overflow-hidden">
      <CardContent className="p-0">
        {/* Header with Profile Link */}
        <div className="bg-gradient-to-r from-primary/10 to-primary/20 p-4 border-b border-primary/20">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <User className="h-5 w-5 text-primary" />
              <h3 className="font-semibold text-lg">Seu Personal Trainer</h3>
            </div>
            {profileUrl && (
              <Link href={profileUrl}>
                <Button variant="ghost" size="sm" className="text-primary hover:text-primary/80">
                  Ver Perfil
                  <ExternalLink className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            )}
          </div>
        </div>

        {/* Main Content */}
        <div className="p-6">
          <div className="flex flex-col sm:flex-row gap-6">
            {/* Avatar and Basic Info */}
            <div className="flex flex-col items-center sm:items-start gap-3">
              <Avatar className="h-24 w-24 ring-4 ring-primary/20">
                <AvatarImage src={getAssetUrl(trainer.profilePictureUrl)} alt={trainer.name} />
                <AvatarFallback className="bg-primary/20 text-primary text-2xl font-bold">
                  {trainer.name.charAt(0).toUpperCase()}
                </AvatarFallback>
              </Avatar>

              {trainer.location && (
                <div className="flex items-center gap-1 text-sm text-muted-foreground">
                  <MapPin className="h-4 w-4" />
                  {trainer.location}
                </div>
              )}
            </div>

            {/* Details */}
            <div className="flex-1 space-y-4">
              <div>
                <h4 className="text-2xl font-bold">{trainer.name}</h4>
                {trainer.specialization && (
                  <Badge variant="secondary" className="mt-1">
                    {trainer.specialization}
                  </Badge>
                )}
              </div>

              {trainer.bio && (
                <p className="text-muted-foreground line-clamp-3">{trainer.bio}</p>
              )}

              {/* Professional Info */}
              <div className="grid gap-2 sm:grid-cols-2">
                {trainer.experience && (
                  <div className="flex items-start gap-2">
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div className="text-sm">
                      <p className="font-medium">Experiência</p>
                      <p className="text-muted-foreground">{trainer.experience}</p>
                    </div>
                  </div>
                )}

                {trainer.education && (
                  <div className="flex items-start gap-2">
                    <GraduationCap className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div className="text-sm">
                      <p className="font-medium">Formação</p>
                      <p className="text-muted-foreground">{trainer.education}</p>
                    </div>
                  </div>
                )}
              </div>

              {/* Stats */}
              <div className="flex items-center gap-2 pt-2">
                <Award className="h-4 w-4 text-primary" />
                <span className="text-sm font-medium">{trainer.clientCount} alunos</span>
              </div>

              {/* Social Links */}
              {(trainer.instagramUrl || trainer.facebookUrl || trainer.websiteUrl) && (
                <div className="flex flex-wrap gap-2 pt-2 border-t border-primary/10">
                  {trainer.instagramUrl && (
                    <Link href={trainer.instagramUrl} target="_blank">
                      <Button variant="outline" size="sm" className="gap-2">
                        <Instagram className="h-4 w-4" />
                        Instagram
                      </Button>
                    </Link>
                  )}
                  {trainer.facebookUrl && (
                    <Link href={trainer.facebookUrl} target="_blank">
                      <Button variant="outline" size="sm" className="gap-2">
                        <Facebook className="h-4 w-4" />
                        Facebook
                      </Button>
                    </Link>
                  )}
                  {trainer.websiteUrl && (
                    <Link href={trainer.websiteUrl} target="_blank">
                      <Button variant="outline" size="sm" className="gap-2">
                        <Globe className="h-4 w-4" />
                        Website
                      </Button>
                    </Link>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
