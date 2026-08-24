<script lang="ts">
  import { gamesStore } from '../../stores/gamesStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice, formatBasePrice, formatBytes } from '../../utils/formatters';
  import { X, Star, ShoppingCart, Download, Check, Sparkles, HardDrive, Shield, Heart } from 'lucide-svelte';

  const game = $derived($gamesStore.selectedGame);
  const isWishlisted = $derived(game ? $wishlistStore.wishlistGameIds.has(game.id) : false);

  let selectedScreenshot = $state<string | null>(null);

  function close() {
    gamesStore.selectGame(null);
  }

  function handleDecorativeBuy() {
    uiStore.addToast({
      title: 'Демо-режим',
      message: `Кнопка покупки для '${game?.title}' бутафорная. Модуль транзакций TON временно отключен.`,
      type: 'info',
    });
  }

  function handleToggleWishlist() {
    if (game) {
      wishlistStore.toggleWishlist(game);
    }
  }
</script>

{#if game}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-4xl max-h-[90vh] overflow-y-auto bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 md:p-8">
      <button
        onclick={close}
        class="absolute top-4 right-4 p-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="grid grid-cols-1 md:grid-cols-12 gap-6">
        <div class="md:col-span-7 flex flex-col gap-3">
          <div class="rounded-xl overflow-hidden aspect-video bg-black border border-cyan-500/20 shadow-lg">
            <img
              src={selectedScreenshot || game.headerImageUrl || game.coverImageUrl || ''}
              alt={game.title}
              class="w-full h-full object-cover"
            />
          </div>

          {#if game.screenshotUrls && game.screenshotUrls.length > 0}
            <div class="flex gap-2 overflow-x-auto pb-1">
              <button
                onclick={() => selectedScreenshot = (game.headerImageUrl || game.coverImageUrl || null)}
                class="w-20 h-12 rounded-lg overflow-hidden shrink-0 border-2 transition-all cursor-pointer {selectedScreenshot === (game.headerImageUrl || game.coverImageUrl) || !selectedScreenshot ? 'border-cyan-400' : 'border-transparent opacity-60'}"
              >
                <img src={game.headerImageUrl || game.coverImageUrl || ''} alt="Header" class="w-full h-full object-cover" />
              </button>
              {#each game.screenshotUrls as shot}
                <button
                  onclick={() => selectedScreenshot = shot}
                  class="w-20 h-12 rounded-lg overflow-hidden shrink-0 border-2 transition-all cursor-pointer {selectedScreenshot === shot ? 'border-cyan-400' : 'border-transparent opacity-60'}"
                >
                  <img src={shot} alt="Screenshot" class="w-full h-full object-cover" />
                </button>
              {/each}
            </div>
          {/if}
        </div>

        <div class="md:col-span-5 flex flex-col justify-between">
          <div>
            <div class="flex items-center gap-2">
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-cyan-950 text-cyan-300 border border-cyan-500/30 uppercase tracking-widest">
                DTEAM STORE
              </span>
              {#if game.isDlc}
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-purple-950 text-purple-300 border border-purple-500/30 uppercase tracking-widest">
                  DLC
                </span>
              {/if}
            </div>

            <h2 class="text-2xl font-black text-white mt-1.5 leading-tight">
              {game.title}
            </h2>

            <p class="text-xs text-slate-300 mt-2.5 leading-relaxed">
              {game.description}
            </p>

            <div class="mt-4 space-y-2 text-xs border-t border-b border-cyan-950 py-3">
              <div class="flex justify-between text-slate-400">
                <span>Рейтинг:</span>
                <span class="text-amber-400 font-bold flex items-center gap-1">
                  <Star class="w-3.5 h-3.5 fill-amber-400" /> {game.averageRating > 0 ? game.averageRating.toFixed(1) : '5.0'} ({game.reviewsCount} reviews)
                </span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Версія:</span>
                <span class="text-slate-200 font-mono">v{game.version}</span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Розмір на диску:</span>
                <span class="text-slate-200">{formatBytes(game.sizeInBytes)}</span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Завантажень:</span>
                <span class="text-cyan-300 font-mono">{game.downloadCount.toLocaleString()}</span>
              </div>
            </div>
          </div>

          <div class="mt-6 pt-4 bg-[#0a232c]/80 p-4 rounded-xl border border-cyan-500/20 space-y-3">
            <div class="flex items-baseline justify-between">
              <div class="flex items-center gap-2">
                <span class="text-xs text-slate-400 font-semibold uppercase">Ціна</span>
                {#if (game.discountPercentage || 0) > 0}
                  <span class="px-1.5 py-0.5 rounded bg-rose-600 text-white font-extrabold text-[10px]">
                    -{game.discountPercentage}%
                  </span>
                {/if}
              </div>
              <div class="flex items-center gap-2">
                <span class="text-2xl font-black text-cyan-300 font-mono">
                  {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                </span>
                {#if (game.discountPercentage || 0) > 0}
                  <span class="text-xs text-slate-400 line-through font-mono">
                    {formatBasePrice(game.priceInNanoTons)}
                  </span>
                {/if}
              </div>
            </div>

            <div class="flex gap-2">
              <button
                onclick={handleToggleWishlist}
                class="flex-1 py-2.5 rounded-xl border transition-all cursor-pointer flex items-center justify-center gap-2 text-xs font-bold
                  {isWishlisted
                    ? 'bg-rose-950/60 border-rose-500 text-rose-300 hover:bg-rose-900/80'
                    : 'bg-[#061820] border-cyan-500/30 text-slate-300 hover:text-rose-400 hover:border-rose-400/60'}"
                title={isWishlisted ? 'Видалити зі списку бажань' : 'Додати до списку бажань'}
              >
                <Heart class="w-4 h-4 {isWishlisted ? 'fill-rose-500 text-rose-500' : ''}" />
                <span>{isWishlisted ? 'У бажаному ♥' : 'У список бажань'}</span>
              </button>

              <button
                onclick={handleDecorativeBuy}
                class="flex-1 py-2.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-extrabold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2"
              >
                <ShoppingCart class="w-4 h-4 text-black" />
                <span>Купити гру</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
{/if}
