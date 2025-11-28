'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent } from '@/components/ui/card';
import { toast } from '@/components/ui/use-toast';
import { MessageSquare, Send, Trash2, Edit2, Reply, Loader2, X } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface Comment {
  id: string;
  userId: string;
  userName: string;
  content: string;
  parentCommentId: string | null;
  createdAt: string;
  replies: Comment[];
}

interface PlanCommentsProps {
  planId: string;
  currentUserId?: string;
}

interface CommentItemProps {
  comment: Comment;
  currentUserId?: string;
  onReply: (commentId: string) => void;
  onEdit: (commentId: string, content: string) => void;
  onDelete: (commentId: string) => void;
  isNested?: boolean;
}

function CommentItem({ comment, currentUserId, onReply, onEdit, onDelete, isNested = false }: CommentItemProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editContent, setEditContent] = useState(comment.content);
  const isOwner = currentUserId === comment.userId;

  const handleSaveEdit = () => {
    onEdit(comment.id, editContent);
    setIsEditing(false);
  };

  const handleCancelEdit = () => {
    setEditContent(comment.content);
    setIsEditing(false);
  };

  return (
    <div className={`space-y-2 ${isNested ? 'ml-8 mt-3' : ''}`}>
      <Card className={isNested ? 'border-l-4 border-l-primary/30' : ''}>
        <CardContent className="pt-4">
          <div className="flex justify-between items-start mb-2">
            <div>
              <span className="font-semibold text-sm">{comment.userName}</span>
              <span className="text-xs text-muted-foreground ml-2">
                {formatDistanceToNow(new Date(comment.createdAt), {
                  addSuffix: true,
                  locale: ptBR
                })}
              </span>
            </div>

            {isOwner && !isEditing && (
              <div className="flex gap-1">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setIsEditing(true)}
                  className="h-7 w-7 p-0"
                >
                  <Edit2 className="h-3 w-3" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onDelete(comment.id)}
                  className="h-7 w-7 p-0 text-destructive hover:text-destructive"
                >
                  <Trash2 className="h-3 w-3" />
                </Button>
              </div>
            )}
          </div>

          {isEditing ? (
            <div className="space-y-2">
              <Textarea
                value={editContent}
                onChange={(e) => setEditContent(e.target.value)}
                rows={3}
                className="text-sm"
              />
              <div className="flex gap-2">
                <Button size="sm" onClick={handleSaveEdit}>
                  Salvar
                </Button>
                <Button size="sm" variant="outline" onClick={handleCancelEdit}>
                  <X className="h-4 w-4 mr-1" />
                  Cancelar
                </Button>
              </div>
            </div>
          ) : (
            <>
              <p className="text-sm whitespace-pre-wrap">{comment.content}</p>
              {!isNested && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onReply(comment.id)}
                  className="mt-2 h-7 text-xs"
                >
                  <Reply className="h-3 w-3 mr-1" />
                  Responder
                </Button>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Replies */}
      {comment.replies.length > 0 && (
        <div className="space-y-2">
          {comment.replies.map((reply) => (
            <CommentItem
              key={reply.id}
              comment={reply}
              currentUserId={currentUserId}
              onReply={onReply}
              onEdit={onEdit}
              onDelete={onDelete}
              isNested={true}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export function PlanComments({ planId, currentUserId }: PlanCommentsProps) {
  const [newComment, setNewComment] = useState('');
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyContent, setReplyContent] = useState('');
  const queryClient = useQueryClient();

  // Fetch comments
  const { data: comments = [], isLoading } = useQuery<Comment[]>({
    queryKey: ['plan-comments', planId],
    queryFn: async () => {
      return apiClient.get(`/workout-plans/${planId}/comments`);
    },
  });

  // Create comment mutation
  const createMutation = useMutation({
    mutationFn: async (data: { content: string; parentCommentId: string | null }) => {
      return apiClient.post(`/workout-plans/${planId}/comments`, {
        workoutPlanId: planId,
        content: data.content,
        parentCommentId: data.parentCommentId,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plan-comments', planId] });
      setNewComment('');
      setReplyContent('');
      setReplyingTo(null);
      toast({
        title: 'Comentário publicado!',
        description: 'Seu comentário foi adicionado com sucesso.',
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao publicar comentário',
        description: error.response?.data?.message || 'Tente novamente.',
      });
    },
  });

  // Update comment mutation
  const updateMutation = useMutation({
    mutationFn: async (data: { commentId: string; content: string }) => {
      return apiClient.put(`/workout-plans/${planId}/comments/${data.commentId}`, {
        content: data.content,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plan-comments', planId] });
      toast({
        title: 'Comentário atualizado!',
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao atualizar comentário',
        description: error.response?.data?.message || 'Tente novamente.',
      });
    },
  });

  // Delete comment mutation
  const deleteMutation = useMutation({
    mutationFn: async (commentId: string) => {
      return apiClient.delete(`/workout-plans/${planId}/comments/${commentId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plan-comments', planId] });
      toast({
        title: 'Comentário excluído!',
      });
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao excluir comentário',
        description: error.response?.data?.message || 'Tente novamente.',
      });
    },
  });

  const handleSubmit = () => {
    if (newComment.trim()) {
      createMutation.mutate({ content: newComment, parentCommentId: null });
    }
  };

  const handleReply = (commentId: string) => {
    setReplyingTo(commentId);
  };

  const handleSubmitReply = () => {
    if (replyContent.trim() && replyingTo) {
      createMutation.mutate({ content: replyContent, parentCommentId: replyingTo });
    }
  };

  const handleEdit = (commentId: string, content: string) => {
    updateMutation.mutate({ commentId, content });
  };

  const handleDelete = (commentId: string) => {
    if (confirm('Tem certeza que deseja excluir este comentário?')) {
      deleteMutation.mutate(commentId);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-6 w-6 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold flex items-center gap-2">
          <MessageSquare className="h-5 w-5" />
          Comentários ({comments.length})
        </h3>
      </div>

      {/* New comment form */}
      <Card>
        <CardContent className="pt-4">
          <div className="space-y-2">
            <Textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Adicione um comentário sobre este plano..."
              rows={3}
            />
            <Button
              onClick={handleSubmit}
              disabled={createMutation.isPending || !newComment.trim()}
              size="sm"
            >
              {createMutation.isPending ? (
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              ) : (
                <Send className="h-4 w-4 mr-2" />
              )}
              Comentar
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Comments list */}
      <div className="space-y-3">
        {comments.length === 0 ? (
          <Card>
            <CardContent className="pt-6 text-center text-sm text-muted-foreground">
              Nenhum comentário ainda. Seja o primeiro a comentar!
            </CardContent>
          </Card>
        ) : (
          comments.map((comment) => (
            <div key={comment.id}>
              <CommentItem
                comment={comment}
                currentUserId={currentUserId}
                onReply={handleReply}
                onEdit={handleEdit}
                onDelete={handleDelete}
              />

              {/* Reply form */}
              {replyingTo === comment.id && (
                <Card className="ml-8 mt-2 border-l-4 border-l-primary">
                  <CardContent className="pt-4">
                    <div className="space-y-2">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium">
                          Respondendo a {comment.userName}
                        </span>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => {
                            setReplyingTo(null);
                            setReplyContent('');
                          }}
                          className="h-7 w-7 p-0"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                      <Textarea
                        value={replyContent}
                        onChange={(e) => setReplyContent(e.target.value)}
                        placeholder="Escreva sua resposta..."
                        rows={3}
                      />
                      <Button
                        onClick={handleSubmitReply}
                        disabled={createMutation.isPending || !replyContent.trim()}
                        size="sm"
                      >
                        {createMutation.isPending ? (
                          <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        ) : (
                          <Send className="h-4 w-4 mr-2" />
                        )}
                        Responder
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
