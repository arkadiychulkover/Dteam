<script lang="ts">
  import { gamesStore, filteredGames } from '../../stores/gamesStore';
  import { GENRES } from '../../utils/constants';
  import FeaturedCarousel from './FeaturedCarousel.svelte';
  import GameCard from './GameCard.svelte';
  import GameDetailsModal from './GameDetailsModal.svelte';
  import { Search, SlidersHorizontal, Gamepad2 } from 'lucide-svelte';
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6">
  <!-- Featured Hero Banner -->
  <FeaturedCarousel />

  <!-- Search & Category Filters -->
  <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
    <!-- Search Bar -->
    <div class="relative flex-1 max-w-md">
      <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
      <input
        type="text"
        placeholder="Search decentralized games, tags, publishers..."
        value={$gamesStore.searchQuery}
        oninput={(e) => gamesStore.setSearchQuery((e.target as HTMLInputElement).value)}
        class="w-full pl-10 pr-4 py-2.5 rounded-xl bg-slate-900/90 border border-slate-800 focus:border-cyan-500 focus:outline-none text-sm text-slate-100 placeholder-slate-500 transition-all shadow-inner"
      />
    </div>

    <!-- Category Chips -->
    <div class="flex items-center gap-1.5 overflow-x-auto pb-1 max-w-full">
      {#each GENRES as category}
        <button
          onclick={() => gamesStore.setCategory(category)}
          class="px-3 py-1.5 rounded-xl text-xs font-semibold whitespace-nowrap transition-all cursor-pointer
            {$gamesStore.selectedCategory === category
              ? 'bg-cyan-500 text-black shadow-md shadow-cyan-500/20'
              : 'bg-slate-900/80 hover:bg-slate-800 text-slate-400 hover:text-slate-200 border border-slate-800'}"
        >
          {category}
        </button>
      {/each}
    </div>
  </div>

  <!-- Games Grid -->
  <div>
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-xl font-bold text-white tracking-wide flex items-center gap-2">
        <span>Popular Games</span>
        <span class="text-xs px-2 py-0.5 rounded-full bg-slate-800 text-slate-400 font-normal">
          {$filteredGames.length}
        </span>
      </h3>
    </div>

    {#if $filteredGames.length > 0}
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {#each $filteredGames as game (game.id)}
          <GameCard {game} />
        {/each}
      </div>
    {:else}
      <div class="text-center py-16 bg-slate-900/40 rounded-2xl border border-slate-800">
        <Gamepad2 class="w-12 h-12 text-slate-600 mx-auto mb-3" />
        <h4 class="text-lg font-bold text-slate-300">No games found</h4>
        <p class="text-xs text-slate-500 mt-1">Try adjusting your search query or selected genre filter.</p>
      </div>
    {/if}
  </div>

  <!-- Game Details Modal -->
  <GameDetailsModal />
</div>
