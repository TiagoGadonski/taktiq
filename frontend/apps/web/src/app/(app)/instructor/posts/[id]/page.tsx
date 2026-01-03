'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Switch } from '@/components/ui/switch';
import { ArrowLeft, Save, Trash2 } from 'lucide-react';
import { apiClient } from '@/lib/api';
import { useToast } from '@/hooks/use-toast';

interface BlogPost {
  id: string;
  title: string;
  content: string;
  imageUrl?: string;
  isPublished: boolean;
  viewsCount: number;
  createdAt: string;
  updatedAt: string;
}

export default function EditPostPage() {
  const router = useRouter();
  const params = useParams();
  const postId = params?.id as string;
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [isPublished, setIsPublished] = useState(false);

  // Fetch post
  const { data: post, isLoading } = useQuery({
    queryKey: ['post', postId],
    queryFn: async () => {
      try {
        const posts = await apiClient.get<BlogPost[]>('/personal/posts');
        const post = posts.find(p => p.id === postId);
        if (!post) {
          throw new Error('Post not found');
        }
        return post;
      } catch (error) {
        console.error('Failed to fetch post:', error);
        throw error;
      }
    },
    enabled: !!postId,
  });

  // Update form when post loads
  useEffect(() => {
    if (post) {
      setTitle(post.title);
      setContent(post.content);
      setImageUrl(post.imageUrl || '');
      setIsPublished(post.isPublished);
    }
  }, [post]);

  // Update post mutation
  const updatePostMutation = useMutation({
    mutationFn: async (data: { title: string; content: string; imageUrl?: string; isPublished: boolean }) => {
      await apiClient.put(`/personal/posts/${postId}`, data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-posts'] });
      queryClient.invalidateQueries({ queryKey: ['post', postId] });
      toast({
        title: 'Post atualizado',
        description: 'Seu post foi atualizado com sucesso.',
      });
      router.push('/instructor/posts');
    },
    onError: (error: any) => {
      console.error('Failed to update post:', error);
      toast({
        title: 'Erro ao atualizar post',
        description: error.response?.data?.message || 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Delete post mutation
  const deletePostMutation = useMutation({
    mutationFn: async () => {
      await apiClient.delete(`/personal/posts/${postId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-posts'] });
      toast({
        title: 'Post excluído',
        description: 'O post foi excluído com sucesso.',
      });
      router.push('/instructor/posts');
    },
    onError: (error: any) => {
      console.error('Failed to delete post:', error);
      toast({
        title: 'Erro ao excluir',
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

    updatePostMutation.mutate({
      title,
      content,
      imageUrl: imageUrl.trim() || undefined,
      isPublished,
    });
  };

  const handleDelete = () => {
    if (confirm('Tem certeza que deseja excluir este post?')) {
      deletePostMutation.mutate();
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6 max-w-4xl">
        <p className="text-center text-muted-foreground">Carregando...</p>
      </div>
    );
  }

  if (!post) {
    return (
      <div className="container mx-auto p-6 max-w-4xl">
        <p className="text-center text-muted-foreground">Post não encontrado</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
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
            <h1 className="text-3xl font-bold">Editar Post</h1>
            <p className="text-muted-foreground mt-1">
              Atualize o conteúdo do seu post
            </p>
          </div>
        </div>
        <Button
          variant="destructive"
          onClick={handleDelete}
          disabled={deletePostMutation.isPending}
        >
          <Trash2 className="h-4 w-4 mr-2" />
          Excluir
        </Button>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader>
            <CardTitle>Informações do Post</CardTitle>
            <CardDescription>
              Edite os detalhes do seu post do blog
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
                <div className="font-medium">Status de Publicação</div>
                <div className="text-sm text-muted-foreground">
                  {isPublished
                    ? 'O post está visível publicamente'
                    : 'O post está salvo como rascunho'}
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
                disabled={updatePostMutation.isPending}
                className="flex-1"
              >
                <Save className="h-4 w-4 mr-2" />
                {updatePostMutation.isPending
                  ? 'Salvando...'
                  : 'Salvar Alterações'}
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
