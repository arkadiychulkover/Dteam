import * as signalR from '@microsoft/signalr';
import { BACKEND_URL } from '../utils/constants';
import { tokenStorage } from './tokenStorage';
import { usePresenceStore } from '../store/usePresenceStore';

class FriendsHubService {
  private connection: signalR.HubConnection | null = null;
  private isStarting = false;

  public async start(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.isStarting) {
      return;
    }

    const token = tokenStorage.getSyncToken() || (await tokenStorage.getToken());
    if (!token) {
      console.log('[FriendsHubService] Cannot start hub: No auth token present.');
      return;
    }

    this.isStarting = true;

    try {
      if (!this.connection) {
        const hubUrl = `${BACKEND_URL.replace(/\/+$/, '')}/hubs/friends`;
        console.log('[FriendsHubService] Initializing connection to:', hubUrl);

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

        // 1. Initial list of online friends from server
        this.connection.on('OnlineFriendsList', (userIds: string[]) => {
          console.log('[FriendsHubService] 🟢 OnlineFriendsList received:', userIds);
          usePresenceStore.getState().setInitialOnlineFriends(userIds);
        });

        // 2. A friend became online
        this.connection.on('UserOnline', (userId: string) => {
          console.log('[FriendsHubService] 🟢 UserOnline:', userId);
          usePresenceStore.getState().setUserOnline(userId);
        });

        // 3. A friend became offline
        this.connection.on('UserOffline', (userId: string) => {
          console.log('[FriendsHubService] 🔴 UserOffline:', userId);
          usePresenceStore.getState().setUserOffline(userId);
        });

        // 4. Friend status changed (e.g. Online vs Offline)
        this.connection.on('UserStatusChanged', (data: { userId: string; status: string }) => {
          console.log('[FriendsHubService] 🔄 UserStatusChanged:', data);
          if (data.status === 'Online') {
            usePresenceStore.getState().setUserOnline(data.userId);
          } else {
            usePresenceStore.getState().setUserOffline(data.userId);
          }
        });

        this.connection.onreconnected(() => {
          console.log('[FriendsHubService] 🔄 Reconnected to FriendsHub');
        });

        this.connection.onclose((err) => {
          console.log('[FriendsHubService] Connection closed:', err);
        });
      }

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        await this.connection.start();
        console.log('[FriendsHubService] ✅ Connected to FriendsHub successfully!');
      }
    } catch (err) {
      console.warn('[FriendsHubService] ❌ Failed to start FriendsHub:', err);
    } finally {
      this.isStarting = false;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        console.log('[FriendsHubService] Stopping FriendsHub connection...');
        await this.connection.stop();
      } catch (err) {
        console.warn('[FriendsHubService] Error stopping FriendsHub:', err);
      }
      this.connection = null;
      usePresenceStore.getState().clear();
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const friendsHubService = new FriendsHubService();
