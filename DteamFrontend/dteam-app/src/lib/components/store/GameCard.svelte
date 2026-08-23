<script lang="ts">
  import type { Game } from '../../types';
  import { formatPrice } from '../../utils/formatters';
  import { gamesStore } from '../../stores/gamesStore';
  import { libraryStore } from '../../stores/libraryStore';
  import { Star, Download, ShoppingCart, Check } from 'lucide-svelte';

  interface Props {
    game: Game;
  }

  let { game }: Props = $props();

  const isOwned = $derived(
    $libraryStore.items.some(i => i.gameId === game.id)
  );

  function handleBuy(e: MouseEvent) {
    e.stopPropagation();
    libraryStore.buyGame(game);
  }
</script>

<div
  role="button"
  tabindex="0"
  onclick={() => gamesStore.selectGame(game)}
  onkeydown={(e) => e.key === 'Enter' && gamesStore.selectGame(game)}
  class="group relative flex flex-col rounded-2xl bg-[#131622] hover:bg-[#1a1e2e] border border-slate-800/80 hover:border-cyan-500/50 overflow-hidden shadow-lg hover:shadow-2xl hover:shadow-cyan-500/10 transition-all duration-300 transform hover:-translate-y-1 cursor-pointer text-left"
>
  <!-- Game Cover Media -->
  <div class="relative aspect-[16/9] w-full overflow-hidden bg-slate-950">
    <img
      src={game.coverImageUrl || game.headerImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=600&auto=format&fit=crop&q=80'}
      alt={game.title}
      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
    />
    <div class="absolute inset-0 bg-gradient-to-t from-[#131622] via-transparent to-transparent opacity-80"></div>

    <!-- Rating badge -->
    <div class="absolute top-3 right-3 flex items-center gap-1 px-2 py-1 rounded-lg bg-black/60 backdrop-blur-md border border-white/10 text-[11px] font-bold text-amber-400">
      <Star class="w-3 h-3 fill-amber-400" />
      <span>{game.averageRating}</span>
    </div>

    {#if isOwned}
      <div class="absolute top-3 left-3 flex items-center gap-1 px-2.5 py-1 rounded-lg bg-emerald-500/90 text-black text-[11px] font-extrabold shadow-md">
        <Check class="w-3.5 h-3.5 stroke-[3]" /> IN LIBRARY
      </div>
    {/if}
  </div>

  <!-- Details Content -->
  <div class="flex-1 p-4 flex flex-col justify-between">
    <div>
      <h3 class="font-bold text-base text-white group-hover:text-cyan-400 transition-colors line-clamp-1">
        {game.title}
      </h3>

      <p class="text-xs text-slate-400 mt-1.5 line-clamp-2 leading-relaxed">
        {game.shortDescription || game.description}
      </p>

      <!-- Tags -->
      <div class="flex flex-wrap gap-1.5 mt-3">
        {#if game.tags}
          {#each game.tags.slice(0, 3) as tag}
            <span class="text-[10px] font-medium px-2 py-0.5 rounded bg-slate-800 text-slate-300 border border-slate-700/60">
              {tag}
            </span>
          {/each}
        {/if}
      </div>
    </div>

    <!-- Bottom Footer: Price & Quick Action -->
    <div class="flex items-center justify-between mt-4 pt-3 border-t border-slate-800/80">
      <div>
        <span class="text-[10px] text-slate-500 uppercase font-semibold block">Price</span>
        <span class="text-sm font-extrabold text-cyan-300 font-mono">
          {formatPrice(game.priceInNanoTons)}
        </span>
      </div>

      {#if isOwned}
        <span class="text-xs text-slate-400 font-medium">Ready to play</span>
      {:else}
        <button
          type="button"
          onclick={handleBuy}
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-cyan-500/10 hover:bg-cyan-500 text-cyan-400 hover:text-black border border-cyan-500/40 text-xs font-bold transition-all shadow-sm active:scale-95 cursor-pointer"
        >
          <ShoppingCart class="w-3.5 h-3.5" />
          <span>Buy</span>
        </button>
      {/if}
    </div>
  </div>
</div>
