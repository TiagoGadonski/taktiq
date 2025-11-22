'use client';

import { useState, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Upload, X, Video, Loader2 } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { VideoPlayer } from './video-player';

interface VideoUploadProps {
  onVideoUploaded: (url: string) => void;
  currentVideoUrl?: string;
  usageContext?: string;
  entityId?: string;
  className?: string;
}

export function VideoUpload({
  onVideoUploaded,
  currentVideoUrl,
  usageContext,
  entityId,
  className = '',
}: VideoUploadProps) {
  const [uploading, setUploading] = useState(false);
  const [videoUrl, setVideoUrl] = useState<string | null>(currentVideoUrl || null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { toast } = useToast();

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('video/')) {
      toast({
        variant: 'destructive',
        title: 'Tipo de arquivo inválido',
        description: 'Por favor, selecione um vídeo.',
      });
      return;
    }

    // Validate file size (max 100MB)
    const maxSize = 100 * 1024 * 1024;
    if (file.size > maxSize) {
      toast({
        variant: 'destructive',
        title: 'Arquivo muito grande',
        description: 'O vídeo deve ter no máximo 100MB.',
      });
      return;
    }

    // Upload file
    setUploading(true);
    setUploadProgress(0);

    try {
      const formData = new FormData();
      formData.append('file', file);
      if (usageContext) formData.append('usageContext', usageContext);
      if (entityId) formData.append('entityId', entityId);

      const response = await apiClient.post<{ fileUrl: string }>('/media/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent: any) => {
          if (progressEvent.total) {
            const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            setUploadProgress(percentCompleted);
          }
        },
      });

      setVideoUrl(response.fileUrl);
      onVideoUploaded(response.fileUrl);
      toast({
        title: 'Vídeo enviado!',
        description: 'O vídeo foi enviado com sucesso.',
      });
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao enviar vídeo',
        description: error?.response?.data?.message || 'Não foi possível enviar o vídeo.',
      });
      setVideoUrl(currentVideoUrl || null);
    } finally {
      setUploading(false);
      setUploadProgress(0);
      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleRemove = () => {
    setVideoUrl(null);
    onVideoUploaded('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className={`space-y-4 ${className}`}>
      <input
        ref={fileInputRef}
        type="file"
        accept="video/*"
        onChange={handleFileSelect}
        className="hidden"
        disabled={uploading}
      />

      {videoUrl ? (
        <div className="relative group">
          <VideoPlayer src={videoUrl} className="h-64" />
          <div className="absolute top-2 right-2 flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
            <Button
              type="button"
              onClick={handleClick}
              disabled={uploading}
              variant="secondary"
              size="sm"
              className="hover-lift tap-scale"
            >
              {uploading ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <>
                  <Upload className="h-4 w-4 mr-2" />
                  Alterar
                </>
              )}
            </Button>
            <Button
              type="button"
              onClick={handleRemove}
              disabled={uploading}
              variant="destructive"
              size="sm"
              className="hover-lift tap-scale"
            >
              <X className="h-4 w-4 mr-2" />
              Remover
            </Button>
          </div>
        </div>
      ) : (
        <button
          type="button"
          onClick={handleClick}
          disabled={uploading}
          className="w-full h-64 glass border-2 border-dashed border-primary/30 hover:border-primary/50 rounded-lg flex flex-col items-center justify-center gap-3 transition-all hover-lift tap-scale"
        >
          {uploading ? (
            <>
              <Loader2 className="h-12 w-12 text-primary animate-spin" />
              <p className="text-sm text-muted-foreground">Enviando vídeo... {uploadProgress}%</p>
              <div className="w-64 h-2 bg-muted rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary transition-all duration-300"
                  style={{ width: `${uploadProgress}%` }}
                />
              </div>
            </>
          ) : (
            <>
              <Video className="h-12 w-12 text-muted-foreground" />
              <div className="text-center">
                <p className="text-sm font-medium">Clique para fazer upload de vídeo</p>
                <p className="text-xs text-muted-foreground mt-1">MP4, WebM até 100MB</p>
              </div>
            </>
          )}
        </button>
      )}
    </div>
  );
}
