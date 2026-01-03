'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Plus,
  FileText,
  Eye,
  Edit,
  Trash2,
  MoreVertical,
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { apiClient } from '@/lib/api';
import { useToast } from '@/hooks/use-toast';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';

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

export default function PostsPage() {
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // Fetch posts
  const { data: posts, isLoading } = useQuery({
    queryKey: ['my-posts'],
    queryFn: async () => {
      try {
        const data = await apiClient.get<BlogPost[]>('/personal/posts');
        return data;
      } catch (error) {
        console.error('Failed to fetch posts:', error);
        return [];
      }
    },
    staleTime: 2 * 60 * 1000,
  });

  // Delete post mutation
  const deletePostMutation = useMutation({
    mutationFn: async (postId: string) => {
      await apiClient.delete(`/personal/posts/${postId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-posts'] });
      toast({
        title: 'Post excluído',
        description: 'O post foi excluído com sucesso.',
      });
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

  const handleDelete = (postId: string) => {
    if (confirm('Tem certeza que deseja excluir este post?')) {
      deletePostMutation.mutate(postId);
    }
  };

  const getTimeAgo = (date: string) => {
    return formatDistanceToNow(new Date(date), {
      addSuffix: true,
      locale: ptBR,
    });
  };

  return (
    <div className="container mx-auto p-6 max-w-7xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Posts do Blog</h1>
          <p className="text-muted-foreground mt-1">
            Crie conteúdo educacional para atrair e engajar clientes
          </p>
        </div>
        <Button onClick={() => router.push('/instructor/posts/new')}>
          <Plus className="h-4 w-4 mr-2" />
          Novo Post
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total de Posts
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">
              {isLoading ? '...' : posts?.length || 0}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Publicados
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-green-600">
              {isLoading ? '...' : posts?.filter(p => p.isPublished).length || 0}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Rascunhos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-yellow-600">
              {isLoading ? '...' : posts?.filter(p => !p.isPublished).length || 0}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Posts List */}
      {isLoading ? (
        <div className="text-center py-12">
          <p className="text-muted-foreground">Carregando posts...</p>
        </div>
      ) : posts && posts.length > 0 ? (
        <div className="space-y-4">
          {posts.map((post) => (
            <Card key={post.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-6">
                <div className="flex items-start justify-between gap-4">
                  {/* Post Image */}
                  {post.imageUrl && (
                    <img
                      src={post.imageUrl}
                      alt={post.title}
                      className="w-32 h-32 object-cover rounded-lg"
                    />
                  )}

                  {/* Post Content */}
                  <div className="flex-1">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h3 className="text-lg font-semibold mb-1">
                          {post.title}
                        </h3>
                        <div className="flex items-center gap-3 text-sm text-muted-foreground mb-2">
                          <span className="flex items-center gap-1">
                            <Eye className="h-3 w-3" />
                            {post.viewsCount || 0} visualizações
                          </span>
                          <span>•</span>
                          <span>{getTimeAgo(post.updatedAt)}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          {post.isPublished ? (
                            <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                              Publicado
                            </Badge>
                          ) : (
                            <Badge variant="outline" className="bg-yellow-50 text-yellow-700 border-yellow-200">
                              Rascunho
                            </Badge>
                          )}
                        </div>
                      </div>

                      {/* Actions */}
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => router.push(`/instructor/posts/${post.id}`)}
                          >
                            <Edit className="h-4 w-4 mr-2" />
                            Editar
                          </DropdownMenuItem>
                          {post.isPublished && (
                            <DropdownMenuItem
                              onClick={() => window.open(`/blog/${post.id}`, '_blank')}
                            >
                              <Eye className="h-4 w-4 mr-2" />
                              Ver Publicado
                            </DropdownMenuItem>
                          )}
                          <DropdownMenuItem
                            className="text-red-600"
                            onClick={() => handleDelete(post.id)}
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Excluir
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>

                    {/* Content Preview */}
                    <p className="text-sm text-muted-foreground mt-2 line-clamp-2">
                      {post.content.replace(/<[^>]*>/g, '').substring(0, 200)}...
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card className="border-2 border-dashed">
          <CardContent className="p-12 text-center">
            <FileText className="h-12 w-12 mx-auto mb-3 text-muted-foreground opacity-20" />
            <p className="text-muted-foreground mb-2">
              Nenhum post criado ainda
            </p>
            <p className="text-sm text-muted-foreground mb-4">
              Crie conteúdo educacional para atrair e engajar clientes
            </p>
            <Button onClick={() => router.push('/instructor/posts/new')}>
              <Plus className="h-4 w-4 mr-2" />
              Criar Primeiro Post
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
