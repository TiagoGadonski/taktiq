'use client';

import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { MapPin, Star, Navigation, ExternalLink, Smartphone } from 'lucide-react';

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
  formatted_phone_number?: string;
  website?: string;
}

interface GymDetailsModalProps {
  gym: Gym | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function GymDetailsModal({ gym, open, onOpenChange }: GymDetailsModalProps) {
  if (!gym) return null;

  const googleMapsUrl = `https://www.google.com/maps/search/?api=1&query=${gym.geometry.location.lat},${gym.geometry.location.lng}&query_place_id=${gym.place_id}`;

  // Google Maps Embed API
  // Note: Replace YOUR_GOOGLE_MAPS_API_KEY with actual API key in production
  const embedUrl = `https://www.google.com/maps/embed/v1/place?key=${process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY || 'YOUR_GOOGLE_MAPS_API_KEY'}&q=place_id:${gym.place_id}&zoom=15`;

  const isMobile = typeof window !== 'undefined' && window.innerWidth < 768;

  const handleOpenInMaps = () => {
    window.open(googleMapsUrl, '_blank');
  };

  const handleOpenInApp = () => {
    // Try to open in Google Maps app on mobile
    const appUrl = `geo:${gym.geometry.location.lat},${gym.geometry.location.lng}?q=${encodeURIComponent(gym.name)}`;
    window.location.href = appUrl;
    // Fallback to web if app doesn't open
    setTimeout(() => {
      window.open(googleMapsUrl, '_blank');
    }, 1000);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold flex items-start justify-between">
            <span className="flex-1">{gym.name}</span>
            {gym.opening_hours?.open_now !== undefined && (
              <Badge
                variant="outline"
                className={
                  gym.opening_hours.open_now
                    ? 'bg-green-500/20 text-green-500 border-green-500/30'
                    : 'bg-red-500/20 text-red-500 border-red-500/30'
                }
              >
                {gym.opening_hours.open_now ? 'Aberto' : 'Fechado'}
              </Badge>
            )}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Map - Desktop: Embedded, Mobile: Image with button */}
          {!isMobile ? (
            <div className="w-full h-[400px] rounded-lg overflow-hidden border border-border">
              <iframe
                src={embedUrl}
                width="100%"
                height="100%"
                style={{ border: 0 }}
                allowFullScreen
                loading="lazy"
                referrerPolicy="no-referrer-when-downgrade"
              ></iframe>
            </div>
          ) : (
            <div className="w-full h-[200px] bg-gradient-to-br from-primary/20 to-primary/5 rounded-lg flex flex-col items-center justify-center gap-3 border border-border">
              <MapPin className="h-16 w-16 text-primary/30" />
              <Button onClick={handleOpenInApp} className="gap-2">
                <Smartphone className="h-4 w-4" />
                Abrir no App de Mapas
              </Button>
            </div>
          )}

          {/* Gym Details */}
          <div className="space-y-3">
            {/* Address */}
            <div className="flex items-start gap-3">
              <MapPin className="h-5 w-5 text-muted-foreground mt-0.5 flex-shrink-0" />
              <div>
                <p className="font-medium text-sm">Endereço</p>
                <p className="text-sm text-muted-foreground">{gym.vicinity}</p>
              </div>
            </div>

            {/* Rating */}
            {gym.rating && (
              <div className="flex items-start gap-3">
                <Star className="h-5 w-5 text-yellow-500 fill-yellow-500 mt-0.5" />
                <div>
                  <p className="font-medium text-sm">Avaliação</p>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-semibold">{gym.rating.toFixed(1)}</span>
                    {gym.user_ratings_total && (
                      <span className="text-xs text-muted-foreground">
                        ({gym.user_ratings_total} avaliações)
                      </span>
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-2 pt-4 border-t">
            <Button
              onClick={handleOpenInMaps}
              className="flex-1 gap-2"
              variant="outline"
            >
              <ExternalLink className="h-4 w-4" />
              Abrir no Google Maps
            </Button>
            <Button
              onClick={() => {
                const directionsUrl = `https://www.google.com/maps/dir/?api=1&destination=${gym.geometry.location.lat},${gym.geometry.location.lng}&destination_place_id=${gym.place_id}`;
                window.open(directionsUrl, '_blank');
              }}
              className="flex-1 gap-2"
            >
              <Navigation className="h-4 w-4" />
              Como Chegar
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
