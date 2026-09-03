<script lang="ts">
  import type { CatalogFilterState } from '../../stores/gamesStore';
  import { Sparkles, Gamepad2, Users, Monitor, Flame, Tag, ChevronRight, X } from 'lucide-svelte';

  interface Props {
    isOpen: boolean;
    onSelectFilter: (filter: Partial<CatalogFilterState>) => void;
    onClose?: () => void;
  }

  let { isOpen, onSelectFilter, onClose }: Props = $props();

  const genreColumn1 = [
    { label: 'Рольові (RPG)', filter: { genre: 'RPG' } },
    { label: 'Бойовики та Екшн', filter: { genre: 'Action' } },
    { label: 'Стратегії', filter: { genre: 'Strategy' } },
    { label: 'Пригоди', filter: { genre: 'Adventure' } },
    { label: 'Стрілянки (Shooter)', filter: { genre: 'Shooter' } },
    { label: 'Інді-ігри', filter: { genre: 'Indie' } },
    { label: 'Симулятори', filter: { genre: 'Simulation' } },
    { label: 'Горори', filter: { genre: 'Horror' } },
    { label: 'Спорт і перегони', filter: { genre: 'Sports' } },
    { label: 'MMO', filter: { genre: 'MMO' } },
  ];

  const genreColumn2 = [
    { label: 'Кіберпанк (Cyberpunk)', filter: { tagQuery: 'кіберпанк' } },
    { label: 'Наукова фантастика', filter: { genre: 'SciFi' } },
    { label: 'Відкритий світ', filter: { tagQuery: 'відкритий світ' } },
    { label: 'Глибокий сюжет', filter: { tagQuery: 'сюжет' } },
    { label: 'Військові стратегії', filter: { tagQuery: 'військові' } },
    { label: 'Карткові й настільні', filter: { tagQuery: 'карткові' } },
    { label: 'Містобудівельні ігри', filter: { tagQuery: 'містобудування' } },
    { label: 'Покрокові стратегії', filter: { tagQuery: 'покрокові' } },
    { label: 'Виживання', filter: { tagQuery: 'виживання' } },
    { label: 'Головоломки', filter: { tagQuery: 'головоломки' } },
  ];

  const usersCategory = [
    { label: 'Однокористувацькі ігри', filter: { feature: 'SinglePlayer' } },
    { label: 'Багатокористувацькі', filter: { tagQuery: 'Multiplayer' } },
    { label: 'Кооперативні (Co-op)', filter: { tagQuery: 'coop' } },
    { label: 'Хмарні збереження', filter: { feature: 'CloudSaves' } },
    { label: 'Досягнення Dteam', filter: { feature: 'Achievements' } },
    { label: 'Базові ігри', filter: { isDlc: false } },
    { label: 'DLC та Доповнення', filter: { isDlc: true } },
  ];

  const platformCategory = [
    { label: 'Windows', filter: { platform: 'Windows' } },
    { label: 'MacOS', filter: { platform: 'MacOS' } },
    { label: 'Linux', filter: { platform: 'Linux' } },
  ];

  const specialCategory: { label: string; filter: Partial<CatalogFilterState> }[] = [
    { label: 'Знижки та акції', filter: { isDiscounted: true } },
    { label: 'Безкоштовно', filter: { priceFilter: 'free' } },
    { label: 'До 1 TON', filter: { priceFilter: 'under1' } },
    { label: 'До 3 TON', filter: { priceFilter: 'under3' } },
    { label: 'До 6 TON', filter: { priceFilter: 'under6' } },
  ];
</script>

{#if isOpen}
  <div
    class="absolute top-full left-1/2 -translate-x-1/2 mt-3 w-[calc(100vw-2rem)] max-w-5xl bg-[#041a22]/98 backdrop-blur-2xl border border-cyan-500/35 rounded-3xl shadow-[0_25px_60px_rgba(0,0,0,0.85),0_0_35px_rgba(6,182,212,0.18)] p-6 sm:p-7 z-50 animate-in fade-in zoom-in-95 duration-150 text-left cursor-default select-none before:content-[''] before:absolute before:-top-4 before:left-0 before:right-0 before:h-4 relative"
  >
    {#if onClose}
      <button
        type="button"
        onclick={onClose}
        class="absolute top-4 right-4 p-1.5 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800/60 transition-colors cursor-pointer"
        title="Закрити"
      >
        <X class="w-4 h-4" />
      </button>
    {/if}
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-6 sm:gap-7">
      <div class="space-y-3">
        <div class="flex items-center gap-1.5 pb-2 border-b border-cyan-500/20 text-white font-black text-sm tracking-wide">
          <Gamepad2 class="w-4 h-4 text-cyan-400 shrink-0" />
          <span>Жанри</span>
        </div>
        <ul class="space-y-1.5">
          {#each genreColumn1 as item}
            <li>
              <button
                type="button"
                onclick={() => onSelectFilter(item.filter)}
                class="w-full text-left text-xs font-medium text-slate-300 hover:text-cyan-300 hover:translate-x-1 transition-all py-0.5 cursor-pointer flex items-center justify-between group"
              >
                <span class="group-hover:text-white transition-colors">{item.label}</span>
                <ChevronRight class="w-3 h-3 opacity-0 group-hover:opacity-100 text-cyan-400 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </button>
            </li>
          {/each}
        </ul>
      </div>

      <div class="space-y-3">
        <div class="flex items-center gap-1.5 pb-2 border-b border-cyan-500/20 text-white font-black text-sm tracking-wide">
          <Tag class="w-4 h-4 text-cyan-400 shrink-0" />
          <span>Категорії</span>
        </div>
        <ul class="space-y-1.5">
          {#each genreColumn2 as item}
            <li>
              <button
                type="button"
                onclick={() => onSelectFilter(item.filter)}
                class="w-full text-left text-xs font-medium text-slate-300 hover:text-cyan-300 hover:translate-x-1 transition-all py-0.5 cursor-pointer flex items-center justify-between group"
              >
                <span class="group-hover:text-white transition-colors">{item.label}</span>
                <ChevronRight class="w-3 h-3 opacity-0 group-hover:opacity-100 text-cyan-400 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </button>
            </li>
          {/each}
        </ul>
      </div>

      <div class="space-y-3">
        <div class="flex items-center gap-1.5 pb-2 border-b border-cyan-500/20 text-white font-black text-sm tracking-wide">
          <Users class="w-4 h-4 text-cyan-400 shrink-0" />
          <span>Користувачі</span>
        </div>
        <ul class="space-y-1.5">
          {#each usersCategory as item}
            <li>
              <button
                type="button"
                onclick={() => onSelectFilter(item.filter)}
                class="w-full text-left text-xs font-medium text-slate-300 hover:text-cyan-300 hover:translate-x-1 transition-all py-0.5 cursor-pointer flex items-center justify-between group"
              >
                <span class="group-hover:text-white transition-colors">{item.label}</span>
                <ChevronRight class="w-3 h-3 opacity-0 group-hover:opacity-100 text-cyan-400 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </button>
            </li>
          {/each}
        </ul>
      </div>

      <div class="space-y-3">
        <div class="flex items-center gap-1.5 pb-2 border-b border-cyan-500/20 text-white font-black text-sm tracking-wide">
          <Monitor class="w-4 h-4 text-cyan-400 shrink-0" />
          <span>Платформа</span>
        </div>
        <ul class="space-y-1.5">
          {#each platformCategory as item}
            <li>
              <button
                type="button"
                onclick={() => onSelectFilter(item.filter)}
                class="w-full text-left text-xs font-medium text-slate-300 hover:text-cyan-300 hover:translate-x-1 transition-all py-0.5 cursor-pointer flex items-center justify-between group"
              >
                <span class="group-hover:text-white transition-colors">{item.label}</span>
                <ChevronRight class="w-3 h-3 opacity-0 group-hover:opacity-100 text-cyan-400 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </button>
            </li>
          {/each}
        </ul>
      </div>

      <div class="space-y-3">
        <div class="flex items-center gap-1.5 pb-2 border-b border-cyan-500/20 text-white font-black text-sm tracking-wide">
          <Flame class="w-4 h-4 text-amber-400 shrink-0" />
          <span>Ціна та акції</span>
        </div>
        <ul class="space-y-1.5">
          {#each specialCategory as item}
            <li>
              <button
                type="button"
                onclick={() => onSelectFilter(item.filter)}
                class="w-full text-left text-xs font-medium text-slate-300 hover:text-cyan-300 hover:translate-x-1 transition-all py-0.5 cursor-pointer flex items-center justify-between group"
              >
                <span class="group-hover:text-white transition-colors">{item.label}</span>
                <ChevronRight class="w-3 h-3 opacity-0 group-hover:opacity-100 text-cyan-400 -translate-x-1 group-hover:translate-x-0 transition-all" />
              </button>
            </li>
          {/each}
        </ul>
      </div>
    </div>

    <div class="mt-6 pt-4 border-t border-cyan-500/20 flex items-center justify-between text-xs text-slate-400">
      <span class="flex items-center gap-1.5">
        <Sparkles class="w-3.5 h-3.5 text-cyan-400" />
        <span>Оберіть категорію для швидкого переходу в каталог</span>
      </span>
      <button
        type="button"
        onclick={() => onSelectFilter({})}
        class="text-cyan-400 hover:text-cyan-300 font-bold hover:underline cursor-pointer flex items-center gap-1"
      >
        <span>Відкрити весь каталог</span>
        <ChevronRight class="w-3.5 h-3.5" />
      </button>
    </div>
  </div>
{/if}
