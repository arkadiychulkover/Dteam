<script lang="ts">
  import { onMount } from 'svelte';
  import { Search, X, Gamepad2, Loader2, Sparkles, Check } from 'lucide-svelte';
  import { gamesService } from '../../services/gamesService';
  import type { GameRecommendation } from '../../types';

  interface Props {
    isOpen: boolean;
    selectedGameId?: string | null;
    onSelect: (game: { id: string; title: string; bannerUrl?: string }) => void;
    onClose: () => void;
  }

  let {
    isOpen,
    selectedGameId = null,
    onSelect,
    onClose
  }: Props = $props();

  let searchQuery = $state('');
  let recommendations = $state<GameRecommendation[]>([]);
  let isLoading = $state(false);
  let debounceTimeout: any;

  async function fetchGames(query: string) {
    isLoading = true;
    try {
      const res = await gamesService.getRecommendations(query.trim(), 16);
      recommendations = res || [];
    } catch (err) {
      console.warn('[SelectGameModal] Помилка завантаження рекомендацій:', err);
      recommendations = [];
    } finally {
      isLoading = false;
    }
  }

  function handleInput(e: Event) {
    const val = (e.target as HTMLInputElement).value;
    searchQuery = val;
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(() => {
      fetchGames(val);
    }, 250);
  }

  function handleClear() {
    searchQuery = '';
    fetchGames('');
  }

  function handleSelect(game: GameRecommendation) {
    onSelect({
      id: game.id,
      title: game.title,
      bannerUrl: game.bannerUrl || game.banner || game.headerImageUrl || game.coverImageUrl || ''
    });
    onClose();
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (e.key === 'Escape') {
      onClose();
    }
  }

  $effect(() => {
    if (isOpen) {
      searchQuery = '';
      fetchGames('');
    }
  });

  onMount(() => {
    return () => clearTimeout(debounceTimeout);
  });
</script>

<svelte:window onkeydown={handleKeyDown} />

{#if isOpen}
  <div
    class="fixed inset-0 z-[120] bg-black/80 backdrop-blur-md flex items-center justify-center p-3 sm:p-5 animate-in fade-in duration-200"
    onclick={(e) => { if (e.target === e.currentTarget) onClose(); }}
    role="presentation"
  >
    <div
      class="bg-[#061e27] border border-cyan-500/30 rounded-3xl w-full max-w-2xl max-h-[88vh] flex flex-col shadow-[0_0_40px_rgba(6,182,212,0.15)] overflow-hidden"
      role="dialog"
      aria-modal="true"
      aria-labelledby="select-game-modal-title"
      tabindex="-1"
    >
      
      <!-- Header -->
      <div class="p-5 sm:p-6 border-b border-cyan-500/15 flex items-center justify-between bg-[#04151c]/70 shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-2xl bg-cyan-500/15 border border-cyan-400/30 flex items-center justify-center text-cyan-400">
            <Gamepad2 class="w-5 h-5" />
          </div>
          <div>
            <h2 id="select-game-modal-title" class="text-lg sm:text-xl font-black text-white tracking-wide font-display">
              Оберіть гру для публікації
            </h2>
            <p class="text-xs text-slate-400 mt-0.5">
              Пост буде підв'язано до сторінки обраної гри у спільноті
            </p>
          </div>
        </div>

        <button
          type="button"
          onclick={onClose}
          class="w-8 h-8 rounded-full bg-cyan-950/40 hover:bg-cyan-900/60 text-slate-400 hover:text-white flex items-center justify-center transition-colors cursor-pointer"
          title="Закрити"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <!-- Search Input Bar -->
      <div class="p-4 sm:p-6 border-b border-cyan-500/10 bg-[#061b23]/40 shrink-0">
        <div class="relative">
          <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
          <input
            type="text"
            value={searchQuery}
            oninput={handleInput}
            placeholder="Введіть назву гри для швидкого пошуку..."
            class="w-full bg-[#03131a] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(6,182,212,0.25)] rounded-2xl pl-11 pr-11 py-3 text-sm text-white placeholder-slate-500 focus:outline-none transition-all"
          />
          {#if searchQuery}
            <button
              type="button"
              onclick={handleClear}
              class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white p-1 rounded-full hover:bg-slate-800/60 transition-colors"
            >
              <X class="w-4 h-4" />
            </button>
          {/if}
        </div>
        <div class="flex items-center gap-1.5 mt-2.5 px-1 text-[11px] text-cyan-400/80 font-medium">
          <Sparkles class="w-3 h-3 text-cyan-300" />
          <span>{searchQuery.trim() ? 'Результати пошуку' : 'Рекомендовані та популярні ігри'}</span>
        </div>
      </div>

      <!-- Games List / Grid -->
      <div class="p-4 sm:p-6 overflow-y-auto custom-scrollbar flex-1 space-y-2.5">
        {#if isLoading}
          <div class="py-16 flex flex-col items-center justify-center gap-3 text-cyan-400">
            <Loader2 class="w-8 h-8 animate-spin" />
            <span class="text-xs text-slate-400">Пошук ігор...</span>
          </div>
        {:else if recommendations.length === 0}
          <div class="py-16 text-center text-slate-400 space-y-2">
            <Gamepad2 class="w-10 h-10 mx-auto text-slate-600 mb-1" />
            <p class="text-sm font-semibold text-slate-300">Ігор не знайдено</p>
            <p class="text-xs text-slate-500">
              {searchQuery ? `За запитом «${searchQuery}» нічого немає.` : 'Список доступних ігор наразі порожній.'}
            </p>
            {#if searchQuery}
              <button
                type="button"
                onclick={handleClear}
                class="mt-2 inline-flex items-center text-xs font-bold text-cyan-400 hover:text-cyan-300 underline cursor-pointer"
              >
                Скинути фільтр
              </button>
            {/if}
          </div>
        {:else}
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {#each recommendations as game (game.id)}
              {@const isSelected = selectedGameId && selectedGameId.toLowerCase() === game.id.toLowerCase()}
              {@const banner = game.bannerUrl || game.banner || game.headerImageUrl || game.coverImageUrl}
              <button
                type="button"
                onclick={() => handleSelect(game)}
                class="w-full flex items-center gap-3.5 p-2.5 sm:p-3 rounded-2xl border text-left transition-all duration-200 cursor-pointer group relative overflow-hidden
                  {isSelected
                    ? 'bg-cyan-950/60 border-cyan-400 shadow-[0_0_15px_rgba(6,182,212,0.3)] ring-1 ring-cyan-400/50'
                    : 'bg-[#04161e] border-cyan-900/40 hover:border-cyan-400/60 hover:bg-[#072430]'}"
              >
                <!-- Game Banner -->
                <div class="w-16 h-12 sm:w-20 sm:h-14 rounded-xl overflow-hidden shrink-0 bg-slate-900 border border-cyan-900/60 relative">
                  {#if banner}
                    <img
                      src={banner}
                      alt={game.title}
                      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  {:else}
                    <div class="w-full h-full bg-gradient-to-tr from-cyan-950 to-slate-900 flex items-center justify-center text-cyan-500">
                      <Gamepad2 class="w-6 h-6 opacity-60" />
                    </div>
                  {/if}
                </div>

                <!-- Info -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-center justify-between gap-1">
                    <span class="block text-sm font-bold text-white group-hover:text-cyan-300 transition-colors truncate">
                      {game.title}
                    </span>
                    {#if isSelected}
                      <span class="w-5 h-5 rounded-full bg-cyan-400 text-slate-950 flex items-center justify-center shrink-0">
                        <Check class="w-3 h-3 stroke-[3]" />
                      </span>
                    {/if}
                  </div>
                  <span class="block text-[11px] text-slate-400 truncate mt-0.5">
                    Натисніть для вибору
                  </span>
                </div>
              </button>
            {/each}
          </div>
        {/if}
      </div>

      <!-- Footer -->
      <div class="p-4 bg-[#04151c]/60 border-t border-cyan-500/15 flex items-center justify-between text-xs text-slate-400 shrink-0">
        <span>Знайдено: <strong class="text-white">{recommendations.length}</strong> ігор</span>
        <button
          type="button"
          onclick={onClose}
          class="px-4 py-1.5 rounded-xl border border-cyan-500/30 hover:border-cyan-400 text-slate-300 hover:text-white transition-colors cursor-pointer"
        >
          Скасувати
        </button>
      </div>

    </div>
  </div>
{/if}
