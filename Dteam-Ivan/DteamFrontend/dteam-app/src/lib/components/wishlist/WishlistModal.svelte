<script lang="ts">
import { wishlistStore } from '../../stores/wishlistStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice } from '../../utils/formatters';
  import type { WishlistItem } from '../../types';
  import {
    X,
    Heart,
    Trash2,
    ShoppingCart,
    Search,
    ArrowUpDown,
    Sparkles,
    Gamepad2,
    ExternalLink
  } from 'lucide-svelte';

  let search = $state('');
  let sortBy = $state<'date' | 'price_asc' | 'price_desc' | 'title'>('date');

  const filteredItems = $derived(
    $wishlistStore.items
      .filter((item) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (
          item.game.title.toLowerCase().includes(q) ||
          (item.game.shortDescription && item.game.shortDescription.toLowerCase().includes(q)) ||
          (item.game.tags && item.game.tags.some((t) => t.toLowerCase().includes(q)))
        );
      })
      .sort((a, b) => {
        if (sortBy === 'price_asc') return Number(a.game.priceInNanoTons) - Number(b.game.priceInNanoTons);
        if (sortBy === 'price_desc') return Number(b.game.priceInNanoTons) - Number(a.game.priceInNanoTons);
        if (sortBy === 'title') return a.game.title.localeCompare(b.game.title);
        return new Date(b.addedAt).getTime() - new Date(a.addedAt).getTime();
      })
  );

  function handleDecorativeBuy(e: MouseEvent, item: WishlistItem) {
    e.stopPropagation();
    uiStore.addToast({
      title: 'Демо-режим',
      message: `Кнопка покупки для '${item.game.title}' бутафорная.`,
      type: 'info',
    });
  }

  function handleOpenGameDetails(item: WishlistItem) {
    wishlistStore.closeModal();
    gamesStore.selectGame(item.game);
  }
</script>

{#if $wishlistStore.isModalOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-3xl bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6 overflow-hidden max-h-[90vh] flex flex-col">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-rose-500/20 border border-rose-500/40 flex items-center justify-center text-rose-400">
            <Heart class="w-5 h-5 fill-rose-500 text-rose-500" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
              Список бажань
              <span class="text-xs px-2.5 py-0.5 rounded-full bg-rose-950 text-rose-300 border border-rose-500/30 font-mono font-bold">
                {$wishlistStore.items.length}
              </span>
            </h3>
            <p class="text-xs text-slate-400">Збережені ігри та доповнення</p>
          </div>
        </div>

        <div class="flex items-center gap-2">
          {#if $wishlistStore.items.length > 0}
            <button
              onclick={() => wishlistStore.clearWishlist()}
              class="px-3 py-1.5 rounded-xl bg-slate-800/80 hover:bg-rose-900/60 border border-slate-700 hover:border-rose-500 text-xs font-semibold text-slate-300 hover:text-rose-200 transition-all cursor-pointer flex items-center gap-1.5"
            >
              <Trash2 class="w-3.5 h-3.5" />
              <span>Очистити все</span>
            </button>
          {/if}
          <button
            onclick={() => wishlistStore.closeModal()}
            class="p-2 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/60 transition-colors cursor-pointer"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>

      {#if $wishlistStore.items.length > 0}
        <div class="flex items-center gap-3 py-3 border-b border-cyan-950/60">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
            <input
              type="text"
              placeholder="Пошук у списку бажань..."
              bind:value={search}
              class="w-full pl-9 pr-3 py-1.5 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500"
            />
          </div>

          <div class="flex items-center gap-1.5 text-xs text-slate-400 shrink-0">
            <ArrowUpDown class="w-3.5 h-3.5 text-cyan-400" />
            <select
              bind:value={sortBy}
              class="bg-[#0a232c] text-cyan-300 font-bold border border-cyan-500/30 rounded-lg px-2 py-1 text-xs focus:outline-none cursor-pointer"
            >
              <option value="date">За датою додавання</option>
              <option value="price_asc">Ціна: від низької</option>
              <option value="price_desc">Ціна: від високої</option>
              <option value="title">За назвою</option>
            </select>
          </div>
        </div>
      {/if}

      <div class="flex-1 overflow-y-auto py-4 space-y-3 pr-1">
        {#if filteredItems.length > 0}
          {#each filteredItems as item (item.gameId)}
            <div
              role="button"
              tabindex="0"
              onclick={() => handleOpenGameDetails(item)}
              onkeydown={(e) => e.key === 'Enter' && handleOpenGameDetails(item)}
              class="group flex items-center justify-between p-3 rounded-2xl bg-[#0a232c]/90 hover:bg-[#0c2b36] border border-cyan-500/20 hover:border-cyan-400/60 shadow-lg transition-all cursor-pointer"
            >
              <div class="flex items-center gap-3.5 min-w-0 pr-2">
                <div class="relative aspect-[16/10] w-24 sm:w-28 rounded-xl overflow-hidden bg-slate-950 border border-cyan-500/30 shrink-0">
                  <img
                    src={item.game.headerImageUrl || item.game.coverImageUrl || 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=400'}
                    alt={item.game.title}
                    class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  {#if item.game.isDlc}
                    <span class="absolute top-1 left-1 px-1.5 py-0.2 rounded bg-purple-950 text-purple-300 text-[8px] font-bold uppercase">
                      DLC
                    </span>
                  {/if}
                </div>

                <div class="min-w-0">
                  <h4 class="text-sm font-extrabold text-white group-hover:text-cyan-300 transition-colors truncate">
                    {item.game.title}
                  </h4>
                  <div class="text-[11px] text-slate-400 truncate mt-0.5">
                    {item.game.genres?.slice(0, 2).join(', ') || 'Game'} • v{item.game.version}
                  </div>
                  <div class="text-[10px] text-slate-500 mt-0.5">
                    Додано {new Date(item.addedAt).toLocaleDateString('uk-UA')}
                  </div>
                </div>
              </div>

              <div class="flex items-center gap-3 shrink-0">
                <div class="text-right">
                  {#if (item.game.discountPercentage || 0) > 0}
                    <span class="px-1.5 py-0.2 rounded bg-rose-600 text-white font-extrabold text-[10px] block text-center mb-0.5">
                      -{item.game.discountPercentage}%
                    </span>
                  {/if}
                  <span class="text-xs sm:text-sm font-black text-cyan-300 font-mono">
                    {formatPrice(item.game.priceInNanoTons, item.game.discountPercentage)}
                  </span>
                </div>

                <button
                  type="button"
                  onclick={(e) => handleDecorativeBuy(e, item)}
                  class="p-2 rounded-xl bg-cyan-500/20 hover:bg-cyan-500 text-cyan-300 hover:text-black border border-cyan-500/40 transition-all cursor-pointer"
                  title="Купити (Демо)"
                >
                  <ShoppingCart class="w-4 h-4" />
                </button>

                <button
                  type="button"
                  onclick={(e) => {
                    e.stopPropagation();
                    wishlistStore.toggleWishlist(item.game);
                  }}
                  class="p-2 rounded-xl bg-slate-800 hover:bg-rose-600 text-slate-400 hover:text-white transition-all cursor-pointer"
                  title="Видалити зі списку бажань"
                >
                  <Trash2 class="w-4 h-4" />
                </button>
              </div>
            </div>
          {/each}
        {:else if $wishlistStore.items.length > 0}
          <div class="text-center py-12 text-slate-400 text-xs">
            За запитом "{search}" нічого не знайдено в списку бажань.
          </div>
        {:else}
          <div class="text-center py-16 space-y-3">
            <div class="w-14 h-14 rounded-2xl bg-rose-500/10 border border-rose-500/20 flex items-center justify-center text-rose-400 mx-auto">
              <Heart class="w-7 h-7 text-rose-400/60" />
            </div>
            <h4 class="text-base font-bold text-white">Ваш список бажань порожній</h4>
            <p class="text-xs text-slate-400 max-w-sm mx-auto">
              Натискайте на іконку серця ♥ біля будь-якої гри, щоб зберегти її тут і стежити за знижками.
            </p>
            <button
              onclick={() => {
                wishlistStore.closeModal();
                uiStore.setTab('catalog');
              }}
              class="mt-2 px-5 py-2.5 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-black font-bold text-xs shadow-lg shadow-cyan-500/25 transition-all cursor-pointer inline-flex items-center gap-1.5"
            >
              <Gamepad2 class="w-4 h-4" />
              <span>Перейти до каталогу ігор</span>
            </button>
          </div>
        {/if}
      </div>
    </div>
  </div>
{/if}

