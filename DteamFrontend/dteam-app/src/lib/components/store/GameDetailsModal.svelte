<script lang="ts">
  import { gamesStore } from '../../stores/gamesStore';
  import { libraryStore } from '../../stores/libraryStore';
  import { formatPrice, formatBytes } from '../../utils/formatters';
  import { X, Star, ShoppingCart, Download, Check, MessageSquare, ThumbsUp, HardDrive } from 'lucide-svelte';

  const game = $derived($gamesStore.selectedGame);
  const isOwned = $derived(
    game ? $libraryStore.items.some(i => i.gameId === game.id) : false
  );

  let selectedScreenshot = $state<string | null>(null);
  let newReviewRating = $state(5);
  let newReviewContent = $state('');

  function close() {
    gamesStore.selectGame(null);
  }
</script>

{#if game}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-fade-in">
    <div class="relative w-full max-w-4xl max-h-[90vh] overflow-y-auto bg-[#10131f] border border-slate-700/80 rounded-2xl shadow-2xl p-6 md:p-8">
      <!-- Close Button -->
      <button
        onclick={close}
        class="absolute top-4 right-4 p-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <!-- Main Header -->
      <div class="grid grid-cols-1 md:grid-cols-12 gap-6">
        <!-- Left Banner / Media -->
        <div class="md:col-span-7 flex flex-col gap-3">
          <div class="rounded-xl overflow-hidden aspect-video bg-black border border-slate-800">
            <img
              src={selectedScreenshot || game.headerImageUrl || game.coverImageUrl || ''}
              alt={game.title}
              class="w-full h-full object-cover"
            />
          </div>

          <!-- Screenshots thumbnail gallery -->
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

        <!-- Right Quick Info & Buy Panel -->
        <div class="md:col-span-5 flex flex-col justify-between">
          <div>
            <h2 class="text-2xl md:text-3xl font-black text-white font-['Outfit']">
              {game.title}
            </h2>

            <p class="text-xs text-slate-300 mt-2 leading-relaxed">
              {game.description}
            </p>

            <!-- Metadata specs -->
            <div class="mt-4 space-y-2 text-xs border-t border-b border-slate-800 py-3">
              <div class="flex justify-between text-slate-400">
                <span>Rating:</span>
                <span class="text-amber-400 font-bold flex items-center gap-1">
                  <Star class="w-3.5 h-3.5 fill-amber-400" /> {game.averageRating} ({game.reviewsCount} reviews)
                </span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Version:</span>
                <span class="text-slate-200 font-mono">{game.version}</span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Storage Size:</span>
                <span class="text-slate-200">{formatBytes(game.sizeInBytes)}</span>
              </div>
              <div class="flex justify-between text-slate-400">
                <span>Downloads:</span>
                <span class="text-slate-200 font-mono">{game.downloadCount.toLocaleString()}</span>
              </div>
            </div>
          </div>

          <!-- Pricing & Purchase Button -->
          <div class="mt-6 pt-4 bg-slate-900/80 p-4 rounded-xl border border-slate-800">
            <div class="flex items-baseline justify-between mb-3">
              <span class="text-xs text-slate-400 font-semibold uppercase">Total Price</span>
              <span class="text-xl font-black text-cyan-400 font-mono">
                {formatPrice(game.priceInNanoTons)}
              </span>
            </div>

            {#if isOwned}
              <button
                disabled
                class="w-full py-3 rounded-xl bg-emerald-600/30 text-emerald-300 font-bold border border-emerald-500/40 flex items-center justify-center gap-2"
              >
                <Check class="w-4 h-4" /> Already in Library
              </button>
            {:else}
              <button
                onclick={() => { libraryStore.buyGame(game); }}
                class="w-full py-3.5 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-extrabold text-sm tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2"
              >
                <ShoppingCart class="w-4 h-4" />
                <span>Buy Game with TON</span>
              </button>
            {/if}
          </div>
        </div>
      </div>
    </div>
  </div>
{/if}
