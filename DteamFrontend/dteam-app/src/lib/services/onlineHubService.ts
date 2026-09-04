import * as signalR from '@microsoft/signalr';
import { uiStore } from '../stores/uiStore';
import { api } from './api';

type OnlineCountCallback = (count: number) => void;
type RewardMintedCallback = (reward: any) => void;

class OnlineHubService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Set<OnlineCountCallback> = new Set();
  private rewardListeners: Set<RewardMintedCallback> = new Set();
  private currentCount: number = 0;
  private heartbeatTimer: any = null;

  constructor() {
    // ⚠️ Использование относительного пути активирует Vite Proxy
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/online', {
        accessTokenFactory: async () => (await api.getValidToken()) || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('UpdateOnlineCount', (count: number) => {
      console.log('[OnlineHub] Новый онлайн count:', count);
      this.currentCount = count;
      this.notify();
    });

    this.connection.on('NftRewardMinted', (reward: any) => {
      console.log('[OnlineHub] 🎉 Отримано нагороду NFT за перебування на сайті:', reward);
      const tokenStr = reward?.tokenId !== undefined ? `#${String(reward.tokenId).padStart(3, '0')}` : '';
      uiStore.addToast({
        title: 'Нагорода за активність!',
        message: `Вам нараховано новий значок ${tokenStr} за час, проведений на сайті.`,
        type: 'success'
      });
      this.rewardListeners.forEach(cb => cb(reward));
    });

    this.connection.onreconnected(() => {
      console.log('🔄 [OnlineHub] З\'єднання відновлено, перезапуск heartbeat...');
      this.startHeartbeat();
    });

    // При оновленні токена перезапускаємо хаб з новим токеном
    api.onTokenRefreshed(async () => {
      console.log('[OnlineHub] Токен оновлено, перепідключаємо OnlineHub...');
      await this.restartConnection();
    });
  }

  async startConnection(): Promise<void> {
    if (!this.connection) return;

    if (this.connection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.start();
        console.log('✅ [OnlineHub] Соединение успешно установлено через Vite Proxy');
        this.startHeartbeat();
      } catch (err) {
        console.error('❌ [OnlineHub] Ошибка при подключении:', err);
      }
    }
  }

  // Переподключение потрібне, коли токен змінюється (логін/логаут):
  async restartConnection(): Promise<void> {
    if (!this.connection) return;
    try {
      this.stopHeartbeat();
      if (this.connection.state !== signalR.HubConnectionState.Disconnected) {
        await this.connection.stop();
      }
      await this.connection.start();
      console.log('🔄 [OnlineHub] Перепідключення з новим токеном виконано');
      this.startHeartbeat();
    } catch (err) {
      console.error('❌ [OnlineHub] Помилка при перепідключенні:', err);
    }
  }

  async stopConnection(): Promise<void> {
    this.stopHeartbeat();
    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      try {
        await this.connection.stop();
        console.log('🛑 [OnlineHub] Соединение разорвано');
      } catch (err) {
        console.error('Ошибка при остановке OnlineHub:', err);
      }
    }
  }

  private startHeartbeat() {
    this.stopHeartbeat();
    this.sendHeartbeat();
    this.heartbeatTimer = setInterval(() => this.sendHeartbeat(), 20000);
  }

  private stopHeartbeat() {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }
  }

  private async sendHeartbeat() {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) return;
    try {
      const token = await api.getValidToken();
      await this.connection.invoke('Heartbeat', token);
    } catch (e) {
      console.debug('[OnlineHub] Heartbeat notice:', e);
    }
  }

  onOnlineCountChanged(callback: OnlineCountCallback): () => void {
    this.listeners.add(callback);
    callback(this.currentCount);

    return () => {
      this.listeners.delete(callback);
    };
  }

  onRewardMinted(callback: RewardMintedCallback): () => void {
    this.rewardListeners.add(callback);
    return () => {
      this.rewardListeners.delete(callback);
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