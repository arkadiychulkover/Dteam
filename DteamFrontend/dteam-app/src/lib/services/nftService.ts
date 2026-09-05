import { BrowserProvider, Contract, JsonRpcProvider } from 'ethers';
import { DTEAM_NFT_ABI } from '../contracts/DteamNFTAbi';
import { api } from './api';

export const DTEAM_NFT_CONTRACT_ADDRESS = '0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0';
export const HARDHAT_RPC_URL = 'http://127.0.0.1:8545';

export enum NftRarity {
  Common = 0,
  Uncommon = 1,
  Rare = 2,
  Epic = 3,
  Legendary = 4
}

export interface NftGift {
  id: string;
  tokenId?: number | null;
  onChainTokenId?: number | null;
  name: string;
  description?: string | null;
  giftMessage?: string | null;
  rarity: NftRarity | number;
  imageUrl: string;
  image?: string;
  backgroundIndex?: number;
  patternIndex?: number;
  modelIndex?: number;
  contractAddress?: string;
  ownerAddress?: string | null;
  fromAddress?: string | null;
  userId?: string | null;
  isMinted?: boolean;
  lastTransferredAt?: string | null;
  createdAt?: string;
}

export function formatBadgeNumber(tokenId?: number | string | null): string {
  if (tokenId === undefined || tokenId === null || tokenId === '') return '#000';
  const num = Number(tokenId);
  if (isNaN(num)) return '#000';
  return `#${String(num).padStart(3, '0')}`;
}

export function getRarityInfo(rarity: NftRarity | number) {
  const r = Number(rarity);
  switch (r) {
    case 4:
    case NftRarity.Legendary:
      return {
        label: 'Легендарний',
        color: 'text-amber-400',
        badgeBg: 'bg-amber-500/20 border-amber-500/40 text-amber-300',
        glowColor: 'shadow-amber-500/20'
      };
    case 3:
    case NftRarity.Epic:
      return {
        label: 'Епічний',
        color: 'text-purple-400',
        badgeBg: 'bg-purple-500/20 border-purple-500/40 text-purple-300',
        glowColor: 'shadow-purple-500/20'
      };
    case 2:
    case NftRarity.Rare:
      return {
        label: 'Рідкісний',
        color: 'text-blue-400',
        badgeBg: 'bg-blue-500/20 border-blue-500/40 text-blue-300',
        glowColor: 'shadow-blue-500/20'
      };
    case 1:
    case NftRarity.Uncommon:
      return {
        label: 'Незвичайний',
        color: 'text-emerald-400',
        badgeBg: 'bg-emerald-500/20 border-emerald-500/40 text-emerald-300',
        glowColor: 'shadow-emerald-500/20'
      };
    case 0:
    case NftRarity.Common:
    default:
      return {
        label: 'Звичайний',
        color: 'text-slate-400',
        badgeBg: 'bg-slate-500/20 border-slate-500/40 text-slate-300',
        glowColor: 'shadow-slate-500/10'
      };
  }
}

/**
 * Отримання списку значків (NFT) поточного користувача зі смарт-контракту в блокчейні (DNFT):
 * 1. Зчитує токени, які належать даній адресі через події Transfer та перевірку ownerOf
 * 2. Викликає contract.tokenURI(tokenId) для отримання посилання на подарунок
 * 3. Завантажує дані подарунка з бекенда за цим посиланням
 */
export async function getUserNftsFromContract(walletAddress: string, userId?: string): Promise<NftGift[]> {
  if (!walletAddress) {
    if (userId) {
      return await getUserGiftsByUserId(userId);
    }
    return [];
  }

  let provider: any = null;

  // 1. Спроба через MetaMask якщо він підключений до Hardhat (31337)
  if (typeof window !== 'undefined' && (window as any).ethereum) {
    try {
      const browserProvider = new BrowserProvider((window as any).ethereum);
      const network = await browserProvider.getNetwork();
      if (Number(network.chainId) === 31337) {
        provider = browserProvider;
      }
    } catch {
      provider = null;
    }
  }

  // 2. Фоллбек на прямий Hardhat JSON-RPC
  if (!provider) {
    provider = new JsonRpcProvider(HARDHAT_RPC_URL);
  }

  const contract = new Contract(DTEAM_NFT_CONTRACT_ADDRESS, DTEAM_NFT_ABI as any, provider);
  const targetAddr = walletAddress.toLowerCase();
  const ownedTokenIds: number[] = [];

  try {
    const filter = contract.filters.Transfer(null, walletAddress);
    const events = await contract.queryFilter(filter, 0, 'latest');

    const candidateIds = new Set<number>();
    for (const evt of events) {
      if ('args' in evt && evt.args) {
        const tokenId = Number(evt.args[2] ?? evt.args.tokenId);
        candidateIds.add(tokenId);
      }
    }

    for (const id of candidateIds) {
      try {
        const currentOwner = await contract.ownerOf(id);
        if (currentOwner.toLowerCase() === targetAddr) {
          ownedTokenIds.push(id);
        }
      } catch (err) {
        console.warn(`[NFT Service] Error checking ownerOf for #${id}:`, err);
      }
    }
  } catch (err) {
    console.warn('[NFT Service] Error querying Transfer events on chain, trying fallback to backend:', err);
  }

  // Якщо через блокчейн нічого не знайшли, але передано userId, фоллбечимось на бекенд
  if (ownedTokenIds.length === 0 && userId) {
    return await getUserGiftsByUserId(userId);
  }

  const gifts: NftGift[] = [];
  for (const tokenId of ownedTokenIds) {
    try {
      let uri: string = await contract.tokenURI(tokenId);

      // Якщо uri містить GUID подарунка (/api/nft/{guid})
      const idMatch = uri.match(/\/api\/nft\/([a-f0-9\-]{36})/i);
      if (idMatch && idMatch[1]) {
        try {
          const giftData = await api.get<NftGift>(`/nft/${idMatch[1]}`);
          if (giftData) {
            gifts.push({
              ...giftData,
              tokenId: tokenId, // Номер буквально є айді NFT на блокчейні!
              onChainTokenId: tokenId
            });
            continue;
          }
        } catch {}
      }

      // Якщо uri повний або відносний, пробуємо також завантажити за tokenId
      const giftData = await api.get<NftGift>(`/nft/token/${tokenId}`);
      if (giftData) {
        gifts.push({
          ...giftData,
          tokenId: tokenId, // Номер буквально є айді NFT на блокчейні!
          onChainTokenId: tokenId
        });
      }
    } catch (err) {
      console.warn(`[NFT Service] Failed to load data for token #${tokenId}:`, err);
    }
  }

  // Сортуємо за tokenId за спаданням (найновіші зверху)
  gifts.sort((a, b) => (Number(b.tokenId ?? 0) - Number(a.tokenId ?? 0)));
  return gifts;
}

/**
 * Отримання подарунків чужого профілю просто за айді користувача з бекенду:
 * GET /api/nft/user/{userId}/gifts
 */
export async function getUserGiftsByUserId(userId: string): Promise<NftGift[]> {
  if (!userId) return [];
  try {
    const gifts = await api.get<NftGift[]>(`/nft/user/${userId}/gifts`);
    return (gifts || []).sort((a, b) => (Number(b.tokenId ?? 0) - Number(a.tokenId ?? 0)));
  } catch (err) {
    console.error(`[NFT Service] Failed to fetch gifts for user ${userId}:`, err);
    return [];
  }
}
