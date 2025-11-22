'use client';

import { useState, useEffect, useRef } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useChat } from '@/hooks/use-chat';
import { apiClient } from '@/lib/api';
import { getAssetUrl } from '@/lib/env';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  MessageCircle,
  Send,
  ArrowLeft,
  Loader2,
  Paperclip,
  Image as ImageIcon,
} from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderProfilePictureUrl?: string;
  content: string;
  messageType: string;
  fileUrl?: string;
  fileName?: string;
  sentAt: string;
  readAt?: string;
  isEdited: boolean;
  isDeleted: boolean;
}

interface Conversation {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserProfilePictureUrl?: string;
  otherUserRole?: string;
  lastMessageAt: string;
  lastMessagePreview?: string;
  lastMessageSenderId?: string;
  unreadCount: number;
  isArchived: boolean;
}

interface ChatDrawerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialUserId?: string;
}

export function ChatDrawer({ open, onOpenChange, initialUserId }: ChatDrawerProps) {
  const { user } = useAuth();
  const { toast } = useToast();
  const {
    isConnected,
    conversations,
    setConversations,
    totalUnreadCount,
    setTotalUnreadCount,
    typingUsers,
    sendTypingIndicator,
    joinConversation,
    leaveConversation,
  } = useChat();

  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [messageInput, setMessageInput] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
  const [isLoadingConversations, setIsLoadingConversations] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const typingTimeoutRef = useRef<NodeJS.Timeout>();

  // Fetch conversations
  const fetchConversations = async () => {
    try {
      setIsLoadingConversations(true);
      const data = await apiClient.get<Conversation[]>('/chat/conversations');
      setConversations(data);

      // Calculate total unread
      const total = data.reduce((sum, conv) => sum + conv.unreadCount, 0);
      setTotalUnreadCount(total);
    } catch (error: any) {
      console.error('Error fetching conversations:', error);
    } finally {
      setIsLoadingConversations(false);
    }
  };

  // Fetch or create conversation with user
  const openConversationWithUser = async (userId: string) => {
    try {
      setIsLoadingMessages(true);
      const data = await apiClient.get<any>(`/chat/conversations/with/${userId}`);

      const conversation: Conversation = {
        id: data.id,
        otherUserId: data.otherUserId,
        otherUserName: data.otherUserName,
        otherUserProfilePictureUrl: data.otherUserProfilePictureUrl,
        otherUserRole: data.otherUserRole,
        lastMessageAt: new Date().toISOString(),
        lastMessagePreview: '',
        unreadCount: 0,
        isArchived: false,
      };

      setSelectedConversation(conversation);
      setMessages(data.messages || []);
      joinConversation(data.id);

      // Mark as read
      if (data.messages?.some((m: Message) => m.senderId !== user?.id && !m.readAt)) {
        await markAsRead(data.id);
      }
    } catch (error: any) {
      toast({
        title: 'Erro ao abrir conversa',
        description: error.message || 'Não foi possível carregar a conversa.',
        variant: 'destructive',
      });
    } finally {
      setIsLoadingMessages(false);
    }
  };

  // Load conversations when drawer opens
  useEffect(() => {
    if (open) {
      fetchConversations();

      if (initialUserId) {
        openConversationWithUser(initialUserId);
      }
    }
  }, [open, initialUserId]);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Handle conversation selection
  const handleSelectConversation = async (conversation: Conversation) => {
    setSelectedConversation(conversation);
    setIsLoadingMessages(true);

    try {
      const data = await apiClient.get<any>(`/chat/conversations/with/${conversation.otherUserId}`);
      setMessages(data.messages || []);
      joinConversation(conversation.id);

      // Mark as read
      await markAsRead(conversation.id);
    } catch (error: any) {
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar as mensagens.',
        variant: 'destructive',
      });
    } finally {
      setIsLoadingMessages(false);
    }
  };

  // Mark messages as read
  const markAsRead = async (conversationId: string) => {
    try {
      await apiClient.post(`/chat/conversations/${conversationId}/mark-read`, {});

      // Update conversations
      setConversations(prev =>
        prev.map(c => (c.id === conversationId ? { ...c, unreadCount: 0 } : c))
      );

      // Update total unread count
      setTotalUnreadCount(prev => Math.max(0, prev - (selectedConversation?.unreadCount || 0)));
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  // Send message
  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!messageInput.trim() || !selectedConversation || isSending) return;

    setIsSending(true);

    try {
      const response = await apiClient.post<Message>('/chat/messages', {
        recipientId: selectedConversation.otherUserId,
        content: messageInput.trim(),
        messageType: 'Text',
      });

      setMessages(prev => [...prev, response]);
      setMessageInput('');

      // Update conversation in list
      setConversations(prev => {
        const updated = prev.map(c =>
          c.id === selectedConversation.id
            ? {
                ...c,
                lastMessageAt: response.sentAt,
                lastMessagePreview: response.content,
                lastMessageSenderId: user?.id,
              }
            : c
        );
        // Move to top
        const index = updated.findIndex(c => c.id === selectedConversation.id);
        if (index > 0) {
          const [conv] = updated.splice(index, 1);
          updated.unshift(conv);
        }
        return updated;
      });
    } catch (error: any) {
      toast({
        title: 'Erro ao enviar mensagem',
        description: error.message || 'Não foi possível enviar a mensagem.',
        variant: 'destructive',
      });
    } finally {
      setIsSending(false);
    }
  };

  // Handle typing indicator
  const handleTyping = () => {
    if (!selectedConversation) return;

    sendTypingIndicator(selectedConversation.id, selectedConversation.otherUserId, true);

    // Clear previous timeout
    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    // Set timeout to stop typing indicator
    typingTimeoutRef.current = setTimeout(() => {
      sendTypingIndicator(selectedConversation.id, selectedConversation.otherUserId, false);
    }, 3000);
  };

  // Back to conversations
  const handleBack = () => {
    if (selectedConversation) {
      leaveConversation(selectedConversation.id);
    }
    setSelectedConversation(null);
    setMessages([]);
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="w-full sm:max-w-md p-0 flex flex-col">
        <SheetHeader className="p-4 border-b">
          <div className="flex items-center gap-2">
            {selectedConversation && (
              <Button variant="ghost" size="icon" onClick={handleBack}>
                <ArrowLeft className="h-5 w-5" />
              </Button>
            )}
            <MessageCircle className="h-5 w-5" />
            <SheetTitle>
              {selectedConversation ? selectedConversation.otherUserName : 'Mensagens'}
            </SheetTitle>
            {!selectedConversation && totalUnreadCount > 0 && (
              <Badge variant="destructive" className="ml-auto">
                {totalUnreadCount}
              </Badge>
            )}
            {isConnected && !selectedConversation && (
              <div className="ml-auto flex items-center gap-1 text-xs text-green-600">
                <div className="h-2 w-2 rounded-full bg-green-600" />
                Online
              </div>
            )}
          </div>
        </SheetHeader>

        {!selectedConversation ? (
          // Conversation List
          <ScrollArea className="flex-1">
            {isLoadingConversations ? (
              <div className="flex items-center justify-center py-12">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : conversations.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
                <MessageCircle className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground">Nenhuma conversa ainda</p>
                <p className="text-sm text-muted-foreground mt-1">
                  Inicie uma conversa com um personal trainer ou aluno
                </p>
              </div>
            ) : (
              <div className="divide-y">
                {conversations.map(conversation => (
                  <button
                    key={conversation.id}
                    onClick={() => handleSelectConversation(conversation)}
                    className="w-full p-4 hover:bg-accent transition-colors text-left"
                  >
                    <div className="flex items-start gap-3">
                      <Avatar className="h-12 w-12">
                        <AvatarImage src={getAssetUrl(conversation.otherUserProfilePictureUrl)} />
                        <AvatarFallback>
                          {conversation.otherUserName.charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between mb-1">
                          <span className="font-semibold truncate">
                            {conversation.otherUserName}
                          </span>
                          {conversation.lastMessageAt && (
                            <span className="text-xs text-muted-foreground">
                              {formatDistanceToNow(new Date(conversation.lastMessageAt), {
                                addSuffix: true,
                                locale: ptBR,
                              })}
                            </span>
                          )}
                        </div>
                        <div className="flex items-center justify-between">
                          <p className="text-sm text-muted-foreground truncate">
                            {conversation.lastMessagePreview || 'Sem mensagens'}
                          </p>
                          {conversation.unreadCount > 0 && (
                            <Badge variant="destructive" className="ml-2">
                              {conversation.unreadCount}
                            </Badge>
                          )}
                        </div>
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </ScrollArea>
        ) : (
          // Message Thread
          <>
            <ScrollArea className="flex-1 p-4">
              {isLoadingMessages ? (
                <div className="flex items-center justify-center py-12">
                  <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                </div>
              ) : messages.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <MessageCircle className="h-12 w-12 text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">Nenhuma mensagem ainda</p>
                  <p className="text-sm text-muted-foreground mt-1">
                    Envie a primeira mensagem!
                  </p>
                </div>
              ) : (
                <div className="space-y-4">
                  {messages.map(message => {
                    const isOwnMessage = message.senderId === user?.id;
                    return (
                      <div
                        key={message.id}
                        className={`flex ${isOwnMessage ? 'justify-end' : 'justify-start'}`}
                      >
                        <div
                          className={`max-w-[75%] rounded-lg p-3 ${
                            isOwnMessage
                              ? 'bg-primary text-primary-foreground'
                              : 'bg-muted'
                          }`}
                        >
                          <p className="text-sm whitespace-pre-wrap break-words">
                            {message.content}
                          </p>
                          <div className="flex items-center gap-2 mt-1">
                            <span className="text-xs opacity-70">
                              {new Date(message.sentAt).toLocaleTimeString('pt-BR', {
                                hour: '2-digit',
                                minute: '2-digit',
                              })}
                            </span>
                            {message.isEdited && (
                              <span className="text-xs opacity-70">(editado)</span>
                            )}
                            {isOwnMessage && message.readAt && (
                              <span className="text-xs opacity-70">✓✓</span>
                            )}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                  {typingUsers.has(selectedConversation.id) && (
                    <div className="flex justify-start">
                      <div className="bg-muted rounded-lg p-3">
                        <div className="flex items-center gap-1">
                          <div className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce" />
                          <div
                            className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce"
                            style={{ animationDelay: '0.2s' }}
                          />
                          <div
                            className="w-2 h-2 bg-muted-foreground rounded-full animate-bounce"
                            style={{ animationDelay: '0.4s' }}
                          />
                        </div>
                      </div>
                    </div>
                  )}
                  <div ref={messagesEndRef} />
                </div>
              )}
            </ScrollArea>

            {/* Message Input */}
            <form onSubmit={handleSendMessage} className="p-4 border-t">
              <div className="flex items-center gap-2">
                <Input
                  value={messageInput}
                  onChange={e => {
                    setMessageInput(e.target.value);
                    handleTyping();
                  }}
                  placeholder="Digite uma mensagem..."
                  disabled={isSending}
                  className="flex-1"
                />
                <Button type="submit" size="icon" disabled={isSending || !messageInput.trim()}>
                  {isSending ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Send className="h-4 w-4" />
                  )}
                </Button>
              </div>
            </form>
          </>
        )}
      </SheetContent>
    </Sheet>
  );
}
