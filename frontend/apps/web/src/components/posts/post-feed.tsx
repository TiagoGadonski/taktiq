'use client';

import { useState } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Calendar, FileText } from 'lucide-react';
import { getAssetUrl } from '@/lib/env';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import Link from 'next/link';

interface Post {
  id: string;
  title: string;
  content: string;
  imageUrl?: string;
  authorId: string;
  authorName: string;
  authorProfilePictureUrl?: string;
  authorProfileSlug?: string;
  isPublished: boolean;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

interface PostFeedProps {
  posts: Post[];
  showAuthor?: boolean;
  compact?: boolean;
}

export function PostFeed({ posts, showAuthor = false, compact = false }: PostFeedProps) {
  const [expandedPosts, setExpandedPosts] = useState<Set<string>>(new Set());

  const togglePost = (postId: string) => {
    setExpandedPosts((prev) => {
      const next = new Set(prev);
      if (next.has(postId)) {
        next.delete(postId);
      } else {
        next.add(postId);
      }
      return next;
    });
  };

  if (posts.length === 0) {
    return (
      <div className="text-center py-12">
        <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4 opacity-50" />
        <h3 className="text-lg font-semibold mb-2">Nenhum post publicado</h3>
        <p className="text-muted-foreground">
          Este personal trainer ainda não publicou nenhum conteúdo.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {posts.map((post, index) => {
        const isExpanded = expandedPosts.has(post.id);
        const content = post.content || '';
        const contentPreview = content.slice(0, 200);
        const needsReadMore = content.length > 200;

        return (
          <Card
            key={post.id}
            className="glass border-primary/20 overflow-hidden hover-lift animate-scale-in"
            style={{ animationDelay: `${index * 100}ms` }}
          >
            {/* Post Image */}
            {post.imageUrl && (
              <div className="w-full h-48 overflow-hidden bg-muted">
                <img
                  src={getAssetUrl(post.imageUrl)}
                  alt={post.title}
                  className="w-full h-full object-cover"
                />
              </div>
            )}

            <div className={compact ? 'p-4' : 'p-6'}>
              {/* Author Info */}
              {showAuthor && (
                <div className="flex items-center gap-3 mb-4">
                  {post.authorProfileSlug ? (
                    <Link href={`/trainer/${post.authorProfileSlug}`} className="flex items-center gap-3 hover:opacity-80 transition-opacity group">
                      <Avatar className="h-10 w-10 ring-2 ring-primary/30 group-hover:ring-primary/50 transition-all">
                        <AvatarImage src={getAssetUrl(post.authorProfilePictureUrl)} />
                        <AvatarFallback className="bg-primary/20 text-primary font-bold">
                          {(post.authorName || 'A').charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-semibold text-sm group-hover:text-primary transition-colors">{post.authorName || 'Autor'}</p>
                        {post.publishedAt && (
                          <p className="text-xs text-muted-foreground flex items-center gap-1">
                            <Calendar className="h-3 w-3" />
                            {new Date(post.publishedAt).toLocaleDateString('pt-BR', {
                              day: 'numeric',
                              month: 'long',
                              year: 'numeric',
                            })}
                          </p>
                        )}
                      </div>
                    </Link>
                  ) : (
                    <>
                      <Avatar className="h-10 w-10 ring-2 ring-primary/30">
                        <AvatarImage src={getAssetUrl(post.authorProfilePictureUrl)} />
                        <AvatarFallback className="bg-primary/20 text-primary font-bold">
                          {(post.authorName || 'A').charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-semibold text-sm">{post.authorName || 'Autor'}</p>
                        {post.publishedAt && (
                          <p className="text-xs text-muted-foreground flex items-center gap-1">
                            <Calendar className="h-3 w-3" />
                            {new Date(post.publishedAt).toLocaleDateString('pt-BR', {
                              day: 'numeric',
                              month: 'long',
                              year: 'numeric',
                            })}
                          </p>
                        )}
                      </div>
                    </>
                  )}
                </div>
              )}

              {/* Title */}
              <h3 className={`font-bold mb-3 ${compact ? 'text-lg' : 'text-xl'}`}>
                {post.title}
              </h3>

              {/* Date (if author not shown) */}
              {!showAuthor && post.publishedAt && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-3">
                  <Calendar className="h-4 w-4" />
                  {new Date(post.publishedAt).toLocaleDateString('pt-BR', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric',
                  })}
                </div>
              )}

              {/* Content with Markdown */}
              <div
                className={`prose prose-sm dark:prose-invert max-w-none mb-4 ${
                  !isExpanded && needsReadMore ? 'line-clamp-6' : ''
                }`}
              >
                <ReactMarkdown remarkPlugins={[remarkGfm]}>
                  {isExpanded || !needsReadMore ? content : contentPreview}
                </ReactMarkdown>
              </div>

              {/* Read More/Less Button */}
              {needsReadMore && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => togglePost(post.id)}
                  className="text-primary hover:text-primary/80 hover:bg-primary/10"
                >
                  {isExpanded ? 'Ver menos' : 'Ler mais'}
                </Button>
              )}
            </div>
          </Card>
        );
      })}
    </div>
  );
}
