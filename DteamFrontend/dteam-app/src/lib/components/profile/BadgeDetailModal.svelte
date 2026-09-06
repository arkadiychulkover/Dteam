<script lang="ts">
  import { type NftGift, formatBadgeNumber, getRarityInfo, DTEAM_NFT_CONTRACT_ADDRESS } from '../../services/nftService';
  import BackendImage from '../ui/BackendImage.svelte';
  import { X, ExternalLink, Calendar, Hash, ShieldCheck, Sparkles, Copy, Check } from 'lucide-svelte';
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

  let copied = $state(false);

  async function copyContractAddress() {
    try {
      await navigator.clipboard.writeText(DTEAM_NFT_CONTRACT_ADDRESS);
      copied = true;
      setTimeout(() => {
        copied = false;
      }, 2000);
    } catch (err) {
      console.warn('Failed to copy contract address:', err);
    }
  }

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

      <button
        onclick={onClose}
        class="absolute top-4 right-4 text-slate-400 hover:text-white p-2 rounded-full hover:bg-white/10 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="flex items-center gap-3">
        <span class="text-xl font-black text-white bg-black/60 px-3 py-1 rounded-xl border border-white/10">
          {badgeNumber}
        </span>
        <span class="text-xs font-bold uppercase tracking-wider px-3 py-1 rounded-full border {rarityInfo.badgeBg}">
          {rarityInfo.label}
        </span>
      </div>

      <div class="relative w-full aspect-square rounded-2xl overflow-hidden bg-black/50 border border-white/10 flex items-center justify-center p-2">
        <BackendImage
          src={gift.imageUrl || gift.image}
          alt={badgeNumber}
          class="w-full h-full object-contain rounded-xl"
        />
      </div>

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

      <div class="space-y-1.5 pt-1">
        <div class="flex items-center justify-between text-[11px] px-1">
          <span class="text-[10px] text-slate-400 font-semibold uppercase tracking-wider">Контракт</span>
          <span class="text-cyan-400 font-bold flex items-center gap-1 text-[11px]">
            <ShieldCheck class="w-3.5 h-3.5 text-emerald-400" /> ERC-721 (DNFT)
          </span>
        </div>

        <div class="flex items-center justify-between gap-2 bg-black/50 border border-white/10 hover:border-cyan-500/40 rounded-xl px-3 py-2 transition-colors group">
          <span class="font-mono text-[11px] text-slate-300 select-all break-all leading-relaxed" title={DTEAM_NFT_CONTRACT_ADDRESS}>
            {DTEAM_NFT_CONTRACT_ADDRESS}
          </span>
          <button
            type="button"
            onclick={copyContractAddress}
            class="flex items-center gap-1.5 text-[11px] font-medium px-2.5 py-1 rounded-lg bg-white/5 hover:bg-cyan-500/20 text-slate-300 hover:text-cyan-300 border border-white/10 hover:border-cyan-500/30 transition-all cursor-pointer shrink-0"
            title="Скопіювати повну адресу смарт-контракту"
          >
            {#if copied}
              <Check class="w-3.5 h-3.5 text-emerald-400" />
              <span class="text-emerald-400 text-[10px]">Скопійовано</span>
            {:else}
              <Copy class="w-3.5 h-3.5 text-slate-400 group-hover:text-cyan-400" />
              <span class="text-[10px]">Копіювати</span>
            {/if}
          </button>
        </div>
      </div>
    </div>
  </div>
{/if}

