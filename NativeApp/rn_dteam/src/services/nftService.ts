import { api } from './api';
import { resolveMediaUrl } from '../utils/constants';
import type { Badge } from '../types';

export interface BackendNftItem {
  id: string;
  tokenId?: number | null;
  name: string;
  description?: string | null;
  giftMessage?: string | null;
  rarity: number; // 0=common, 1=uncommon/rare, 2=rare, 3=epic, 4=legendary
  imageUrl: string;
  contractAddress: string;
  ownerAddress?: string | null;
  isMinted: boolean;
  createdAt: string;
}

export function mapRarity(rarityNumber: number): Badge['rarity'] {
  switch (rarityNumber) {
    case 4:
      return 'mythic';
    case 3:
      return 'legendary';
    case 2:
      return 'epic';
    case 1:
      return 'rare';
    default:
      return 'common';
  }
}

export function mapBackendNftToBadge(item: BackendNftItem): Badge {
  return {
    id: item.id,
    title: item.name,
    description: item.description || item.giftMessage || 'Колекційний NFT значок платформи DTEAM',
    imageUrl: resolveMediaUrl(item.imageUrl) || item.imageUrl,
    iconName: 'sparkles',
    rarity: mapRarity(item.rarity),
    level: item.tokenId != null ? Math.min(5, Math.max(1, (item.tokenId % 5) + 1)) : 1,
    earnedDate: item.createdAt
      ? new Date(item.createdAt).toLocaleDateString('uk-UA', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
        })
      : '2026',
    contractAddress: item.contractAddress || '0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0',
    tokenId: item.tokenId != null ? String(item.tokenId).padStart(3, '0') : '001',
  };
}

export const nftService = {
  /**
   * Отримати NFT користувача з сервера (без використання блокчейну на клієнті)
   */
  async getUserNfts(userId: string): Promise<Badge[]> {
    if (!userId) return [];
    try {
      const items = await api.get<BackendNftItem[]>(`/nft/user/${userId}/gifts`);
      if (items && Array.isArray(items)) {
        return items.map(mapBackendNftToBadge);
      }
      return [];
    } catch (err) {
      console.warn('[nftService] Failed to fetch user gifts from backend:', err);
      return [];
    }
  },

  /**
   * Отримати загальний каталог NFT з сервера (fallback, якщо у користувача ще немає подарунків)
   */
  async getAllNfts(): Promise<Badge[]> {
    try {
      const items = await api.get<BackendNftItem[]>('/nft');
      if (items && Array.isArray(items)) {
        return items.map(mapBackendNftToBadge);
      }
      return [];
    } catch (err) {
      console.warn('[nftService] Failed to fetch all NFTs from backend:', err);
      return [];
    }
  },

  /**
   * Отримати баланс TDP токенів користувача з сервера (через API бекенду, без прямого RPC)
   */
  async getTdpBalance(address?: string | null): Promise<number> {
    if (!address || !address.trim()) return 0;
    try {
      const res = await api.get<{ balance: number }>(`/token/balance/${address.trim()}`);
      return typeof res.balance === 'number' ? res.balance : 0;
    } catch (err) {
      console.warn('[nftService] Failed to fetch token balance from backend:', err);
      return 0;
    }
  },
};
