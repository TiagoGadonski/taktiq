'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { ArrowLeft, Save, Eye } from 'lucide-react';
import { apiClient } from '@/lib/api';
import { useToast } from '@/hooks/use-toast';

export default function NewPostPage() {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [isPublished, setIsPublished] = useState(false);

  // Create post mutation
  const createPostMutation = useMutation({
    mutationFn: async (data: { title: string; content: string; imageUrl?: string; isPublished: boolean }) => {
      await apiClient.post('/personal/posts', data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-posts'] });
      toast({
        title: 'Post criado',
        description: 'Seu post foi criado com sucesso.',
      });
      router.push('/instructor/posts');
    },
    onError: (error: any) => {
      console.error('Failed to create post:', error);
      toast({
        title: 'Erro ao criar post',
        description: error.response?.data?.message || 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!title.trim()) {
      toast({
        title: 'Título obrigatório',
        description: 'Por favor, insira um título para o post.',
        variant: 'destructive',
      });
      return;
    }

    if (!content.trim()) {
      toast({
        title: 'Conteúdo obrigatório',
        description: 'Por favor, insira o conteúdo do post.',
        variant: 'destructive',
      });
      return;
    }

    createPostMutation.mutate({
      title,
      content,
      imageUrl: imageUrl.trim() || undefined,
      isPublished,
    });
  };

  return (
    <div className="container mx-auto p-6 max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/instructor/posts')}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Voltar
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Novo Post</h1>
          <p className="text-muted-foreground mt-1">
            Crie conteúdo educacional para seu blog
          </p>
        </div>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Informações do Post</CardTitle>
            <CardDescription>
              Preencha os detalhes do seu post do blog
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Title */}
            <div className="space-y-2">
              <Label htmlFor="title">Título *</Label>
              <Input
                id="title"
                placeholder="Ex: Como ganhar massa muscular"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required
              />
            </div>

            {/* Image URL */}
            <div className="space-y-2">
              <Label htmlFor="imageUrl">URL da Imagem (opcional)</Label>
              <Input
                id="imageUrl"
                type="url"
                placeholder="https://exemplo.com/imagem.jpg"
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
              />
              {imageUrl && (
                <div className="mt-2">
                  <img
                    src={imageUrl}
                    alt="Preview"
                    className="w-full max-w-md h-48 object-cover rounded-lg"
                    onError={(e) => {
                      (e.target as HTMLImageElement).style.display = 'none';
                    }}
                  />
                </div>
              )}
            </div>

            {/* Content */}
            <div className="space-y-2">
              <Label htmlFor="content">Conteúdo *</Label>
              <Textarea
                id="content"
                placeholder="Escreva o conteúdo do seu post aqui..."
                value={content}
                onChange={(e) => setContent(e.target.value)}
                rows={15}
                className="resize-none font-mono"
                required
              />
              <p className="text-xs text-muted-foreground">
                Suporta HTML e Markdown
              </p>
            </div>

            {/* Publish Toggle */}
            <div className="flex items-center justify-between p-4 border rounded-lg">
              <div>
                <div className="font-medium">Publicar Imediatamente</div>
                <div className="text-sm text-muted-foreground">
                  {isPublished
                    ? 'O post ficará visível publicamente'
                    : 'O post será salvo como rascunho'}
                </div>
              </div>
              <Switch
                checked={isPublished}
                onCheckedChange={setIsPublished}
              />
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3 pt-4">
              <Button
                type="submit"
                disabled={createPostMutation.isPending}
                className="flex-1"
              >
                <Save className="h-4 w-4 mr-2" />
                {createPostMutation.isPending
                  ? 'Salvando...'
                  : isPublished
                  ? 'Publicar Post'
                  : 'Salvar Rascunho'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push('/instructor/posts')}
              >
                Cancelar
              </Button>
            </div>
          </CardContent>
        </Card>
      </form>
    </div>
  );
}
