import * as signalR from '@microsoft/signalr';
import { BACKEND_URL } from '../utils/constants';
import { tokenStorage } from './tokenStorage';
import type { BackendChatMessage } from './chatService';

export type MessageHandler = (msg: BackendChatMessage) => void;
export type ReadHandler = (data: { messageId: string; readerUserId: string; readAt: string }) => void;
export type TypingHandler = (data: { userId: string; isTyping: boolean }) => void;
export type HistoryClearedHandler = (data: { clearedWithUserId: string }) => void;
export type MessageDeletedHandler = (data: { messageId: string; peerId: string }) => void;
export type ReconnectedHandler = () => void;

class ChatHubService {
  private connection: signalR.HubConnection | null = null;
  private isStarting = false;

  private messageHandlers = new Set<MessageHandler>();
  private readHandlers = new Set<ReadHandler>();
  private typingHandlers = new Set<TypingHandler>();
  private historyClearedHandlers = new Set<HistoryClearedHandler>();
  private messageDeletedHandlers = new Set<MessageDeletedHandler>();
  private reconnectedHandlers = new Set<ReconnectedHandler>();

  public onReceiveMessage(handler: MessageHandler) {
    this.messageHandlers.add(handler);
    return () => this.messageHandlers.delete(handler);
  }

  public onMessageRead(handler: ReadHandler) {
    this.readHandlers.add(handler);
    return () => this.readHandlers.delete(handler);
  }

  public onUserTyping(handler: TypingHandler) {
    this.typingHandlers.add(handler);
    return () => this.typingHandlers.delete(handler);
  }

  public onHistoryCleared(handler: HistoryClearedHandler) {
    this.historyClearedHandlers.add(handler);
    return () => this.historyClearedHandlers.delete(handler);
  }

  public onMessageDeleted(handler: MessageDeletedHandler) {
    this.messageDeletedHandlers.add(handler);
    return () => this.messageDeletedHandlers.delete(handler);
  }

  public onReconnected(handler: ReconnectedHandler) {
    this.reconnectedHandlers.add(handler);
    return () => this.reconnectedHandlers.delete(handler);
  }

  public async start(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.isStarting) {
      return;
    }

    const token = tokenStorage.getSyncToken() || (await tokenStorage.getToken());
    if (!token) {
      console.log('[ChatHubService] Cannot start hub: No auth token present.');
      return;
    }

    this.isStarting = true;

    try {
      if (!this.connection) {
        const hubUrl = `${BACKEND_URL.replace(/\/+$/, '')}/hubs/chat`;
        console.log('[ChatHubService] Initializing connection to:', hubUrl);

        this.connection = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl, {
            accessTokenFactory: async () => {
              const currentToken = tokenStorage.getSyncToken() || (await tokenStorage.getToken());
              return currentToken || '';
            },
            skipNegotiation: false,
            transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
          })
          .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        this.connection.on('ReceiveMessage', (msg: BackendChatMessage) => {
          console.log('[ChatHubService] 📩 Received real-time message:', msg?.id, msg?.content);
          this.messageHandlers.forEach((h) => {
            try {
              h(msg);
            } catch (err) {
              console.error('[ChatHubService] Error in message handler:', err);
            }
          });
        });

        this.connection.on(
          'MessageRead',
          (data: { messageId: string; readerUserId: string; readAt: string }) => {
            console.log('[ChatHubService] 👁️ MessageRead:', data);
            this.readHandlers.forEach((h) => {
              try {
                h(data);
              } catch (err) {
                console.error('[ChatHubService] Error in read handler:', err);
              }
            });
          }
        );

        this.connection.on('UserTyping', (data: { userId: string; isTyping: boolean }) => {
          this.typingHandlers.forEach((h) => {
            try {
              h(data);
            } catch (err) {
              console.error('[ChatHubService] Error in typing handler:', err);
            }
          });
        });

        this.connection.on('HistoryCleared', (data: { clearedWithUserId: string }) => {
          console.log('[ChatHubService] 🧹 HistoryCleared:', data);
          this.historyClearedHandlers.forEach((h) => {
            try {
              h(data);
            } catch (err) {
              console.error('[ChatHubService] Error in history cleared handler:', err);
            }
          });
        });

        this.connection.on('MessageDeleted', (data: { messageId: string; peerId: string }) => {
          console.log('[ChatHubService] 🗑️ MessageDeleted:', data);
          this.messageDeletedHandlers.forEach((h) => {
            try {
              h(data);
            } catch (err) {
              console.error('[ChatHubService] Error in message deleted handler:', err);
            }
          });
        });

        this.connection.onreconnected(() => {
          console.log('[ChatHubService] 🔄 Reconnected to ChatHub');
          this.reconnectedHandlers.forEach((h) => {
            try {
              h();
            } catch (err) {
              console.error('[ChatHubService] Error in reconnected handler:', err);
            }
          });
        });

        this.connection.onclose((err) => {
          console.log('[ChatHubService] Connection closed:', err);
        });
      }

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        await this.connection.start();
        console.log('[ChatHubService] ✅ Connected to ChatHub successfully!');
      }
    } catch (err) {
      console.warn('[ChatHubService] ❌ Failed to start ChatHub:', err);
    } finally {
      this.isStarting = false;
    }
  }

  public clearHandlers(): void {
    this.messageHandlers.clear();
    this.readHandlers.clear();
    this.typingHandlers.clear();
    this.historyClearedHandlers.clear();
    this.messageDeletedHandlers.clear();
    this.reconnectedHandlers.clear();
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        console.log('[ChatHubService] Stopping ChatHub connection...');
        await this.connection.stop();
      } catch (err) {
        console.warn('[ChatHubService] Error stopping ChatHub:', err);
      }
      this.connection = null;
    }
    this.clearHandlers();
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const chatHubService = new ChatHubService();
