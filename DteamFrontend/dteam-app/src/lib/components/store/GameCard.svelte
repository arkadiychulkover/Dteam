<script lang="ts">
  import type { Game } from '../../types';
  import { formatPrice, formatBasePrice } from '../../utils/formatters';
  import { gamesStore } from '../../stores/gamesStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { uiStore } from '../../stores/uiStore';
  import { Star, ShoppingCart, Heart } from 'lucide-svelte';

  interface Props {
    game: Game;
  }

  let { game }: Props = $props();

  const isWishlisted = $derived($wishlistStore.wishlistGameIds.has(game.id));

  function handleDecorativeBuy(e: MouseEvent) {
    e.stopPropagation();
    uiStore.addToast({
      title: 'Демо-режим',
      message: `Кнопка покупки для '${game.title}' бутафорная.`,
      type: 'info',
    });
  }

  function handleToggleWishlist(e: MouseEvent) {
    e.stopPropagation();
    wishlistStore.toggleWishlist(game);
  }
  function openGame() {
    gamesStore.selectGame(game);
    uiStore.setTab('game');
  }
</script>

<div
  role="button"
  tabindex="0"
  onclick={openGame}
  onkeydown={(e) => e.key === 'Enter' && openGame()}
  class="group relative flex flex-col rounded-2xl bg-[#061820]/90 hover:bg-[#08202b] border border-cyan-500/20 hover:border-cyan-400/80 overflow-hidden shadow-lg hover:shadow-2xl hover:shadow-cyan-500/20 transition-all duration-300 transform hover:-translate-y-1.5 cursor-pointer text-left"
>
  <div class="relative aspect-[3/4] w-full overflow-hidden bg-slate-950">
    <img
      src={game.coverImageUrl || game.headerImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80'}
      alt={game.title}
      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
    />
    <div class="absolute inset-0 bg-gradient-to-t from-[#061820] via-transparent to-black/30"></div>

    <div class="absolute top-3 right-3 flex items-center gap-1 px-2 py-0.5 rounded-lg bg-black/70 backdrop-blur-md border border-cyan-500/30 text-[10px] font-black text-cyan-300 shadow-md">
      <Star class="w-3 h-3 fill-amber-400 text-amber-400" />
      <span>{game.averageRating > 0 ? game.averageRating.toFixed(1) : '5.0'}</span>
    </div>

    <button
      type="button"
      onclick={handleToggleWishlist}
      class="absolute top-3 left-3 p-1.5 rounded-xl bg-black/70 backdrop-blur-md border transition-all cursor-pointer shadow-md
        {isWishlisted
          ? 'border-rose-500/60 text-rose-500 hover:bg-rose-950/80'
          : 'border-white/20 text-slate-300 hover:text-rose-400 hover:border-rose-400/60'}"
      title={isWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
    >
      <Heart class="w-3.5 h-3.5 {isWishlisted ? 'fill-rose-500' : ''}" />
    </button>

    {#if game.isDlc}
      <div class="absolute top-11 left-3 px-2 py-0.5 rounded-lg bg-purple-950/80 backdrop-blur-md border border-purple-500/40 text-[9px] font-extrabold text-purple-300 uppercase tracking-wider">
        DLC
      </div>
    {/if}

    <div class="absolute bottom-3 left-3 right-3">
      <h3 class="font-extrabold text-base text-white group-hover:text-cyan-300 transition-colors line-clamp-1">
        {game.title}
      </h3>

      <div class="flex items-center justify-between mt-1.5">
        <div class="flex items-center gap-1.5">
          {#if (game.discountPercentage || 0) > 0}
            <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
              -{game.discountPercentage}%
            </span>
          {/if}

          <span class="text-sm font-black text-cyan-300 font-mono tracking-tight">
            {formatPrice(game.priceInNanoTons, game.discountPercentage)}
          </span>

          {#if (game.discountPercentage || 0) > 0}
            <span class="text-[10px] text-slate-500 line-through font-mono">
              {formatBasePrice(game.priceInNanoTons)}
            </span>
          {/if}
        </div>

        <button
          type="button"
          onclick={handleDecorativeBuy}
          class="flex items-center gap-1 px-2.5 py-1 rounded-lg bg-cyan-500/20 hover:bg-cyan-400 text-cyan-300 hover:text-black border border-cyan-500/40 text-[11px] font-bold transition-all cursor-pointer group-hover:scale-105"
          title="Buy game (Demo)"
        >
          <ShoppingCart class="w-3 h-3" />
          <span>Купити</span>
        </button>
      </div>
    </div>
  </div>
</div>
