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
    return await api.get<HealthCheckResponse>('/health/check-status');
  }

  public async getUsers(_customAdminId?: string): Promise<Duser[]> {
    return await api.get<Duser[]>('/admin/users');
  }

  public async getUserById(id: string, _customAdminId?: string): Promise<Duser> {
    return await api.get<Duser>(`/admin/users/${id}`);
  }

  public async createUser(dto: CreateUserDto, _customAdminId?: string): Promise<Duser> {
    return await api.post<Duser>('/admin/users', dto);
  }

  public async updateUser(id: string, dto: UpdateUserDto, _customAdminId?: string): Promise<Duser> {
    return await api.put<Duser>(`/admin/users/${id}`, dto);
  }

  public async deleteUser(id: string, _customAdminId?: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/admin/users/${id}`);
  }

  public async getGames(_customAdminId?: string): Promise<Game[]> {
    return await api.get<Game[]>('/admin/games');
  }

  public async getGameById(id: string, _customAdminId?: string): Promise<Game> {
    return await api.get<Game>(`/admin/games/${id}`);
  }

  public async createGame(dto: CreateGameDto, _customAdminId?: string): Promise<Game> {
    return await api.post<Game>('/admin/games', dto);
  }

  public async updateGame(id: string, dto: UpdateGameDto, _customAdminId?: string): Promise<Game> {
    return await api.put<Game>(`/admin/games/${id}`, dto);
  }

  public async deleteGame(id: string, _customAdminId?: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/admin/games/${id}`);
  }
}

export const adminService = new AdminService();
