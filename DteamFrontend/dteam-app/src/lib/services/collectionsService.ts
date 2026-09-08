import { api } from './api';
import type {
  GameCollection,
  GameCollectionItem,
  CreateGameCollectionPayload,
  UpdateGameCollectionPayload,
} from '../types';

export function normalizeCollection(col: any): GameCollection {
  if (!col) {
    return {
      id: '',
      name: '',
      createdAt: new Date().toISOString(),
      items: [],
      games: [],
      gamesCount: 0,
    };
  }

  let items: GameCollectionItem[] = [];
  if (Array.isArray(col.items)) {
    items = col.items.map((i: any) => ({
      gameId: i.gameId || i.id,
      title: i.title || i.game?.title || '',
      coverImageUrl: i.coverImageUrl || i.game?.coverImageUrl || i.game?.headerImageUrl || null,
      addedAt: i.addedAt || col.createdAt || new Date().toISOString(),
    }));
  } else if (Array.isArray(col.games)) {
    items = col.games.map((g: any) => ({
      gameId: g.id || g.gameId,
      title: g.title || '',
      coverImageUrl: g.coverImageUrl || g.headerImageUrl || null,
      addedAt: col.createdAt || new Date().toISOString(),
    }));
  }

  return {
    id: col.id,
    userId: col.userId,
    name: col.name || '',
    description: col.description || null,
    createdAt: col.createdAt || new Date().toISOString(),
    updatedAt: col.updatedAt,
    gamesCount: typeof col.gamesCount === 'number' ? col.gamesCount : items.length,
    items,
    games: Array.isArray(col.games) ? col.games : [],
  };
}

export const collectionsService = {
  async getCollections(): Promise<GameCollection[]> {
    const list = await api.get<any[]>('/collections');
    return Array.isArray(list) ? list.map(normalizeCollection) : [];
  },

  async getCollection(id: string): Promise<GameCollection> {
    const col = await api.get<any>(`/collections/${id}`);
    return normalizeCollection(col);
  },

  async createCollection(payload: CreateGameCollectionPayload): Promise<GameCollection> {
    const body = {
      name: payload.name,
      description: payload.description,
      initialGameIds: payload.initialGameIds ?? payload.gameIds ?? [],
    };
    const res = await api.post<any>('/collections', body);
    return normalizeCollection(res);
  },

  async updateCollection(id: string, payload: UpdateGameCollectionPayload): Promise<GameCollection> {
    const res = await api.put<any>(`/collections/${id}`, payload);
    return normalizeCollection(res);
  },

  async deleteCollection(id: string): Promise<void> {
    await api.delete(`/collections/${id}`);
  },

  async addGameToCollection(collectionId: string, gameId: string): Promise<GameCollection> {
    const res = await api.post<any>(`/collections/${collectionId}/games`, { gameId });
    if (res && (res.id || Array.isArray(res.items))) {
      return normalizeCollection(res);
    }
    return await collectionsService.getCollection(collectionId);
  },

  async removeGameFromCollection(collectionId: string, gameId: string): Promise<GameCollection> {
    const res = await api.delete<any>(`/collections/${collectionId}/games/${gameId}`);
    if (res && (res.id || Array.isArray(res.items))) {
      return normalizeCollection(res);
    }
    return await collectionsService.getCollection(collectionId);
  },
};
