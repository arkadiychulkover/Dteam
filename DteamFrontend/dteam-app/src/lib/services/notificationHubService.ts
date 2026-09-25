import * as signalR from '@microsoft/signalr';
import { authStore } from '../stores/authStore';
import { get } from 'svelte/store';
import type { AppNotification } from '../types/notification';
import { BACKEND_URL } from '../utils/constants';

type NotificationHandler = (notification: AppNotification) => void;

class NotificationHubService {
  private connection: signalR.HubConnection | null = null;
  private isStarting = false;
  private notificationHandlers = new Set<NotificationHandler>();

  public onReceiveNotification(handler: NotificationHandler) {
    this.notificationHandlers.add(handler);
    return () => this.notificationHandlers.delete(handler);
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
        const hubUrl = `${BACKEND_URL ? BACKEND_URL.replace(/\/+$/, '') : ''}/hubs/notifications`;

        this.connection = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl, {
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

        this.connection.on('ReceiveNotification', (notification: AppNotification) => {
          this.notificationHandlers.forEach(h => h(notification));
        });

        this.connection.onclose((err) => {
          if (err) {
            console.warn('[NotificationHub] Connection closed with error:', err);
          }
        });
      }

      await this.connection.start();
      console.log('[NotificationHub] Connected successfully');
    } catch (err) {
      console.warn('[NotificationHub] Start failed:', err);
    } finally {
      this.isStarting = false;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch {
      } finally {
        this.connection = null;
      }
    }
  }
}

export const notificationHubService = new NotificationHubService();
