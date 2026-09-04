<script lang="ts">
  import { type NftGift, formatBadgeNumber, getRarityInfo } from '../../services/nftService';
  import BackendImage from '../ui/BackendImage.svelte';
  import { Sparkles, Calendar } from 'lucide-svelte';
  import { formatDate } from '../../utils/formatters';

  let {
    gift,
    onclick
  }: {
    gift: NftGift;
    onclick?: (gift: NftGift) => void;
  } = $props();

  const rarityInfo = $derived(getRarityInfo(gift.rarity));
  const badgeNumber = $derived(formatBadgeNumber(gift.onChainTokenId !== undefined && gift.onChainTokenId !== null ? gift.onChainTokenId : gift.tokenId));
</script>

<div
  role="button"
  tabindex="0"
  onclick={() => onclick?.(gift)}
  onkeydown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onclick?.(gift); } }}
  class="group relative rounded-2xl bg-gradient-to-b {rarityInfo.cardBg} border {rarityInfo.borderColor} {rarityInfo.glow} transition-all duration-300 overflow-hidden flex flex-col p-3 text-left cursor-pointer hover:-translate-y-1 shadow-lg focus:outline-none focus:ring-2 focus:ring-cyan-400"
>
  <!-- Верхня плашка: Номер у форматі #111 та бейдж рідкості (БЕЗ БАЛІВ) -->
  <div class="flex items-center justify-between gap-2 mb-2 z-10">
    <span class="text-xs font-black tracking-wider text-white bg-black/50 px-2.5 py-1 rounded-lg border border-white/10 backdrop-blur-md">
      {badgeNumber}
    </span>
    <span class="text-[10px] font-extrabold uppercase tracking-wider px-2 py-0.5 rounded-full border {rarityInfo.badgeBg}">
      {rarityInfo.label}
    </span>
  </div>

  <!-- Зображення значка / подарунка з анімацією при наведенні -->
  <div class="relative w-full aspect-square rounded-xl overflow-hidden bg-black/40 border border-white/5 mb-3 flex items-center justify-center group-hover:border-cyan-500/30 transition-colors">
    <BackendImage
      src={gift.imageUrl || gift.image}
      alt={badgeNumber}
      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
      fallbackText="Значок {badgeNumber}"
    />

    <!-- Легкий ефект сяйва при наведенні -->
    <div class="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"></div>
  </div>

  <!-- Опис та додаткова інформація (БЕЗ БАЛІВ) -->
  <div class="flex-1 flex flex-col justify-between space-y-2 z-10">
    {#if gift.description || gift.giftMessage}
      <p class="text-xs text-slate-300/90 line-clamp-2 leading-relaxed" title={gift.description || gift.giftMessage || ''}>
        {gift.giftMessage || gift.description}
      </p>
    {:else}
      <p class="text-xs text-slate-500 italic">
        Особливий значок Dteam
      </p>
    {/if}

    {#if gift.createdAt || gift.lastTransferredAt}
      <div class="flex items-center gap-1.5 text-[10px] text-slate-500 pt-1 border-t border-white/5">
        <Calendar class="w-3 h-3 text-slate-500" />
        <span>{formatDate(gift.lastTransferredAt || gift.createdAt)}</span>
      </div>
    {/if}
  </div>
</div>
