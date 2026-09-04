<script lang="ts">
  import { type NftGift, formatBadgeNumber, getRarityInfo, DTEAM_NFT_CONTRACT_ADDRESS } from '../../services/nftService';
  import BackendImage from '../ui/BackendImage.svelte';
  import { X, ExternalLink, Calendar, Hash, ShieldCheck, Sparkles } from 'lucide-svelte';
  import { formatDate } from '../../utils/formatters';

  let {
    gift,
    isOpen = false,
    onClose
  }: {
    gift: NftGift | null;
    isOpen: boolean;
    onClose: () => void;
  } = $props();

  const rarityInfo = $derived(gift ? getRarityInfo(gift.rarity) : null);
  const badgeNumber = $derived(gift ? formatBadgeNumber(gift.onChainTokenId !== undefined && gift.onChainTokenId !== null ? gift.onChainTokenId : gift.tokenId) : '');
</script>

{#if isOpen && gift && rarityInfo}
  <div
    class="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-[110] p-4 animate-in fade-in duration-200"
    onclick={(e) => { if (e.target === e.currentTarget) onClose(); }}
    onkeydown={(e) => { if (e.key === 'Escape') onClose(); }}
    role="dialog"
    tabindex="-1"
  >
    <div class="bg-gradient-to-b {rarityInfo.cardBg} border-2 {rarityInfo.borderColor} rounded-3xl p-6 w-full max-w-md shadow-2xl relative space-y-5">
      <!-- Кнопка закриття -->
      <button
        onclick={onClose}
        class="absolute top-4 right-4 text-slate-400 hover:text-white p-2 rounded-full hover:bg-white/10 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <!-- Заголовок: номер значка замість назви -->
      <div class="flex items-center gap-3">
        <span class="text-xl font-black text-white bg-black/60 px-3 py-1 rounded-xl border border-white/10">
          {badgeNumber}
        </span>
        <span class="text-xs font-bold uppercase tracking-wider px-3 py-1 rounded-full border {rarityInfo.badgeBg}">
          {rarityInfo.label}
        </span>
      </div>

      <!-- Зображення значка на весь розмір -->
      <div class="relative w-full aspect-square rounded-2xl overflow-hidden bg-black/50 border border-white/10 flex items-center justify-center p-2">
        <BackendImage
          src={gift.imageUrl || gift.image}
          alt={badgeNumber}
          class="w-full h-full object-contain rounded-xl"
        />
      </div>

      <!-- Опис та повідомлення (БЕЗ БАЛІВ) -->
      <div class="space-y-3 bg-black/40 rounded-2xl p-4 border border-white/5">
        {#if gift.giftMessage}
          <div>
            <span class="text-[10px] text-cyan-400 font-bold uppercase tracking-wider block mb-0.5">Повідомлення</span>
            <p class="text-xs text-slate-200 font-medium whitespace-pre-line">{gift.giftMessage}</p>
          </div>
        {/if}

        {#if gift.description}
          <div>
            <span class="text-[10px] text-slate-400 font-bold uppercase tracking-wider block mb-0.5">Опис</span>
            <p class="text-xs text-slate-300 leading-relaxed whitespace-pre-line">{gift.description}</p>
          </div>
        {/if}

        <div class="grid grid-cols-2 gap-2 pt-2 border-t border-white/10 text-[11px] text-slate-400">
          <div>
            <span class="text-[10px] text-slate-500 block">Token ID</span>
            <span class="font-mono text-slate-200 font-bold">{badgeNumber}</span>
          </div>
          <div>
            <span class="text-[10px] text-slate-500 block">Отримано</span>
            <span class="text-slate-200 font-medium">{formatDate(gift.lastTransferredAt || gift.createdAt)}</span>
          </div>
        </div>
      </div>

      <!-- Інформація про смарт-контракт -->
      <div class="flex items-center justify-between text-[11px] text-slate-400 px-1">
        <span class="font-mono text-[10px] text-slate-500 truncate max-w-[240px]">
          {DTEAM_NFT_CONTRACT_ADDRESS}
        </span>
        <span class="text-cyan-400 font-bold flex items-center gap-1">
          <ShieldCheck class="w-3.5 h-3.5 text-emerald-400" /> ERC-721 (DNFT)
        </span>
      </div>
    </div>
  </div>
{/if}
