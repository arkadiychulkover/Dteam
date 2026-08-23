<script lang="ts">
  import { gamesStore } from '../../stores/gamesStore';
  import { libraryStore } from '../../stores/libraryStore';
  import { formatPrice } from '../../utils/formatters';
  import type { Game } from '../../types';
  import { Star, Download, Sparkles, ChevronRight, ShoppingCart } from 'lucide-svelte';

  let currentIndex = $state(0);
  const featuredGames = $derived($gamesStore.games.slice(0, 3));
  const activeGame = $derived(featuredGames[currentIndex] || $gamesStore.games[0]);

  function handleBuy(game: Game) {
    libraryStore.buyGame(game);
  }
</script>

{#if activeGame}
  <div class="relative rounded-2xl overflow-hidden bg-slate-900 border border-slate-800/80 shadow-2xl group mb-10">
    <!-- Background backdrop with blur -->
    <div class="absolute inset-0 z-0">
      <img
        src={activeGame.headerImageUrl || activeGame.coverImageUrl}
        alt={activeGame.title}
        class="w-full h-full object-cover object-center brightness-40 filter blur-sm scale-105 transform transition-transform duration-1000"
      />
      <div class="absolute inset-0 bg-gradient-to-t from-[#0a0c14] via-[#0a0c14]/70 to-transparent"></div>
      <div class="absolute inset-0 bg-gradient-to-r from-[#0a0c14] via-[#0a0c14]/80 to-transparent"></div>
    </div>

    <!-- Content Grid -->
    <div class="relative z-10 p-6 md:p-10 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center min-h-[380px]">
      <!-- Left Info -->
      <div class="lg:col-span-7 flex flex-col justify-between">
        <div>
          <!-- Badge -->
          <div class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-cyan-500/20 border border-cyan-500/40 text-cyan-300 text-xs font-bold tracking-wide uppercase mb-4">
            <Sparkles class="w-3.5 h-3.5" /> Featured Spotlight
          </div>

          <h2 class="text-3xl md:text-5xl font-black text-white tracking-tight font-['Outfit'] leading-none">
            {activeGame.title}
          </h2>

          <p class="mt-3 text-slate-300 text-sm md:text-base line-clamp-3 leading-relaxed max-w-xl">
            {activeGame.shortDescription || activeGame.description}
          </p>

          <!-- Tags & Stats -->
          <div class="flex flex-wrap items-center gap-2 mt-4">
            {#if activeGame.tags}
              {#each activeGame.tags as tag}
                <span class="px-2.5 py-1 rounded-md bg-slate-800/80 text-slate-300 text-xs font-medium border border-slate-700">
                  {tag}
                </span>
              {/each}
            {/if}
            <div class="flex items-center gap-1 text-amber-400 text-xs font-bold ml-2">
              <Star class="w-4 h-4 fill-amber-400" />
              <span>{activeGame.averageRating}</span>
              <span class="text-slate-400 font-normal">({activeGame.reviewsCount} reviews)</span>
            </div>
          </div>
        </div>

        <!-- Pricing & Action Button -->
        <div class="flex items-center gap-4 mt-8">
          <div class="bg-slate-900/90 px-4 py-2 rounded-xl border border-slate-700">
            <span class="text-[10px] text-slate-400 block uppercase font-bold tracking-wider">Price</span>
            <span class="text-lg md:text-xl font-black text-cyan-400 font-mono">
              {formatPrice(activeGame.priceInNanoTons)}
            </span>
          </div>

          <button
            onclick={() => handleBuy(activeGame)}
            class="flex items-center gap-2 px-6 py-3.5 rounded-xl bg-gradient-to-r from-cyan-500 via-blue-600 to-indigo-600 hover:from-cyan-400 hover:to-blue-500 text-white font-extrabold text-sm tracking-wide shadow-lg shadow-cyan-500/25 hover:shadow-cyan-500/40 hover:scale-[1.02] active:scale-95 transition-all cursor-pointer"
          >
            <ShoppingCart class="w-4 h-4" />
            <span>Buy with TON</span>
          </button>

          <button
            onclick={() => gamesStore.selectGame(activeGame)}
            class="px-4 py-3.5 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-200 text-sm font-semibold border border-slate-700 transition-all cursor-pointer"
          >
            Details
          </button>
        </div>
      </div>

      <!-- Right Screenshots / Thumbnail Preview -->
      <div class="lg:col-span-5 hidden lg:flex flex-col gap-3">
        <div class="rounded-xl overflow-hidden shadow-2xl border border-slate-700/60 aspect-video">
          <img
            src={activeGame.coverImageUrl || activeGame.headerImageUrl}
            alt={activeGame.title}
            class="w-full h-full object-cover"
          />
        </div>

        <!-- Carousel navigation indicators -->
        <div class="flex items-center gap-2 justify-end mt-2">
          {#each featuredGames as game, idx}
            <button
              onclick={() => currentIndex = idx}
              class="h-2 rounded-full transition-all cursor-pointer {currentIndex === idx ? 'w-8 bg-cyan-400' : 'w-2 bg-slate-700 hover:bg-slate-600'}"
              aria-label="Slide {idx + 1}"
            ></button>
          {/each}
        </div>
      </div>
    </div>
  </div>
{/if}
