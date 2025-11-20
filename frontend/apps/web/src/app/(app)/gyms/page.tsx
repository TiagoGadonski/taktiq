'use client';

import { useState, useEffect } from 'react';
import { MapPin, Navigation, Star, Phone, Globe, ArrowLeft, Loader2, MapPinned, Info } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/use-toast';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';

interface Gym {
  place_id: string;
  name: string;
  vicinity: string;
  rating?: number;
  user_ratings_total?: number;
  geometry: {
    location: {
      lat: number;
      lng: number;
    };
  };
  photos?: Array<{
    photo_reference: string;
  }>;
  opening_hours?: {
    open_now: boolean;
  };
}

export default function GymsNearMePage() {
  const router = useRouter();
  const { toast } = useToast();
  const [gyms, setGyms] = useState<Gym[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null);
  const [searchLocation, setSearchLocation] = useState('');
  const [isLoadingLocation, setIsLoadingLocation] = useState(false);

  const fetchGyms = async (lat: number, lng: number) => {
    setIsLoading(true);
    try {
      // Using a proxy service or backend endpoint to call Google Places API
      // For now, we'll use a mock implementation
      // In production, create a backend endpoint that calls Google Places API

      const response = await fetch(
        `/api/places?lat=${lat}&lng=${lng}&type=gym&radius=5000`
      );

      if (!response.ok) {
        throw new Error('Failed to fetch gyms');
      }

      const data = await response.json();
      setGyms(data.results || []);
    } catch (error) {
      // Fallback to mock data for development
      const mockGyms: Gym[] = [
        {
          place_id: '1',
          name: 'Academia SmartFit',
          vicinity: 'Av. Paulista, 1000 - São Paulo',
          rating: 4.5,
          user_ratings_total: 250,
          geometry: {
            location: { lat: lat + 0.01, lng: lng + 0.01 },
          },
          opening_hours: { open_now: true },
        },
        {
          place_id: '2',
          name: 'Bio Ritmo',
          vicinity: 'Rua Augusta, 500 - São Paulo',
          rating: 4.7,
          user_ratings_total: 180,
          geometry: {
            location: { lat: lat - 0.01, lng: lng - 0.01 },
          },
          opening_hours: { open_now: true },
        },
        {
          place_id: '3',
          name: 'Bodytech',
          vicinity: 'Shopping Iguatemi - São Paulo',
          rating: 4.8,
          user_ratings_total: 320,
          geometry: {
            location: { lat: lat + 0.02, lng: lng - 0.01 },
          },
          opening_hours: { open_now: false },
        },
        {
          place_id: '4',
          name: 'Clube Atlético',
          vicinity: 'Rua dos Pinheiros, 200 - São Paulo',
          rating: 4.3,
          user_ratings_total: 95,
          geometry: {
            location: { lat: lat - 0.015, lng: lng + 0.02 },
          },
          opening_hours: { open_now: true },
        },
      ];
      setGyms(mockGyms);
    } finally {
      setIsLoading(false);
    }
  };

  const getCurrentLocation = () => {
    setIsLoadingLocation(true);
    if ('geolocation' in navigator) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const { latitude, longitude } = position.coords;
          setUserLocation({ lat: latitude, lng: longitude });
          fetchGyms(latitude, longitude);
          setIsLoadingLocation(false);
          toast({
            title: 'Localização obtida!',
            description: 'Buscando academias próximas...',
          });
        },
        (error) => {
          setIsLoadingLocation(false);
          toast({
            title: 'Erro ao obter localização',
            description: 'Por favor, permita o acesso à sua localização ou busque por endereço.',
            variant: 'destructive',
          });
          // Fallback to default location (São Paulo)
          const defaultLat = -23.5505;
          const defaultLng = -46.6333;
          setUserLocation({ lat: defaultLat, lng: defaultLng });
          fetchGyms(defaultLat, defaultLng);
        }
      );
    } else {
      setIsLoadingLocation(false);
      toast({
        title: 'Geolocalização não suportada',
        description: 'Seu navegador não suporta geolocalização.',
        variant: 'destructive',
      });
    }
  };

  const searchByAddress = async () => {
    if (!searchLocation.trim()) {
      toast({
        title: 'Digite um endereço',
        description: 'Por favor, insira um endereço para buscar.',
        variant: 'destructive',
      });
      return;
    }

    setIsLoading(true);
    try {
      // TODO: Implement geocoding to convert address to coordinates
      // For now, use default location
      const defaultLat = -23.5505;
      const defaultLng = -46.6333;
      setUserLocation({ lat: defaultLat, lng: defaultLng });
      await fetchGyms(defaultLat, defaultLng);
    } catch (error) {
      toast({
        title: 'Erro na busca',
        description: 'Não foi possível buscar academias neste endereço.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    getCurrentLocation();
  }, []);

  const getPhotoUrl = (photoReference?: string) => {
    if (!photoReference) return null;
    // In production, use actual Google Places Photo API
    return `https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photo_reference=${photoReference}&key=YOUR_API_KEY`;
  };

  const openInMaps = (gym: Gym) => {
    const url = `https://www.google.com/maps/search/?api=1&query=${gym.geometry.location.lat},${gym.geometry.location.lng}&query_place_id=${gym.place_id}`;
    window.open(url, '_blank');
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
            className="hover-lift tap-scale"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <MapPinned className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Academias Próximas
          </h1>
        </div>
        <p className="text-muted-foreground ml-14">
          Encontre as melhores academias perto de você
        </p>
      </div>

      {/* Coming Soon Banner */}
      <Alert className="glass border-yellow-500/50 bg-yellow-500/10">
        <Info className="h-5 w-5 text-yellow-500" />
        <AlertTitle className="text-yellow-500 font-semibold">Em Breve</AlertTitle>
        <AlertDescription className="text-yellow-500/80">
          Esta funcionalidade está em desenvolvimento e estará disponível em breve. Por enquanto, você pode explorar as outras funcionalidades do app!
        </AlertDescription>
      </Alert>

      {/* Search and Location */}
      <Card className="glass border-primary/20">
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="flex-1 relative">
              <MapPin className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Digite um endereço ou CEP"
                value={searchLocation}
                onChange={(e) => setSearchLocation(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && searchByAddress()}
                className="pl-10 glass"
              />
            </div>
            <div className="flex gap-2">
              <Button
                onClick={searchByAddress}
                disabled={isLoading}
                className="hover-lift tap-scale"
              >
                Buscar
              </Button>
              <Button
                onClick={getCurrentLocation}
                disabled={isLoadingLocation}
                variant="outline"
                className="hover-lift tap-scale"
              >
                {isLoadingLocation ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Navigation className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results */}
      {isLoading ? (
        <div className="flex justify-center items-center py-12">
          <div className="text-center">
            <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
            <p className="text-muted-foreground">Buscando academias próximas...</p>
          </div>
        </div>
      ) : (
        <>
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              {gyms.length} {gyms.length === 1 ? 'academia encontrada' : 'academias encontradas'}
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {gyms.map((gym) => (
              <Card
                key={gym.place_id}
                className="glass hover-lift tap-scale border-primary/20 overflow-hidden cursor-pointer transition-all hover:shadow-lg"
                onClick={() => openInMaps(gym)}
              >
                <div className="h-40 bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center relative">
                  <MapPin className="h-16 w-16 text-primary/30" />
                  {gym.opening_hours?.open_now !== undefined && (
                    <div className="absolute top-2 right-2">
                      <span
                        className={`text-xs px-2 py-1 rounded-full ${
                          gym.opening_hours.open_now
                            ? 'bg-green-500/20 text-green-500 border border-green-500/30'
                            : 'bg-red-500/20 text-red-500 border border-red-500/30'
                        }`}
                      >
                        {gym.opening_hours.open_now ? 'Aberto' : 'Fechado'}
                      </span>
                    </div>
                  )}
                </div>

                <CardContent className="pt-4">
                  <h3 className="font-semibold text-lg mb-2">{gym.name}</h3>

                  <div className="flex items-center gap-1 mb-2">
                    <MapPin className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                    <p className="text-sm text-muted-foreground line-clamp-1">{gym.vicinity}</p>
                  </div>

                  {gym.rating && (
                    <div className="flex items-center gap-2 mb-3">
                      <div className="flex items-center gap-1">
                        <Star className="h-4 w-4 fill-yellow-500 text-yellow-500" />
                        <span className="font-medium">{gym.rating.toFixed(1)}</span>
                      </div>
                      {gym.user_ratings_total && (
                        <span className="text-xs text-muted-foreground">
                          ({gym.user_ratings_total} avaliações)
                        </span>
                      )}
                    </div>
                  )}

                  <Button
                    onClick={(e) => {
                      e.stopPropagation();
                      openInMaps(gym);
                    }}
                    className="w-full hover-lift tap-scale"
                    size="sm"
                  >
                    <MapPin className="h-4 w-4 mr-2" />
                    Ver no Mapa
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>

          {gyms.length === 0 && !isLoading && (
            <Card className="glass border-primary/20">
              <CardContent className="py-12 text-center">
                <MapPin className="h-16 w-16 mx-auto text-muted-foreground mb-4" />
                <h3 className="text-lg font-semibold mb-2">Nenhuma academia encontrada</h3>
                <p className="text-muted-foreground mb-4">
                  Tente buscar em outro local ou ampliar o raio de busca
                </p>
                <Button onClick={getCurrentLocation} variant="outline" className="hover-lift tap-scale">
                  <Navigation className="mr-2 h-4 w-4" />
                  Usar Minha Localização
                </Button>
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );
}
