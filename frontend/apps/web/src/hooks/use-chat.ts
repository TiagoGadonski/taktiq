import { useEffect, useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './use-auth';
import { env } from '@/lib/env';
import { tokenStorage } from '@/lib/api';

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
  editedAt?: string;
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

interface MessageNotification {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderProfilePictureUrl?: string;
  content: string;
  messageType: string;
  sentAt: string;
}

interface TypingIndicator {
  conversationId: string;
  userId: string;
  userName: string;
  isTyping: boolean;
}

export function useChat() {
  const { isAuthenticated } = useAuth();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [totalUnreadCount, setTotalUnreadCount] = useState(0);
  const [typingUsers, setTypingUsers] = useState<Map<string, string>>(new Map());

  // Initialize SignalR connection
  useEffect(() => {
    const token = tokenStorage.getAccessTokenSync();
    if (!isAuthenticated || !token) {
      return;
    }

    let retryAttempts = 0;

    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${env.apiHost}/hubs/chat`, {
        accessTokenFactory: () => tokenStorage.getAccessTokenSync() || '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => {
          // Retry after 0, 2, 10, 30 seconds, then every 30 seconds
          const delay = Math.min(1000 * (2 ** retryAttempts), 30000);
          retryAttempts++;
          return delay;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    hubConnection.on('ReceiveMessage', (notification: MessageNotification) => {
      // Handle incoming message
      console.log('New message received:', notification);

      // Update conversation list
      setConversations(prev => {
        const conversationIndex = prev.findIndex(c => c.id === notification.conversationId);
        if (conversationIndex >= 0) {
          const updated = [...prev];
          updated[conversationIndex] = {
            ...updated[conversationIndex],
            lastMessageAt: notification.sentAt,
            lastMessagePreview: notification.content,
            lastMessageSenderId: notification.senderId,
            unreadCount: updated[conversationIndex].unreadCount + 1,
          };
          // Move to top
          const [conversation] = updated.splice(conversationIndex, 1);
          updated.unshift(conversation);
          return updated;
        }
        return prev;
      });

      // Update unread count
      setTotalUnreadCount(prev => prev + 1);
    });

    hubConnection.on('UserTyping', (indicator: TypingIndicator) => {
      setTypingUsers(prev => {
        const newMap = new Map(prev);
        if (indicator.isTyping) {
          newMap.set(indicator.conversationId, indicator.userName);
        } else {
          newMap.delete(indicator.conversationId);
        }
        return newMap;
      });
    });

    hubConnection.on('MessageRead', (data: { messageId: string; readByUserId: string; readAt: string }) => {
      console.log('Message marked as read:', data);
      // You can update the UI to show read status if needed
    });

    // Handle connection state changes
    hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      setIsConnected(false);
    });

    hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      setIsConnected(true);
    });

    hubConnection.onclose(() => {
      console.log('SignalR connection closed');
      setIsConnected(false);
    });

    // Start the connection
    hubConnection
      .start()
      .then(() => {
        console.log('SignalR connected');
        setIsConnected(true);
      })
      .catch(err => {
        console.error('SignalR connection error:', err);
        setIsConnected(false);
      });

    setConnection(hubConnection);

    // Cleanup
    return () => {
      hubConnection.stop();
    };
  }, [isAuthenticated]);

  // Send typing indicator
  const sendTypingIndicator = useCallback(
    (conversationId: string, recipientId: string, isTyping: boolean) => {
      if (connection && isConnected) {
        connection
          .invoke('SendTypingIndicator', conversationId, recipientId, isTyping)
          .catch(err => console.error('Error sending typing indicator:', err));
      }
    },
    [connection, isConnected]
  );

  // Join conversation (for real-time updates)
  const joinConversation = useCallback(
    (conversationId: string) => {
      if (connection && isConnected) {
        connection
          .invoke('JoinConversation', conversationId)
          .catch(err => console.error('Error joining conversation:', err));
      }
    },
    [connection, isConnected]
  );

  // Leave conversation
  const leaveConversation = useCallback(
    (conversationId: string) => {
      if (connection && isConnected) {
        connection
          .invoke('LeaveConversation', conversationId)
          .catch(err => console.error('Error leaving conversation:', err));
      }
    },
    [connection, isConnected]
  );

  return {
    connection,
    isConnected,
    conversations,
    setConversations,
    totalUnreadCount,
    setTotalUnreadCount,
    typingUsers,
    sendTypingIndicator,
    joinConversation,
    leaveConversation,
  };
}
