import * as signalR from '@microsoft/signalr';
import { authStore } from '../stores/authStore';
import { get } from 'svelte/store';
import type { ChatMessage } from '../types/chat';

type MessageHandler = (msg: ChatMessage) => void;
type ReadHandler = (data: { messageId: string; readerUserId: string; readAt: string }) => void;
type TypingHandler = (data: { userId: string; isTyping: boolean }) => void;
type HistoryClearedHandler = (data: { clearedWithUserId: string }) => void;
type ReconnectedHandler = () => void;

class ChatHubService {
  private connection: signalR.HubConnection | null = null;
  private isStarting = false;

  private messageHandlers = new Set<MessageHandler>();
  private readHandlers = new Set<ReadHandler>();
  private typingHandlers = new Set<TypingHandler>();
  private historyClearedHandlers = new Set<HistoryClearedHandler>();
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

    const state = get(authStore);
    if (!state.token) {
      return;
    }

    this.isStarting = true;

    try {
      if (!this.connection) {
        this.connection = new signalR.HubConnectionBuilder()
          .withUrl('/hubs/chat', {
            accessTokenFactory: () => {
              const current = get(authStore);
              return current.token || localStorage.getItem('dteam_token') || '';
            },
            skipNegotiation: false,
            transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
          })
          .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        this.connection.on('ReceiveMessage', (msg: ChatMessage) => {
          this.messageHandlers.forEach(h => h(msg));
        });

        this.connection.on('MessageRead', (data: { messageId: string; readerUserId: string; readAt: string }) => {
          this.readHandlers.forEach(h => h(data));
        });

        this.connection.on('UserTyping', (data: { userId: string; isTyping: boolean }) => {
          this.typingHandlers.forEach(h => h(data));
        });

        this.connection.on('HistoryCleared', (data: { clearedWithUserId: string }) => {
          this.historyClearedHandlers.forEach(h => h(data));
        });

        this.connection.onreconnected(() => {
          this.reconnectedHandlers.forEach(h => h());
        });
      }

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        await this.connection.start();
      }
    } catch (err) {
      console.warn('ChatHub connection error:', err);
    } finally {
      this.isStarting = false;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.warn('ChatHub stop error:', err);
      }
      this.connection = null;
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const chatHubService = new ChatHubService();
