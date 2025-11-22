'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';
import { useAuth } from '@/hooks/use-auth';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  UserCog,
  MapPin,
  GraduationCap,
  Briefcase,
  DollarSign,
  Users,
  Instagram,
  Facebook,
  Globe,
  Mail,
  ArrowLeft,
  Sparkles,
  FileText,
  Dumbbell,
  Calendar,
  Eye,
  TrendingUp,
  Home,
} from 'lucide-react';
import Link from 'next/link';
import { PostFeed } from '@/components/posts/post-feed';

interface PublicProfileData {
  id: string;
  name: string;
  profilePictureUrl?: string;
  bio?: string;
  location?: string;
  specialization?: string;
  education?: string;
  experience?: string;
  pricingInfo?: string;
  instagramUrl?: string;
  facebookUrl?: string;
  websiteUrl?: string;
  studentCount: number;
}

interface Post {
  id: string;
  title: string;
  content: string;
  imageUrl?: string;
  authorId: string;
  authorName: string;
  authorProfilePictureUrl?: string;
  isPublished: boolean;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export default function TrainerPublicProfilePage() {
  const params = useParams();
  const router = useRouter();
  const slug = params?.slug as string;
  const { user, isAuthenticated } = useAuth();

  const [profile, setProfile] = useState<PublicProfileData | null>(null);
  const [posts, setPosts] = useState<Post[]>([]);
  const [plans, setPlans] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      if (!slug) return;

      try {
        setIsLoading(true);
        const response = await apiClient.get<PublicProfileData>(`/trainer/${slug}`);
        setProfile(response);
        setNotFound(false);
      } catch (error: any) {
        console.error('Error fetching profile:', error);
        if (error.response?.status === 404) {
          setNotFound(true);
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [slug]);

  // Fetch posts for this trainer
  useEffect(() => {
    const fetchPosts = async () => {
      if (!profile?.id) return;

      try {
        const response = await apiClient.get<Post[]>(`/posts/trainer/${profile.id}`);
        setPosts(Array.isArray(response) ? response : []);
      } catch (error: any) {
        console.error('Error fetching posts:', error);
        setPosts([]);
      }
    };

    fetchPosts();
  }, [profile?.id]);

  // Fetch public plans for this trainer
  useEffect(() => {
    const fetchPlans = async () => {
      if (!profile?.id) return;

      try {
        // Fetch public plans from this trainer (sample/top plans)
        const response = await apiClient.get(`/workout-plans/user/${profile.id}/public?pageSize=4`);
        setPlans(Array.isArray(response) ? response : []);
      } catch (error: any) {
        // Silently handle 404 - trainer may not have public plans yet
        if (error.response?.status === 404) {
          setPlans([]);
        } else {
          console.error('Error fetching plans:', error);
          setPlans([]);
        }
      }
    };

    fetchPlans();
  }, [profile?.id]);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
          <p className="text-muted-foreground">Carregando perfil...</p>
        </div>
      </div>
    );
  }

  if (notFound || !profile) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center space-y-4">
          <UserCog className="h-16 w-16 text-muted-foreground mx-auto opacity-50" />
          <h1 className="text-2xl font-bold">Personal Trainer não encontrado</h1>
          <p className="text-muted-foreground">
            O perfil que você está procurando não existe ou não está público.
          </p>
          <Button onClick={() => router.push('/trainers')} className="hover-lift tap-scale">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar para Personal Trainers
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pb-8">
      {/* Header with gradient background */}
      <div className="relative bg-gradient-to-br from-primary/20 via-primary/10 to-background border-b">
        <div className="container mx-auto px-4 py-12 max-w-5xl">
          <Button
            variant="ghost"
            onClick={() => router.push('/trainers')}
            className="mb-6 hover-lift tap-scale"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar
          </Button>

          <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
            <Avatar className="h-32 w-32 ring-4 ring-primary/30 shadow-xl">
              <AvatarImage src={getAssetUrl(profile.profilePictureUrl)} />
              <AvatarFallback className="bg-primary/20 text-primary text-4xl font-bold">
                {profile.name.charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>

            <div className="flex-1">
              <div className="flex items-center gap-3 mb-2">
                <h1 className="text-3xl md:text-4xl font-bold">{profile.name}</h1>
                <Badge className="bg-primary/20 text-primary border-primary/30">
                  <UserCog className="h-3 w-3 mr-1" />
                  Personal Trainer
                </Badge>
              </div>

              {profile.location && (
                <p className="text-muted-foreground flex items-center gap-2 mb-3">
                  <MapPin className="h-4 w-4" />
                  {profile.location}
                </p>
              )}

              {profile.bio && (
                <p className="text-muted-foreground mb-4 max-w-2xl">{profile.bio}</p>
              )}

              <div className="flex flex-wrap items-center gap-3">
                <div className="flex items-center gap-2 text-sm">
                  <Users className="h-4 w-4 text-primary" />
                  <span className="font-semibold">{profile.studentCount}</span>
                  <span className="text-muted-foreground">
                    {profile.studentCount === 1 ? 'aluno' : 'alunos'}
                  </span>
                </div>

                {/* Social Media Links */}
                <div className="flex gap-2">
                  {profile.instagramUrl && (
                    <a
                      href={profile.instagramUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-2 rounded-full bg-pink-500/10 hover:bg-pink-500/20 text-pink-500 transition-colors hover-lift tap-scale"
                    >
                      <Instagram className="h-4 w-4" />
                    </a>
                  )}
                  {profile.facebookUrl && (
                    <a
                      href={profile.facebookUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-2 rounded-full bg-blue-500/10 hover:bg-blue-500/20 text-blue-500 transition-colors hover-lift tap-scale"
                    >
                      <Facebook className="h-4 w-4" />
                    </a>
                  )}
                  {profile.websiteUrl && (
                    <a
                      href={profile.websiteUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-2 rounded-full bg-primary/10 hover:bg-primary/20 text-primary transition-colors hover-lift tap-scale"
                    >
                      <Globe className="h-4 w-4" />
                    </a>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="container mx-auto px-4 py-8 max-w-5xl">
        {/* Stats Section */}
        <div className="grid gap-4 md:grid-cols-3 mb-8">
          <Card className="glass border-primary/20 p-6 text-center hover-lift">
            <div className="text-3xl font-bold text-primary mb-1">{profile.studentCount}</div>
            <p className="text-sm text-muted-foreground">{profile.studentCount === 1 ? 'Aluno' : 'Alunos'}</p>
          </Card>
          <Card className="glass border-primary/20 p-6 text-center hover-lift">
            <div className="text-3xl font-bold text-primary mb-1">{posts.length}</div>
            <p className="text-sm text-muted-foreground">{posts.length === 1 ? 'Post Publicado' : 'Posts Publicados'}</p>
          </Card>
          <Card className="glass border-primary/20 p-6 text-center hover-lift">
            <div className="text-3xl font-bold text-primary mb-1">{plans.length}</div>
            <p className="text-sm text-muted-foreground">{plans.length === 1 ? 'Plano Criado' : 'Planos Criados'}</p>
          </Card>
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          {/* Specialization */}
          {profile.specialization && (
            <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in">
              <div className="flex items-start gap-3">
                <div className="p-3 bg-primary/20 rounded-lg shrink-0">
                  <Sparkles className="h-5 w-5 text-primary" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-semibold mb-2">Especialização</h3>
                  <p className="text-muted-foreground whitespace-pre-wrap break-words">
                    {profile.specialization}
                  </p>
                </div>
              </div>
            </Card>
          )}

          {/* Education */}
          {profile.education && (
            <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '100ms' }}>
              <div className="flex items-start gap-3">
                <div className="p-3 bg-blue-500/20 rounded-lg shrink-0">
                  <GraduationCap className="h-5 w-5 text-blue-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-semibold mb-2">Formação</h3>
                  <p className="text-muted-foreground whitespace-pre-wrap break-words">
                    {profile.education}
                  </p>
                </div>
              </div>
            </Card>
          )}

          {/* Experience */}
          {profile.experience && (
            <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '200ms' }}>
              <div className="flex items-start gap-3">
                <div className="p-3 bg-green-500/20 rounded-lg shrink-0">
                  <Briefcase className="h-5 w-5 text-green-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-semibold mb-2">Experiência</h3>
                  <p className="text-muted-foreground whitespace-pre-wrap break-words">
                    {profile.experience}
                  </p>
                </div>
              </div>
            </Card>
          )}

          {/* Pricing */}
          {profile.pricingInfo && (
            <Card className="glass border-primary/20 p-6 hover-lift animate-scale-in" style={{ animationDelay: '300ms' }}>
              <div className="flex items-start gap-3">
                <div className="p-3 bg-orange-500/20 rounded-lg shrink-0">
                  <DollarSign className="h-5 w-5 text-orange-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-semibold mb-2">Investimento</h3>
                  <p className="text-muted-foreground whitespace-pre-wrap break-words">
                    {profile.pricingInfo}
                  </p>
                </div>
              </div>
            </Card>
          )}
        </div>

        {/* Sample Workout Plans Section */}
        {plans.length > 0 && (
          <div className="mt-12">
            <div className="flex items-center gap-3 mb-6">
              <Dumbbell className="h-6 w-6 text-primary" />
              <h2 className="text-2xl font-bold">Planos de Treino</h2>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              {plans.slice(0, 4).map((plan: any, index: number) => (
                <Card
                  key={plan.id}
                  className="glass border-primary/20 p-6 hover-lift animate-scale-in"
                  style={{ animationDelay: `${index * 50}ms` }}
                >
                  <div className="space-y-3">
                    <div>
                      <h3 className="font-bold text-lg mb-1">{plan.name}</h3>
                      {plan.description && (
                        <p className="text-sm text-muted-foreground line-clamp-2">
                          {plan.description}
                        </p>
                      )}
                    </div>

                    <div className="flex flex-wrap gap-3 text-sm">
                      {plan.goal && (
                        <Badge variant="secondary" className="flex items-center gap-1">
                          <TrendingUp className="h-3 w-3" />
                          {plan.goal}
                        </Badge>
                      )}
                      {plan.duration && (
                        <Badge variant="outline" className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {plan.duration} semanas
                        </Badge>
                      )}
                    </div>

                    {plan.viewCount > 0 && (
                      <div className="flex items-center gap-2 text-sm text-muted-foreground pt-2 border-t">
                        <Eye className="h-4 w-4" />
                        <span>{plan.viewCount} visualizações</span>
                      </div>
                    )}
                  </div>
                </Card>
              ))}
            </div>
          </div>
        )}

        {/* Posts Section */}
        {posts.length > 0 && (
          <div className="mt-12">
            <div className="flex items-center gap-3 mb-6">
              <FileText className="h-6 w-6 text-primary" />
              <h2 className="text-2xl font-bold">Artigos e Dicas</h2>
            </div>
            <PostFeed posts={posts} showAuthor={false} />
          </div>
        )}

        {/* Contact CTA */}
        <Card className="glass border-primary/20 p-8 mt-8 text-center bg-gradient-to-br from-primary/10 to-background">
          <h3 className="text-2xl font-bold mb-3">Interessado em treinar com {profile.name.split(' ')[0]}?</h3>
          <p className="text-muted-foreground mb-6 max-w-2xl mx-auto">
            {isAuthenticated
              ? 'Entre em contato através das redes sociais para iniciar sua jornada fitness!'
              : 'Entre em contato através das redes sociais ou crie sua conta no TaktIQ para começar sua jornada fitness!'}
          </p>
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            {(profile.instagramUrl || profile.facebookUrl || profile.websiteUrl) && (
              <div className="flex gap-2 justify-center">
                {profile.instagramUrl && (
                  <a href={profile.instagramUrl} target="_blank" rel="noopener noreferrer">
                    <Button variant="outline" className="hover-lift tap-scale">
                      <Instagram className="mr-2 h-4 w-4" />
                      Instagram
                    </Button>
                  </a>
                )}
                {profile.facebookUrl && (
                  <a href={profile.facebookUrl} target="_blank" rel="noopener noreferrer">
                    <Button variant="outline" className="hover-lift tap-scale">
                      <Facebook className="mr-2 h-4 w-4" />
                      Facebook
                    </Button>
                  </a>
                )}
                {profile.websiteUrl && (
                  <a href={profile.websiteUrl} target="_blank" rel="noopener noreferrer">
                    <Button variant="outline" className="hover-lift tap-scale">
                      <Globe className="mr-2 h-4 w-4" />
                      Website
                    </Button>
                  </a>
                )}
              </div>
            )}
            {!isAuthenticated && (
              <Link href="/signup">
                <Button className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale">
                  <UserCog className="mr-2 h-4 w-4" />
                  Criar Conta
                </Button>
              </Link>
            )}
            {isAuthenticated && (
              <Link href="/dashboard">
                <Button className="bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale">
                  <Home className="mr-2 h-4 w-4" />
                  Voltar ao Dashboard
                </Button>
              </Link>
            )}
          </div>
        </Card>
      </div>
    </div>
  );
}
