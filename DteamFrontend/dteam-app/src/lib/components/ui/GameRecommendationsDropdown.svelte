<script lang="ts">
  import type { GameRecommendation } from '../../types';
  import { Sparkles, Loader2, Search, ChevronRight } from 'lucide-svelte';

  interface Props {
    recommendations: GameRecommendation[];
    isOpen: boolean;
    isLoading: boolean;
    searchQuery: string;
    onSelect: (rec: GameRecommendation) => void;
    onViewAll: () => void;
  }

  let {
    recommendations,
    isOpen,
    isLoading,
    searchQuery,
    onSelect,
    onViewAll
  }: Props = $props();
</script>

{#if isOpen && searchQuery.trim().length > 0}
  <div
    class="absolute top-full left-0 w-full min-w-[280px] sm:min-w-[320px] mt-2 bg-[#061820]/95 backdrop-blur-xl border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/80 overflow-hidden z-50 animate-in fade-in slide-in-from-top-2 duration-150 text-left"
  >
    <div class="flex items-center justify-between px-3.5 py-2 border-b border-cyan-500/15 bg-cyan-950/40 text-[11px] font-semibold text-cyan-400">
      <div class="flex items-center gap-1.5">
        <Sparkles class="w-3.5 h-3.5 text-cyan-400" />
        <span>Рекомендації</span>
      </div>
      {#if !isLoading && recommendations.length > 0}
        <span class="text-[10px] text-slate-400 shrink-0 whitespace-nowrap">{recommendations.length} знайдено</span>
      {/if}
    </div>

    {#if isLoading}
      <div class="p-4 flex items-center justify-center gap-2 text-xs text-slate-400">
        <Loader2 class="w-4 h-4 animate-spin text-cyan-400" />
        <span>Пошук ігор...</span>
      </div>
    {:else if recommendations.length > 0}
      <div class="max-h-72 overflow-y-auto divide-y divide-cyan-500/10">
        {#each recommendations as rec}
          <button
            type="button"
            onclick={() => onSelect(rec)}
            class="w-full px-3.5 py-2.5 flex items-center gap-3 hover:bg-cyan-500/10 text-left transition-colors cursor-pointer group"
          >
            <img
              src={rec.bannerUrl || rec.headerImageUrl || rec.coverImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=200&auto=format&fit=crop&q=80'}
              alt={rec.title}
              class="w-12 h-8 rounded-lg object-cover bg-slate-900 border border-cyan-500/20 shrink-0 group-hover:scale-105 transition-transform"
            />
            <div class="flex-1 min-w-0">
              <div class="text-xs font-bold text-white group-hover:text-cyan-300 truncate transition-colors">
                {rec.title}
              </div>
              <div class="text-[10px] text-slate-400 flex items-center gap-1">
                <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 inline-block"></span>
                <span>Гра в Dteam</span>
              </div>
            </div>
            <ChevronRight class="w-3.5 h-3.5 text-slate-500 group-hover:text-cyan-400 group-hover:translate-x-0.5 transition-all shrink-0" />
          </button>
        {/each}
      </div>

      <div class="p-2 border-t border-cyan-500/15 bg-black/40">
        <button
          type="button"
          onclick={onViewAll}
          class="w-full py-1.5 px-3 rounded-xl text-center text-xs font-semibold text-cyan-300 hover:text-cyan-200 hover:bg-cyan-500/10 transition-colors cursor-pointer flex items-center justify-center gap-1.5"
        >
          <Search class="w-3 h-3 shrink-0" />
          <span class="truncate">Усі результати для «{searchQuery}»</span>
        </button>
      </div>
    {:else}
      <div class="p-4 text-center space-y-2">
        <p class="text-xs text-slate-400">Ігор на «<span class="text-cyan-300 font-semibold">{searchQuery}</span>» не знайдено</p>
        <button
          type="button"
          onclick={onViewAll}
          class="text-xs text-cyan-400 hover:underline cursor-pointer"
        >
          Шукати в Каталозі
        </button>
      </div>
    {/if}
  </div>
{/if}
