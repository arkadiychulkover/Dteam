import { writable, get } from 'svelte/store';
import { api } from './api';
import type { Duser, Game } from '../types';

export interface CreateUserDto {
  email: string;
  username: string;
  password: string;
  walletAddress?: string;
  isAdmin?: boolean;
  balanceInNanoTons?: number;
  status?: number;
  avatarUrl?: string;
  bio?: string;
}

export interface UpdateUserDto {
  email?: string;
  username?: string;
  password?: string;
  walletAddress?: string;
  isAdmin?: boolean;
  isBanned?: boolean;
  status?: number;
  balanceInNanoTons?: number;
  avatarUrl?: string;
  bio?: string;
}

export interface CreateGameDto {
  title: string;
  description: string;
  shortDescription?: string;
  priceInNanoTons?: number;
  discountPercentage?: number;
  serverArchivePath: string;
  ownerId?: string;
  isDlc?: boolean;
  parentGameId?: string;
  version?: string;
  sizeInBytes?: number;
  isPublished?: boolean;
  genres?: string[];
  platforms?: string[];
  features?: string[];
  tags?: string[];
  headerImageUrl?: string;
  coverImageUrl?: string;
  screenshotUrls?: string[];
  trailerUrl?: string;
}

export interface UpdateGameDto {
  title?: string;
  description?: string;
  shortDescription?: string;
  priceInNanoTons?: number;
  discountPercentage?: number;
  serverArchivePath?: string;
  ownerId?: string;
  isDlc?: boolean;
  parentGameId?: string;
  version?: string;
  sizeInBytes?: number;
  isPublished?: boolean;
  genres?: string[];
  platforms?: string[];
  features?: string[];
  tags?: string[];
  headerImageUrl?: string;
  coverImageUrl?: string;
  screenshotUrls?: string[];
  trailerUrl?: string;
}

export interface HealthCheckResponse {
  status: string;
  timestamp: string;
  adminId?: string;
  totalUsers?: number;
  totalGames?: number;
  error?: string;
}

export const activeAdminIdStore = writable<string>(
  typeof localStorage !== 'undefined'
    ? localStorage.getItem('dteam_admin_guid') || ''
    : ''
);

class AdminService {
  public getActiveAdminId(): string {
    return get(activeAdminIdStore) || '';
  }

  public setActiveAdminId(id: string) {
    activeAdminIdStore.set(id);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('dteam_admin_guid', id);
    }
  }

  public async checkHealth(): Promise<HealthCheckResponse> {
    const res = await api.get<HealthCheckResponse>('/health/check-status');
    if (res.adminId && !get(activeAdminIdStore)) {
      this.setActiveAdminId(res.adminId);
    }
    return res;
  }

  public async autoDiscoverAdminGuid(): Promise<string> {
    try {
      const health = await this.checkHealth();
      if (health.adminId) {
        this.setActiveAdminId(health.adminId);
        return health.adminId;
      }
    } catch (e) {
      console.warn('[AdminService] Health check auto-discovery warning:', e);
    }

    return this.getActiveAdminId();
  }

  public async getUsers(customAdminId?: string): Promise<Duser[]> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.get<Duser[]>(`/admin/users?adminId=${adminId}`);
  }

  public async getUserById(id: string, customAdminId?: string): Promise<Duser> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.get<Duser>(`/admin/users/${id}?adminId=${adminId}`);
  }

  public async createUser(dto: CreateUserDto, customAdminId?: string): Promise<Duser> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.post<Duser>(`/admin/users?adminId=${adminId}`, dto);
  }

  public async updateUser(id: string, dto: UpdateUserDto, customAdminId?: string): Promise<Duser> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.put<Duser>(`/admin/users/${id}?adminId=${adminId}`, dto);
  }

  public async deleteUser(id: string, customAdminId?: string): Promise<{ message: string }> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.delete<{ message: string }>(`/admin/users/${id}?adminId=${adminId}`);
  }

  public async getGames(customAdminId?: string): Promise<Game[]> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.get<Game[]>(`/admin/games?adminId=${adminId}`);
  }

  public async getGameById(id: string, customAdminId?: string): Promise<Game> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.get<Game>(`/admin/games/${id}?adminId=${adminId}`);
  }

  public async createGame(dto: CreateGameDto, customAdminId?: string): Promise<Game> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.post<Game>(`/admin/games?adminId=${adminId}`, dto);
  }

  public async updateGame(id: string, dto: UpdateGameDto, customAdminId?: string): Promise<Game> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.put<Game>(`/admin/games/${id}?adminId=${adminId}`, dto);
  }

  public async deleteGame(id: string, customAdminId?: string): Promise<{ message: string }> {
    let adminId = customAdminId || this.getActiveAdminId();
    if (!adminId) {
      adminId = await this.autoDiscoverAdminGuid();
    }
    return await api.delete<{ message: string }>(`/admin/games/${id}?adminId=${adminId}`);
  }
}

export const adminService = new AdminService();
