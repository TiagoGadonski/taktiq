'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { X, Megaphone, Bell } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { apiClient } from '@/lib/api';

interface Announcement {
  id: string;
  title: string;
  content: string;
  type: string;
  imageUrl?: string;
  publishedAt: string;
  expiresAt?: string;
  priority: number;
  showAsPopup: boolean;
  isRead: boolean;
  readAt?: string;
}

const ANNOUNCEMENT_TYPE_LABELS: Record<string, string> = {
  NewFeature: 'Nova Funcionalidade',
  Maintenance: 'Manutenção',
  Tips: 'Dicas',
  General: 'Geral',
};

export function AnnouncementPopup() {
  const { user } = useAuth();
  const [announcements, setAnnouncements] = useState<Announcement[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [open, setOpen] = useState(false);

  // Fetch unread popup announcements
  const fetchAnnouncements = async () => {
    if (!user) return;

    try {
      const data = await apiClient.get<Announcement[]>(
        '/announcements?unreadOnly=true&popupOnly=true'
      );
      if (data && data.length > 0) {
        setAnnouncements(data);
        setCurrentIndex(0);
        setOpen(true);
      }
    } catch (error) {
      console.error('Error fetching announcements:', error);
    }
  };

  useEffect(() => {
    if (user) {
      // Fetch announcements when user logs in
      fetchAnnouncements();

      // Poll for new announcements every 5 minutes
      const interval = setInterval(fetchAnnouncements, 5 * 60 * 1000);

      return () => clearInterval(interval);
    }
  }, [user]);

  // Mark current announcement as read
  const markAsRead = async (announcementId: string) => {
    try {
      await apiClient.post(`/announcements/${announcementId}/mark-read`, {});
    } catch (error) {
      console.error('Error marking announcement as read:', error);
    }
  };

  // Handle dismiss (mark as read and show next)
  const handleDismiss = async () => {
    if (announcements.length === 0) return;

    const currentAnnouncement = announcements[currentIndex];
    await markAsRead(currentAnnouncement.id);

    // Show next announcement or close
    if (currentIndex < announcements.length - 1) {
      setCurrentIndex(currentIndex + 1);
    } else {
      setOpen(false);
      setAnnouncements([]);
      setCurrentIndex(0);
    }
  };

  // Handle dismiss all
  const handleDismissAll = async () => {
    try {
      await apiClient.post('/announcements/mark-all-read', {});
      setOpen(false);
      setAnnouncements([]);
      setCurrentIndex(0);
    } catch (error) {
      console.error('Error marking all announcements as read:', error);
    }
  };

  if (!user || announcements.length === 0) {
    return null;
  }

  const currentAnnouncement = announcements[currentIndex];
  const hasMore = currentIndex < announcements.length - 1;

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Megaphone className="h-5 w-5" />
              <DialogTitle>{currentAnnouncement.title}</DialogTitle>
            </div>
            <Badge variant="outline">
              {ANNOUNCEMENT_TYPE_LABELS[currentAnnouncement.type] || currentAnnouncement.type}
            </Badge>
          </div>
          {announcements.length > 1 && (
            <DialogDescription>
              Anúncio {currentIndex + 1} de {announcements.length}
            </DialogDescription>
          )}
        </DialogHeader>

        <div className="space-y-4">
          {currentAnnouncement.imageUrl && (
            <div className="w-full">
              <img
                src={currentAnnouncement.imageUrl}
                alt={currentAnnouncement.title}
                className="w-full rounded-lg object-cover max-h-64"
              />
            </div>
          )}

          <div className="prose prose-sm max-w-none">
            <p className="whitespace-pre-wrap text-sm">{currentAnnouncement.content}</p>
          </div>

          <div className="flex items-center justify-between pt-4 border-t">
            <p className="text-xs text-muted-foreground">
              Publicado em {new Date(currentAnnouncement.publishedAt).toLocaleDateString('pt-BR')}
            </p>
            <div className="flex gap-2">
              {announcements.length > 1 && (
                <Button variant="outline" size="sm" onClick={handleDismissAll}>
                  Dispensar Todos
                </Button>
              )}
              <Button onClick={handleDismiss}>
                {hasMore ? 'Próximo' : 'Entendi'}
              </Button>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
