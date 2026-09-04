import * as signalR from '@microsoft/signalr';
import { authStore } from '../stores/authStore';
import { friendsStore } from '../stores/friendsStore';
import { uiStore } from '../stores/uiStore';
import { get } from 'svelte/store';

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

    const state = get(authStore);
    if (!state.token) {
      return;
    }

    this.isStarting = true;

    try {
      if (!this.connection) {
        this.connection = new signalR.HubConnectionBuilder()
          .withUrl('/hubs/friends', {
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

        this.connection.on('OnlineFriendsList', (userIds: string[]) => {
          friendsStore.setInitialOnlineFriends(userIds);
        });

        this.connection.on('UserOnline', (userId: string) => {
          friendsStore.setUserOnline(userId);
        });

        this.connection.on('UserOffline', (userId: string) => {
          friendsStore.setUserOffline(userId);
        });

        this.connection.on('UserStatusChanged', (data: { userId: string; status: string }) => {
          if (data.status === 'Online') {
            friendsStore.setUserOnline(data.userId);
          } else {
            friendsStore.setUserOffline(data.userId);
          }
        });

        // 1. Подія надсилання заявки у друзі (отримувач підтягує заявки)
        this.connection.on('FriendRequestReceived', (data?: { senderUsername?: string }) => {
          friendsStore.loadRequests();
          if (data?.senderUsername) {
            uiStore.addToast({
              title: 'Нова заявка у друзі',
              message: `${data.senderUsername} надіслав(ла) вам запит у друзі!`,
              type: 'info'
            });
          }
        });

        // 2. Подія прийняття заявки (обидві сторони оновлюють друзів та заявки)
        this.connection.on('FriendRequestAccepted', () => {
          friendsStore.loadAll();
        });

        this.connection.onreconnected(() => {
          friendsStore.loadAll();
        });
      }

      if (this.connection.state === signalR.HubConnectionState.Disconnected) {
        await this.connection.start();
      }
    } catch (err) {
      console.warn('FriendsHub connection error:', err);
    } finally {
      this.isStarting = false;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.warn('FriendsHub stop error:', err);
      }
      this.connection = null;
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const friendsHubService = new FriendsHubService();
