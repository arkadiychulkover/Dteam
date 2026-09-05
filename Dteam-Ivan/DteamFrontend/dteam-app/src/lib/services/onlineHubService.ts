import * as signalR from '@microsoft/signalr';

type OnlineCountCallback = (count: number) => void;

class OnlineHubService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Set<OnlineCountCallback> = new Set();
  private currentCount: number = 0;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/online', {
        accessTokenFactory: () => localStorage.getItem('dteam_token') || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('UpdateOnlineCount', (count: number) => {
      console.log('[OnlineHub] Новый онлайн count:', count);
      this.currentCount = count;
      this.notify();
    });
  }

  async startConnection(): Promise<void> {
    if (!this.connection) return;

    if (this.connection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.start();
        console.log('✅ [OnlineHub] Соединение успешно установлено через Vite Proxy');
      } catch (err) {
        console.error('❌ [OnlineHub] Ошибка при подключении:', err);
      }
    }
  }
  async restartConnection(): Promise<void> {
    if (!this.connection) return;
    try {
      if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
        await this.connection.stop();
      }
      await this.connection.start();
      console.log('🔄 [OnlineHub] Перепідключення з новим токеном виконано');
    } catch (err) {
      console.error('❌ [OnlineHub] Помилка при перепідключенні:', err);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.stop();
        console.log('🛑 [OnlineHub] Соединение разорвано');
      } catch (err) {
        console.error('Ошибка при остановке OnlineHub:', err);
      }
    }
  }

  onOnlineCountChanged(callback: OnlineCountCallback): () => void {
    this.listeners.add(callback);
    callback(this.currentCount);

    return () => {
      this.listeners.delete(callback);
    };
  }

  getOnlineCount(): number {
    return this.currentCount;
  }

  private notify(): void {
    this.listeners.forEach((callback) => callback(this.currentCount));
  }
}

export const onlineHubService = new OnlineHubService();