'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  UserCog,
  Search,
  MapPin,
  Users,
  Sparkles,
  Filter,
  X,
} from 'lucide-react';
import Link from 'next/link';

interface Trainer {
  id: string;
  name: string;
  profileSlug?: string;
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

export default function TrainersPage() {
  const [searchTerm, setSearchTerm] = useState('');
  const [specializationFilter, setSpecializationFilter] = useState('');
  const [locationFilter, setLocationFilter] = useState('');
  const [showFilters, setShowFilters] = useState(false);

  const { data: trainers, isLoading } = useQuery({
    queryKey: ['trainers', searchTerm, specializationFilter, locationFilter],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (searchTerm) params.append('search', searchTerm);
      if (specializationFilter) params.append('specialization', specializationFilter);
      if (locationFilter) params.append('location', locationFilter);

      const queryString = params.toString();
      const url = `/trainer${queryString ? `?${queryString}` : ''}`;
      const response = await apiClient.get<Trainer[]>(url);
      return Array.isArray(response) ? response : [];
    },
  });

  const clearFilters = () => {
    setSearchTerm('');
    setSpecializationFilter('');
    setLocationFilter('');
  };

  const hasActiveFilters = searchTerm || specializationFilter || locationFilter;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <UserCog className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Encontre seu Personal Trainer
          </h1>
        </div>
        <p className="text-muted-foreground">
          Descubra personal trainers qualificados na sua região
        </p>
      </div>

      {/* Search and Filters */}
      <Card className="glass border-primary/20 p-6">
        <div className="space-y-4">
          {/* Search Bar */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Buscar por nome, especialização ou bio..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 glass border-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Filter Toggle */}
          <div className="flex items-center justify-between">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowFilters(!showFilters)}
              className="hover-lift tap-scale"
            >
              <Filter className="mr-2 h-4 w-4" />
              {showFilters ? 'Esconder Filtros' : 'Mostrar Filtros'}
            </Button>

            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="sm"
                onClick={clearFilters}
                className="text-muted-foreground hover:text-foreground"
              >
                <X className="mr-2 h-4 w-4" />
                Limpar Filtros
              </Button>
            )}
          </div>

          {/* Filter Inputs */}
          {showFilters && (
            <div className="grid gap-4 md:grid-cols-2 pt-4 border-t border-border/50 animate-scale-in">
              <div className="space-y-2">
                <label className="text-sm font-medium">Especialização</label>
                <Input
                  placeholder="Ex: Musculação, Funcional, Crossfit"
                  value={specializationFilter}
                  onChange={(e) => setSpecializationFilter(e.target.value)}
                  className="glass"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Localização</label>
                <Input
                  placeholder="Ex: São Paulo, Rio de Janeiro"
                  value={locationFilter}
                  onChange={(e) => setLocationFilter(e.target.value)}
                  className="glass"
                />
              </div>
            </div>
          )}
        </div>
      </Card>

      {/* Results Count */}
      {!isLoading && trainers && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {trainers.length} {trainers.length === 1 ? 'trainer encontrado' : 'trainers encontrados'}
          </p>
          {hasActiveFilters && (
            <div className="flex gap-2 flex-wrap">
              {searchTerm && (
                <Badge variant="outline" className="border-primary/30">
                  Busca: {searchTerm}
                </Badge>
              )}
              {specializationFilter && (
                <Badge variant="outline" className="border-primary/30">
                  Especialização: {specializationFilter}
                </Badge>
              )}
              {locationFilter && (
                <Badge variant="outline" className="border-primary/30">
                  Localização: {locationFilter}
                </Badge>
              )}
            </div>
          )}
        </div>
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Card key={i} className="glass border-primary/20 p-6 animate-pulse">
              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  <div className="h-16 w-16 rounded-full bg-muted" />
                  <div className="flex-1 space-y-2">
                    <div className="h-4 bg-muted rounded w-3/4" />
                    <div className="h-3 bg-muted rounded w-1/2" />
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="h-3 bg-muted rounded" />
                  <div className="h-3 bg-muted rounded w-5/6" />
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Trainers Grid */}
      {!isLoading && trainers && trainers.length > 0 && (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {trainers.map((trainer, index) => (
            <Card
              key={trainer.id}
              className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
              style={{ animationDelay: `${index * 50}ms` }}
            >
              <div className="p-6">
                {/* Trainer Header */}
                <div className="flex items-start gap-4 mb-4">
                  <Avatar className="h-16 w-16 ring-2 ring-primary/30">
                    <AvatarImage src={getAssetUrl(trainer.profilePictureUrl)} />
                    <AvatarFallback className="bg-primary/20 text-primary text-xl font-bold">
                      {trainer.name.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 min-w-0">
                    <h3 className="font-bold text-lg truncate">{trainer.name}</h3>
                    {trainer.location && (
                      <p className="text-sm text-muted-foreground flex items-center gap-1">
                        <MapPin className="h-3 w-3" />
                        {trainer.location}
                      </p>
                    )}
                  </div>
                </div>

                {/* Specialization */}
                {trainer.specialization && (
                  <div className="mb-3">
                    <Badge className="bg-primary/20 text-primary border-primary/30">
                      <Sparkles className="h-3 w-3 mr-1" />
                      {trainer.specialization}
                    </Badge>
                  </div>
                )}

                {/* Bio */}
                {trainer.bio && (
                  <p className="text-sm text-muted-foreground line-clamp-3 mb-4">
                    {trainer.bio}
                  </p>
                )}

                {/* Student Count */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-4">
                  <Users className="h-4 w-4 text-primary" />
                  <span className="font-semibold text-foreground">{trainer.studentCount}</span>
                  <span>{trainer.studentCount === 1 ? 'aluno' : 'alunos'}</span>
                </div>

                {/* View Profile Button */}
                {trainer.profileSlug ? (
                  <Link href={`/trainer/${trainer.profileSlug}`}>
                    <Button className="w-full bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 hover-lift tap-scale">
                      Ver Perfil Completo
                    </Button>
                  </Link>
                ) : (
                  <Button disabled className="w-full opacity-50">
                    Perfil não disponível
                  </Button>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Empty State */}
      {!isLoading && trainers && trainers.length === 0 && (
        <Card className="glass border-primary/20 p-12 text-center">
          <UserCog className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
          <h3 className="text-lg font-semibold mb-2">
            Nenhum personal trainer encontrado
          </h3>
          <p className="text-muted-foreground mb-4">
            {hasActiveFilters
              ? 'Tente ajustar seus filtros de busca'
              : 'Ainda não há personal trainers com perfil público'}
          </p>
          {hasActiveFilters && (
            <Button onClick={clearFilters} variant="outline" className="hover-lift tap-scale">
              Limpar Filtros
            </Button>
          )}
        </Card>
      )}
    </div>
  );
}
