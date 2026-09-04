import { BrowserProvider, Contract } from 'ethers';
import { DTEAM_NFT_ABI } from '../contracts/DteamNFTAbi';
import { api } from './api';

export const DTEAM_NFT_CONTRACT_ADDRESS = '0xb7278A61aa25c888815aFC32Ad3cC52fF24fE575';

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
        borderColor: 'border-amber-400/50 hover:border-amber-300',
        cardBg: 'from-amber-950/40 via-yellow-950/20 to-[#02171d]',
        badgeBg: 'bg-amber-400/15 border-amber-400/40 text-amber-300',
        glow: 'hover:shadow-[0_0_25px_rgba(251,191,36,0.25)]'
      };
    case 3:
    case NftRarity.Epic:
      return {
        label: 'Епічний',
        color: 'text-purple-400',
        borderColor: 'border-purple-500/50 hover:border-purple-300',
        cardBg: 'from-purple-950/40 via-fuchsia-950/20 to-[#02171d]',
        badgeBg: 'bg-purple-400/15 border-purple-400/40 text-purple-300',
        glow: 'hover:shadow-[0_0_25px_rgba(192,132,252,0.25)]'
      };
    case 2:
    case NftRarity.Rare:
      return {
        label: 'Рідкісний',
        color: 'text-cyan-400',
        borderColor: 'border-cyan-500/50 hover:border-cyan-300',
        cardBg: 'from-cyan-950/40 via-sky-950/20 to-[#02171d]',
        badgeBg: 'bg-cyan-400/15 border-cyan-400/40 text-cyan-300',
        glow: 'hover:shadow-[0_0_25px_rgba(34,211,238,0.25)]'
      };
    case 1:
    case NftRarity.Uncommon:
      return {
        label: 'Незвичайний',
        color: 'text-emerald-400',
        borderColor: 'border-emerald-500/50 hover:border-emerald-300',
        cardBg: 'from-emerald-950/40 via-teal-950/20 to-[#02171d]',
        badgeBg: 'bg-emerald-400/15 border-emerald-400/40 text-emerald-300',
        glow: 'hover:shadow-[0_0_25px_rgba(52,211,153,0.25)]'
      };
    default:
      return {
        label: 'Звичайний',
        color: 'text-slate-400',
        borderColor: 'border-slate-700/60 hover:border-slate-500',
        cardBg: 'from-slate-900/60 via-slate-900/30 to-[#02171d]',
        badgeBg: 'bg-slate-800/80 border-slate-700 text-slate-400',
        glow: 'hover:shadow-[0_0_15px_rgba(148,163,184,0.15)]'
      };
  }
}

/**
 * Отримання списку значків/подарунків користувача напряму зі смарт-контракту:
 * 1. Зчитує токени, які належать даній адресі через події Transfer та перевірку ownerOf
 * 2. Викликає contract.tokenURI(tokenId) для отримання посилання на подарунок
 * 3. Завантажує дані подарунка з бекенда за цим посиланням
 */
export async function getUserNftsFromContract(walletAddress: string): Promise<NftGift[]> {
  if (!walletAddress) return [];

  // Завжди використовуємо MetaMask
  if (typeof window === 'undefined' || !(window as any).ethereum) {
    console.warn('[NFT Service] MetaMask не знайдено в браузері.');
    return [];
  }

  const provider = new BrowserProvider((window as any).ethereum);
  const contract = new Contract(DTEAM_NFT_CONTRACT_ADDRESS, DTEAM_NFT_ABI as any, provider);

  const targetAddr = walletAddress.toLowerCase();
  const ownedTokenIds: number[] = [];

  // Отримуємо події Transfer де recipient = targetAddr виключно через MetaMask
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

    // Перевіряємо поточного власника (ownerOf)
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
    console.error('[NFT Service] Error querying Transfer events via MetaMask:', err);
  }

  // Для кожного токена отримуємо tokenURI зі смарт-контракту та завантажуємо ведомости з бекенду
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
