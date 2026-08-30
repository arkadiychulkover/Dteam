<script lang="ts">
  import { onMount } from 'svelte';
  import { developerStore } from '../../stores/developerStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { formatTon, nanoTonToTon, formatDate } from '../../utils/formatters';
  import TonIcon from '../ui/TonIcon.svelte';
  import type { Game } from '../../types';
  import {
    Gamepad2,
    Plus,
    RefreshCw,
    Download,
    Star,
    Coins,
    Layers,
    Edit3,
    Eye,
    EyeOff,
    Trash2,
    Search,
    ExternalLink,
    Clock,
    AlertCircle,
    Calendar,
    ArrowUpRight,
    Sparkles,
    FileArchive,
    Newspaper
  } from 'lucide-svelte';

  import { currentUser } from '../../stores/authStore';
  import CreateGameNewsModal from './CreateGameNewsModal.svelte';
  import { developerService } from '../../services/developerService';
  import type { CommunityPost } from '../../services/communityService';

  let searchQuery = $state('');
  let filterStatus = $state<'all' | 'published' | 'draft'>('all');
  let gameToDelete = $state<Game | null>(null);
  let gameForNews = $state<Game | null>(null);
  let myNewsList = $state<CommunityPost[]>([]);
  let isLoadingMyNews = $state(false);
  let activeDevTab = $state<'games' | 'news'>('games');

  // Tooltip hover states for charts
  let hoveredPoint = $state<{ date: string; value: string; type: 'revenue' | 'downloads' } | null>(null);

  async function loadMyNews() {
    isLoadingMyNews = true;
    try {
      myNewsList = await developerService.getMyNews();
    } catch (e) {
      console.warn('[DeveloperView] Не вдалося завантажити новини:', e);
      myNewsList = [];
    } finally {
      isLoadingMyNews = false;
    }
  }

  async function handleDeleteNews(newsId: string) {
    try {
      await developerService.deleteGameNews(newsId);
      uiStore.addToast({ title: 'Видалено', message: 'Новину успішно видалено.', type: 'info' });
      await loadMyNews();
    } catch (err: any) {
      uiStore.addToast({ title: 'Помилка', message: err?.message || 'Не вдалося видалити новину.', type: 'error' });
    }
  }

  onMount(() => {
    if (!$currentUser?.id) {
      uiStore.setLoginModal(true);
      uiStore.setTab('store');
      return;
    }
    developerStore.loadAll();
    loadMyNews();
  });

  const filteredGames = $derived(
    $developerStore.games.filter((game) => {
      const matchesSearch =
        !searchQuery.trim() ||
        game.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        game.genres?.some((g) => g.toLowerCase().includes(searchQuery.toLowerCase())) ||
        game.tags?.some((t) => t.toLowerCase().includes(searchQuery.toLowerCase()));

      const matchesStatus =
        filterStatus === 'all' ||
        (filterStatus === 'published' && game.isPublished) ||
        (filterStatus === 'draft' && !game.isPublished);

      return matchesSearch && matchesStatus;
    })
  );

  // Daily points for charts
  const dailyPoints = $derived($developerStore.stats?.dailyDynamics || []);
  const maxDownloadsInChart = $derived(
    Math.max(2, ...dailyPoints.map((p) => p.downloads))
  );
  const maxEarningsInChart = $derived(
    Math.max(1, ...dailyPoints.map((p) => Number(p.earningsInTon) || 0))
  );

  // Calculated smooth cubic Bezier spline for Revenue Area Chart (width: 520, height: 110)
  // Base at y=90, top grid line at y=20 (height = 70). Max value maps to y=20.
  const revenueChartData = $derived.by(() => {
    if (dailyPoints.length === 0) return { pathD: '', areaD: '', points: [] };
    const w = 470;
    const h = 70;
    const offsetX = 35;
    const baseY = 90;
    const maxVal = maxEarningsInChart;

    const coords = dailyPoints.map((pt, i) => {
      const x = offsetX + (i / Math.max(1, dailyPoints.length - 1)) * w;
      const earn = Number(pt.earningsInTon) || 0;
      const y = baseY - (earn / maxVal) * h;
      return { x, y, pt };
    });

    if (coords.length === 1) {
      return {
        pathD: `M ${coords[0].x.toFixed(1)},${coords[0].y.toFixed(1)}`,
        areaD: `M ${coords[0].x.toFixed(1)},${coords[0].y.toFixed(1)} L ${coords[0].x.toFixed(1)},${baseY} Z`,
        points: coords
      };
    }

    let pathD = `M ${coords[0].x.toFixed(1)},${coords[0].y.toFixed(1)}`;
    const tension = 0.28;

    for (let i = 0; i < coords.length - 1; i++) {
      const pPrev = coords[Math.max(0, i - 1)];
      const pCurr = coords[i];
      const pNext = coords[i + 1];
      const pAfter = coords[Math.min(coords.length - 1, i + 2)];

      let cp1x = pCurr.x + (pNext.x - pPrev.x) * tension;
      let cp1y = pCurr.y + (pNext.y - pPrev.y) * tension;

      let cp2x = pNext.x - (pAfter.x - pCurr.x) * tension;
      let cp2y = pNext.y - (pAfter.y - pCurr.y) * tension;

      // Keep baseline flat when both adjacent points are 0
      if (pCurr.y === baseY && pNext.y === baseY) {
        cp1y = baseY;
        cp2y = baseY;
      } else {
        // Clamp control points gracefully
        cp1y = Math.min(baseY, Math.max(16, cp1y));
        cp2y = Math.min(baseY, Math.max(16, cp2y));
      }

      pathD += ` C ${cp1x.toFixed(1)},${cp1y.toFixed(1)} ${cp2x.toFixed(1)},${cp2y.toFixed(1)} ${pNext.x.toFixed(1)},${pNext.y.toFixed(1)}`;
    }

    const lastX = coords[coords.length - 1]?.x.toFixed(1) || offsetX;
    const firstX = coords[0]?.x.toFixed(1) || offsetX;
    const areaD = `${pathD} L ${lastX},${baseY} L ${firstX},${baseY} Z`;

    return { pathD, areaD, points: coords };
  });

  function handleViewInStore(game: Game) {
    gamesStore.selectGame(game.id);
    uiStore.setTab('game');
  }

  function handleTogglePublish(game: Game) {
    if (!game.isPublished && !game.serverArchivePath) {
      uiStore.addToast({
        title: 'Архів гри не знайдено',
        message: 'Для публікації в каталозі необхідно спочатку додати файл білду гри. Натисніть «Ред.» та оберіть архів.',
        type: 'warning',
      });
      return;
    }
    developerStore.togglePublishStatus(game.id);
  }

  async function confirmDelete() {
    if (gameToDelete) {
      await developerStore.deleteGame(gameToDelete.id);
      gameToDelete = null;
    }
  }
</script>

<div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 animate-in fade-in duration-300">
  <!-- LEVEL 1: Clean Page Header with Primary CTA -->
  <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-2">
    <div>
      <h1 class="text-2xl sm:text-3xl font-extrabold text-white tracking-tight font-display">
        Кабінет розробника
      </h1>
      <p class="text-xs text-slate-400 mt-1">
        Показники студії, дохід у TON та керування ігровими проектами
      </p>
    </div>

    <!-- Primary Action: + New Game (Elevated, primary focus) -->
    <div class="flex items-center gap-3">
      <button
        onclick={() => developerStore.loadAll()}
        class="p-2.5 rounded-xl bg-[#061923] border border-cyan-500/15 hover:border-cyan-500/30 text-slate-400 hover:text-white transition-all cursor-pointer"
        title="Оновити дані"
      >
        <RefreshCw class="w-4 h-4 {$developerStore.isLoading ? 'animate-spin text-cyan-400' : ''}" />
      </button>

      <button
        onclick={() => uiStore.setPublishGameModal(true)}
        class="px-4 py-2.5 rounded-xl bg-[#0df2c9] hover:bg-[#20f8d0] text-black font-extrabold text-xs tracking-wide shadow-lg shadow-cyan-500/15 transition-all flex items-center gap-1.5 cursor-pointer active:scale-95"
      >
        <Plus class="w-4 h-4 stroke-[3]" />
        <span>Створити нову гру</span>
      </button>
    </div>
  </div>

  <!-- LEVEL 2: OVERVIEW — One unified analytics surface (No card soup!) -->
  <div class="rounded-2xl bg-[#061923] border border-cyan-500/15 overflow-hidden shadow-lg">
    <div class="grid grid-cols-2 lg:grid-cols-4 divide-y lg:divide-y-0 lg:divide-x divide-cyan-500/10">
      <!-- Col 1: Revenue -->
      <div class="p-5 flex flex-col justify-between space-y-2">
        <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
          Зароблено TON
        </span>
        <div class="text-2xl font-bold font-mono text-white flex items-center gap-1.5">
          <TonIcon class="w-5 h-5 text-[#0df2c9] shrink-0" />
          <span>{formatTon(nanoTonToTon($developerStore.stats?.totalEarningsInNanoTons || 0))}</span>
        </div>
        <div class="flex items-center gap-1 text-[11px] text-emerald-400 font-medium">
          <ArrowUpRight class="w-3.5 h-3.5" />
          <span>{formatTon(nanoTonToTon($developerStore.stats?.earningsLast30DaysInNanoTons || 0))} за 30 днів</span>
        </div>
      </div>

      <!-- Col 2: Downloads -->
      <div class="p-5 flex flex-col justify-between space-y-2">
        <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
          Завантажень
        </span>
        <div class="text-2xl font-bold font-mono text-white">
          {($developerStore.stats?.totalDownloads || 0).toLocaleString()}
        </div>
        <div class="text-[11px] text-slate-500">
          {$developerStore.stats?.downloadsLast30Days || 0} за останній місяць
        </div>
      </div>

      <!-- Col 3: Games Count -->
      <div class="p-5 flex flex-col justify-between space-y-2">
        <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
          Проектів
        </span>
        <div class="text-2xl font-bold font-mono text-white">
          {$developerStore.games.length}
        </div>
        <div class="text-[11px] text-cyan-400/90 font-medium">
          {$developerStore.games.filter(g => g.isPublished).length} опубліковано у каталозі
        </div>
      </div>

      <!-- Col 4: Developer Share -->
      <div class="p-5 flex flex-col justify-between space-y-2">
        <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
          Ваша частка
        </span>
        <div class="text-2xl font-bold font-mono text-[#0df2c9]">
          95%
        </div>
        <div class="text-[11px] text-slate-500">
          5% фіксована комісія мережі
        </div>
      </div>
    </div>
  </div>

  <!-- LEVEL 2: ANALYTICS — Smooth, bold, organic metrics -->
  <div class="space-y-3">
    <div class="flex items-center justify-between">
      <h2 class="text-xs font-bold uppercase tracking-wider text-slate-400">
        Динаміка за 30 днів
      </h2>
      <div class="flex items-center gap-1.5 text-[11px] text-slate-400 bg-[#061923] border border-cyan-500/15 px-2.5 py-1 rounded-lg">
        <Calendar class="w-3 h-3 text-cyan-400" />
        <span>Останні 30 днів</span>
      </div>
    </div>

    <!-- Side-by-Side Charts (Desktop 7:5 ratio) -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-4">
      <!-- Chart 1: Revenue Area Chart (7 cols) -->
      <div class="lg:col-span-7 rounded-2xl bg-[#061923] border border-cyan-500/15 p-5 shadow-lg relative flex flex-col justify-between overflow-hidden">
        <div class="flex items-center justify-between mb-2 h-7">
          <div>
            <span class="text-xs font-bold text-slate-200 block">Дохід у TON</span>
            <span class="text-[10px] text-slate-500">щоденні надходження</span>
          </div>

          <!-- Dynamic Header Value (zero layout shift) -->
          {#if hoveredPoint && hoveredPoint.type === 'revenue'}
            <span class="text-xs font-mono text-cyan-300 flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-cyan-500/10 border border-cyan-500/25 animate-in fade-in duration-150">
              <span class="text-slate-400">{hoveredPoint.date}:</span>
              <TonIcon class="w-3.5 h-3.5 text-[#0df2c9] shrink-0" />
              <strong class="text-[#0df2c9]">{hoveredPoint.value}</strong>
            </span>
          {:else}
            <span class="text-xs font-mono font-bold text-[#0df2c9] flex items-center gap-1">
              <TonIcon class="w-3.5 h-3.5 text-[#0df2c9] shrink-0" />
              <span>{formatTon(nanoTonToTon($developerStore.stats?.earningsLast30DaysInNanoTons || $developerStore.stats?.totalEarningsInNanoTons || 0))}</span>
            </span>
          {/if}
        </div>

        <!-- SVG Area Chart with smooth bezier spline & thick glowing stroke -->
        <div class="w-full relative pt-2">
          <svg viewBox="0 0 520 110" class="w-full h-32 overflow-visible">
            <defs>
              <linearGradient id="revenueAreaGrad" x1="0%" y1="0%" x2="0%" y2="100%">
                <stop offset="0%" stop-color="#0df2c9" stop-opacity="0.35" />
                <stop offset="50%" stop-color="#0284c7" stop-opacity="0.12" />
                <stop offset="100%" stop-color="#0284c7" stop-opacity="0.0" />
              </linearGradient>
              <filter id="neonCurveGlow" x="-20%" y="-20%" width="140%" height="140%">
                <feDropShadow dx="0" dy="2" stdDeviation="3.5" flood-color="#0df2c9" flood-opacity="0.5" />
              </filter>
            </defs>

            <!-- Horizontal Baseline Grid: top=20, middle=55, bottom=90 -->
            <line x1="35" y1="20" x2="505" y2="20" stroke="#16435c" stroke-dasharray="4 4" stroke-width="1" />
            <line x1="35" y1="55" x2="505" y2="55" stroke="#16435c" stroke-dasharray="4 4" stroke-width="1" />
            <line x1="35" y1="90" x2="505" y2="90" stroke="#16435c" stroke-width="1" />

            <!-- Y Axis values: top=20, middle=55, bottom=90 -->
            <text x="8" y="24" fill="#94a3b8" font-size="10" font-family="monospace">{maxEarningsInChart.toFixed(1)}</text>
            <text x="8" y="59" fill="#94a3b8" font-size="10" font-family="monospace">{(maxEarningsInChart / 2).toFixed(1)}</text>
            <text x="16" y="94" fill="#94a3b8" font-size="10" font-family="monospace">0</text>

            <!-- Smooth Area Fill -->
            {#if revenueChartData.areaD}
              <path d={revenueChartData.areaD} fill="url(#revenueAreaGrad)" />
            {/if}

            <!-- Thick, Smooth Neon Spline Line -->
            {#if revenueChartData.pathD}
              <path
                d={revenueChartData.pathD}
                fill="none"
                stroke="#0df2c9"
                stroke-width="3.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                filter="url(#neonCurveGlow)"
              />
            {/if}

            <!-- Data point glowing nodes & hover hit-areas -->
            {#each revenueChartData.points as pt, i}
              {@const earn = Number(pt.pt.earningsInTon) || 0}
              {#if earn > 0}
                <!-- Outer glowing halo -->
                <circle cx={pt.x} cy={pt.y} r="7" fill="#0df2c9" fill-opacity="0.25" />
                <!-- Core solid dot -->
                <circle cx={pt.x} cy={pt.y} r="4" fill="#0df2c9" stroke="#061923" stroke-width="2" />
              {/if}

              <!-- Interactive hover column -->
              <rect
                role="presentation"
                x={pt.x - 7}
                y="15"
                width="14"
                height="80"
                fill="transparent"
                class="cursor-pointer hover:fill-cyan-400/10 transition-colors"
                onmouseenter={() => hoveredPoint = { date: pt.pt.date, value: `${earn.toFixed(2)} TON`, type: 'revenue' }}
                onmouseleave={() => hoveredPoint = null}
              />

              <!-- Date labels on axis with precise alignment (start, middle, end) -->
              {#if i === 0}
                <text x={pt.x} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="start" font-weight="500">
                  {pt.pt.date}
                </text>
              {:else if i === Math.floor(revenueChartData.points.length / 2)}
                <text x={pt.x} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="middle" font-weight="500">
                  {pt.pt.date}
                </text>
              {:else if i === revenueChartData.points.length - 1}
                <text x={pt.x} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="end" font-weight="500">
                  {pt.pt.date}
                </text>
              {/if}
            {/each}
          </svg>
        </div>
      </div>

      <!-- Chart 2: Downloads Bar Chart with chunky, solid pill bars -->
      <div class="lg:col-span-5 rounded-2xl bg-[#061923] border border-cyan-500/15 p-5 shadow-lg relative flex flex-col justify-between overflow-hidden">
        <div class="flex items-center justify-between mb-2 h-7">
          <div>
            <span class="text-xs font-bold text-slate-200 block">Завантаження</span>
            <span class="text-[10px] text-slate-500">динаміка інсталяцій</span>
          </div>

          <!-- Dynamic Header Value (zero layout shift) -->
          {#if hoveredPoint && hoveredPoint.type === 'downloads'}
            <span class="text-xs font-mono text-cyan-300 flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-cyan-500/10 border border-cyan-500/25 animate-in fade-in duration-150">
              <span class="text-slate-400">{hoveredPoint.date}:</span>
              <Download class="w-3.5 h-3.5 text-cyan-400 shrink-0" />
              <strong class="text-cyan-300">{hoveredPoint.value}</strong>
            </span>
          {:else}
            <span class="text-xs font-mono font-bold text-cyan-300 flex items-center gap-1">
              <Download class="w-3.5 h-3.5 text-cyan-400 shrink-0" />
              <span>{($developerStore.stats?.downloadsLast30Days || 0).toLocaleString()}</span>
            </span>
          {/if}
        </div>

        <!-- SVG Bar Chart with thick, chunky pill bars -->
        <div class="w-full relative pt-2">
          <svg viewBox="0 0 340 110" class="w-full h-32 overflow-visible">
            <defs>
              <linearGradient id="barActiveGrad" x1="0%" y1="0%" x2="0%" y2="100%">
                <stop offset="0%" stop-color="#0df2c9" />
                <stop offset="100%" stop-color="#0284c7" />
              </linearGradient>
              <filter id="barGlow" x="-20%" y="-20%" width="140%" height="140%">
                <feDropShadow dx="0" dy="2" stdDeviation="3" flood-color="#0df2c9" flood-opacity="0.4" />
              </filter>
            </defs>

            <!-- Horizontal Baseline Grid: top=20, middle=55, bottom=90 -->
            <line x1="25" y1="20" x2="325" y2="20" stroke="#16435c" stroke-dasharray="4 4" stroke-width="1" />
            <line x1="25" y1="55" x2="325" y2="55" stroke="#16435c" stroke-dasharray="4 4" stroke-width="1" />
            <line x1="25" y1="90" x2="325" y2="90" stroke="#16435c" stroke-width="1" />

            <!-- Y Axis values with clean contrast -->
            <text x="8" y="24" fill="#94a3b8" font-size="10" font-family="monospace">{maxDownloadsInChart}</text>
            <text x="8" y="59" fill="#94a3b8" font-size="10" font-family="monospace">{Math.round(maxDownloadsInChart / 2)}</text>
            <text x="8" y="94" fill="#94a3b8" font-size="10" font-family="monospace">0</text>

            <!-- Chunky Bars (width 8.5px, rx 3px) -->
            {#each dailyPoints as pt, i}
              {@const barH = pt.downloads > 0 ? Math.max(16, (pt.downloads / maxDownloadsInChart) * 70) : 5}
              {@const barX = 25 + i * 10}
              {@const barY = 90 - barH}

              <rect
                role="presentation"
                x={barX}
                y={barY}
                width="8.5"
                height={barH}
                rx={pt.downloads > 0 ? "3" : "2"}
                fill={pt.downloads > 0 ? 'url(#barActiveGrad)' : '#10384a'}
                filter={pt.downloads > 0 ? 'url(#barGlow)' : undefined}
                class="cursor-pointer transition-all hover:brightness-125"
                onmouseenter={() => hoveredPoint = { date: pt.date, value: `${pt.downloads} завантажень`, type: 'downloads' }}
                onmouseleave={() => hoveredPoint = null}
              />

              <!-- Date labels on axis (start, middle, end) -->
              {#if i === 0}
                <text x={barX} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="start" font-weight="500">
                  {pt.date}
                </text>
              {:else if i === Math.floor(dailyPoints.length / 2)}
                <text x={barX + 4} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="middle" font-weight="500">
                  {pt.date}
                </text>
              {:else if i === dailyPoints.length - 1}
                <text x={barX + 8} y="106" fill="#94a3b8" font-size="10" font-family="monospace" text-anchor="end" font-weight="500">
                  {pt.date}
                </text>
              {/if}
            {/each}
          </svg>
        </div>
      </div>
    </div>
  </div>

  <!-- LEVEL 2: MY GAMES & NEWS -->
  <div class="space-y-4 pt-2 border-t border-cyan-500/10">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div class="flex items-center gap-4 border-b border-cyan-500/15 pb-1">
        <button
          type="button"
          onclick={() => activeDevTab = 'games'}
          class="text-lg font-bold tracking-wide flex items-center gap-2 pb-1 border-b-2 transition-all cursor-pointer {activeDevTab === 'games' ? 'text-white border-cyan-400' : 'text-slate-400 border-transparent hover:text-white'}"
        >
          <span>Мої ігри</span>
          <span class="text-xs font-mono text-slate-400 font-normal">
            ({filteredGames.length})
          </span>
        </button>

        <button
          type="button"
          onclick={() => { activeDevTab = 'news'; loadMyNews(); }}
          class="text-lg font-bold tracking-wide flex items-center gap-2 pb-1 border-b-2 transition-all cursor-pointer {activeDevTab === 'news' ? 'text-white border-cyan-400' : 'text-slate-400 border-transparent hover:text-white'}"
        >
          <Newspaper class="w-4 h-4 text-cyan-400" />
          <span>Новини проектів</span>
          <span class="text-xs font-mono text-slate-400 font-normal">
            ({myNewsList.length})
          </span>
        </button>
      </div>

      {#if activeDevTab === 'games'}
        <!-- Search & Filters -->
        <div class="flex flex-wrap items-center gap-2.5">
          <div class="relative">
            <Search class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Пошук гри..."
              bind:value={searchQuery}
              class="pl-8 pr-3 py-1.5 rounded-xl bg-[#061923] border border-cyan-500/15 text-white text-xs placeholder-slate-500 focus:border-cyan-400 focus:outline-none transition-colors w-48 sm:w-56"
            />
          </div>

          <div class="flex items-center rounded-xl bg-[#061923] border border-cyan-500/15 p-0.5 text-xs">
            <button
              onclick={() => filterStatus = 'all'}
              class="px-2.5 py-1 rounded-lg font-medium transition-colors cursor-pointer {filterStatus === 'all' ? 'bg-cyan-500/20 text-[#0df2c9]' : 'text-slate-400 hover:text-white'}"
            >
              Всі
            </button>
            <button
              onclick={() => filterStatus = 'published'}
              class="px-2.5 py-1 rounded-lg font-medium transition-colors cursor-pointer {filterStatus === 'published' ? 'bg-cyan-500/20 text-[#0df2c9]' : 'text-slate-400 hover:text-white'}"
            >
              Опубліковані
            </button>
            <button
              onclick={() => filterStatus = 'draft'}
              class="px-2.5 py-1 rounded-lg font-medium transition-colors cursor-pointer {filterStatus === 'draft' ? 'bg-cyan-500/20 text-[#0df2c9]' : 'text-slate-400 hover:text-white'}"
            >
              Чернетки
            </button>
          </div>
        </div>
      {/if}
    </div>

    {#if activeDevTab === 'games'}
      <!-- Games Cards Grid: 2 equal balanced columns on md and lg -->
      {#if $developerStore.isLoading}
        <div class="py-16 text-center space-y-2">
          <RefreshCw class="w-6 h-6 animate-spin text-cyan-400 mx-auto" />
          <p class="text-xs text-slate-400">Оновлення каталогу ігор...</p>
        </div>
      {:else if filteredGames.length === 0}
        <div class="py-12 text-center rounded-2xl bg-[#061923]/60 border border-dashed border-cyan-500/15 p-6 space-y-3">
          <Gamepad2 class="w-10 h-10 text-slate-600 mx-auto" />
          <h3 class="text-sm font-bold text-white">Ігор не знайдено</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            {#if searchQuery || filterStatus !== 'all'}
              За вашими критеріями пошуку проектів не знайдено.
            {:else}
              У вас поки немає створених проектів. Опублікуйте вашу першу гру в каталозі Dteam!
            {/if}
          </p>
          {#if !searchQuery && filterStatus === 'all'}
            <button
              onclick={() => uiStore.setPublishGameModal(true)}
              class="px-4 py-2 rounded-xl bg-[#0df2c9] text-black font-extrabold text-xs shadow-md hover:bg-[#21fcd6] transition-all cursor-pointer inline-flex items-center gap-1.5"
            >
              <Plus class="w-3.5 h-3.5 stroke-[3]" />
              Створити першу гру
            </button>
          {/if}
        </div>
      {:else}
        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
          {#each filteredGames as game (game.id)}
            <div class="rounded-2xl bg-[#061923] border border-cyan-500/15 hover:border-cyan-500/30 transition-all overflow-hidden flex flex-col group shadow-md">
              <!-- Cover Header -->
              <div class="relative h-44 w-full bg-slate-950 overflow-hidden">
                <img
                  src={game.coverImageUrl || game.headerImageUrl || 'https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80'}
                  alt={game.title}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
                <div class="absolute inset-0 bg-gradient-to-t from-[#061923] via-transparent to-black/40"></div>

                <!-- Status Badge -->
                <div class="absolute top-2.5 left-2.5 flex items-center gap-2">
                  {#if game.isPublished}
                    <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-emerald-500/90 text-black text-[10px] font-black shadow backdrop-blur-sm">
                      <span class="w-1.5 h-1.5 rounded-full bg-black"></span>
                      Опубліковано
                    </span>
                  {:else}
                    <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-amber-500/90 text-black text-[10px] font-black shadow backdrop-blur-sm">
                      <Clock class="w-2.5 h-2.5" />
                      Чернетка
                    </span>
                  {/if}
                  {#if game.discountPercentage > 0}
                    <span class="px-1.5 py-0.5 rounded-md bg-[#ff4767] text-white text-[10px] font-black shadow">
                      -{game.discountPercentage}%
                    </span>
                  {/if}
                </div>

                <!-- Version & Release Date Tag -->
                <div class="absolute top-2.5 right-2.5 px-2.5 py-0.5 rounded-md bg-black/70 backdrop-blur-md text-[10px] text-slate-300 font-mono flex items-center gap-1.5 border border-white/10 shadow-sm">
                  <span>v{game.version || '1.0.0'}</span>
                  <span class="text-slate-600">•</span>
                  <span>{game.createdAt ? formatDate(game.createdAt) : '01.08.2026'}</span>
                </div>
              </div>

              <!-- Card Body -->
              <div class="p-4 flex-1 flex flex-col justify-between space-y-3">
                <div class="space-y-1.5">
                  <h4 class="text-base font-bold text-white group-hover:text-[#0df2c9] transition-colors font-display line-clamp-1">
                    {game.title}
                  </h4>
                  <p class="text-xs text-slate-200 line-clamp-2 leading-relaxed font-normal">
                    {game.shortDescription || game.description}
                  </p>

                  <!-- Tags / Genres -->
                  <div class="flex flex-wrap gap-1.5 pt-1">
                    {#each (game.genres || []).slice(0, 3) as genre}
                      <span class="px-2.5 py-0.5 rounded-md bg-[#092e40] text-[10px] text-cyan-300 border border-cyan-500/25 font-semibold">
                        {genre}
                      </span>
                    {/each}
                  </div>
                </div>

                <!-- Price & Downloads Line -->
                <div class="pt-2.5 border-t border-cyan-500/10 flex items-center justify-between text-xs">
                  <div class="flex items-center gap-3 text-slate-300 font-mono text-xs">
                    <span class="flex items-center gap-1" title="Завантажень">
                      <Download class="w-3.5 h-3.5 text-cyan-400" />
                      {game.downloadCount.toLocaleString()}
                    </span>
                    <span class="flex items-center gap-1 text-amber-400 font-semibold" title="Рейтинг">
                      <Star class="w-3.5 h-3.5 fill-amber-400" />
                      {game.averageRating ? game.averageRating.toFixed(1) : '5.0'}
                    </span>
                  </div>

                  <!-- Price in TON with TonIcon -->
                  <div>
                    {#if game.priceInNanoTons === 0}
                      <span class="text-xs font-bold text-[#0df2c9]">Free</span>
                    {:else}
                      <span class="text-xs font-bold font-mono text-white flex items-center gap-1">
                        <TonIcon class="w-3.5 h-3.5 text-[#0df2c9] shrink-0" />
                        <span>{formatTon(nanoTonToTon(game.priceInNanoTons))}</span>
                      </span>
                    {/if}
                  </div>
                </div>

                <!-- Actions Grid (5 buttons) -->
                <div class="grid grid-cols-5 gap-1.5 pt-1">
                  <!-- News -->
                  <button
                    onclick={() => gameForNews = game}
                    class="py-2 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/25 text-[#0df2c9] text-xs font-semibold transition-all flex items-center justify-center gap-1 cursor-pointer border border-cyan-500/20"
                    title="Опублікувати новину для цієї гри"
                  >
                    <Newspaper class="w-3.5 h-3.5" />
                    <span class="hidden sm:inline">Новина</span>
                  </button>

                  <!-- Edit -->
                  <button
                    onclick={() => uiStore.setEditGameModal(true, game)}
                    class="py-2 rounded-xl bg-[#092837] hover:bg-cyan-500/15 text-cyan-300 text-xs font-semibold transition-all flex items-center justify-center gap-1 cursor-pointer"
                    title="Редагувати"
                  >
                    <Edit3 class="w-3.5 h-3.5" />
                    <span class="hidden sm:inline">Ред.</span>
                  </button>

                  <!-- Toggle Publish Status -->
                  <button
                    onclick={() => handleTogglePublish(game)}
                    class="py-2 rounded-xl bg-[#092837] hover:bg-cyan-500/15 text-slate-300 hover:text-white text-xs font-semibold transition-all flex items-center justify-center gap-1 cursor-pointer"
                    title={game.isPublished ? 'Приховати в чернетки' : 'Опублікувати у каталозі'}
                  >
                    {#if game.isPublished}
                      <EyeOff class="w-3.5 h-3.5 text-amber-400" />
                      <span class="hidden sm:inline">Сховати</span>
                    {:else}
                      <Eye class="w-3.5 h-3.5 text-emerald-400" />
                      <span class="hidden sm:inline">Викласти</span>
                    {/if}
                  </button>

                  <!-- View in Store -->
                  <button
                    onclick={() => handleViewInStore(game)}
                    class="py-2 rounded-xl bg-[#092837] hover:bg-cyan-500/15 text-slate-300 hover:text-white text-xs font-semibold transition-all flex items-center justify-center gap-1 cursor-pointer"
                    title="Переглянути на вітрині"
                  >
                    <ExternalLink class="w-3.5 h-3.5" />
                    <span class="hidden sm:inline">Крамниця</span>
                  </button>

                  <!-- Delete -->
                  <button
                    onclick={() => gameToDelete = game}
                    class="py-2 rounded-xl bg-[#092837] hover:bg-red-500/20 text-slate-400 hover:text-red-400 text-xs transition-all flex items-center justify-center cursor-pointer"
                    title="Видалити"
                  >
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>
          {/each}
        </div>
      {/if}
    {:else}
      <!-- News Tab View -->
      {#if isLoadingMyNews}
        <div class="py-16 text-center space-y-2">
          <RefreshCw class="w-6 h-6 animate-spin text-cyan-400 mx-auto" />
          <p class="text-xs text-slate-400">Завантаження новин...</p>
        </div>
      {:else if myNewsList.length === 0}
        <div class="py-12 text-center rounded-2xl bg-[#061923]/60 border border-dashed border-cyan-500/15 p-6 space-y-3">
          <Newspaper class="w-10 h-10 text-slate-600 mx-auto" />
          <h3 class="text-sm font-bold text-white">Новин поки немає</h3>
          <p class="text-xs text-slate-400 max-w-sm mx-auto">
            Ви ще не публікували новини для своїх ігор.
          </p>
          {#if filteredGames.length > 0}
            <button
              onclick={() => gameForNews = filteredGames[0]}
              class="px-4 py-2 rounded-xl bg-[#0df2c9] text-black font-extrabold text-xs shadow-md hover:bg-[#21fcd6] transition-all cursor-pointer inline-flex items-center gap-1.5"
            >
              <Plus class="w-3.5 h-3.5 stroke-[3]" />
              Опублікувати першу новину
            </button>
          {/if}
        </div>
      {:else}
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          {#each myNewsList as news (news.id)}
            <div class="rounded-2xl bg-[#061923] border border-cyan-500/20 p-4 space-y-3 shadow-md flex flex-col justify-between">
              <div class="space-y-2">
                <div class="flex items-center justify-between gap-2">
                  <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-cyan-950/60 text-cyan-300 text-[11px] font-bold border border-cyan-800/40">
                    <Gamepad2 class="w-3 h-3 text-cyan-400" />
                    {news.gameTitle || 'Гра'}
                  </span>
                  <span class="text-[10px] text-slate-400 font-mono">
                    {new Date(news.createdAt).toLocaleDateString('uk-UA')}
                  </span>
                </div>

                <h4 class="text-sm font-bold text-white line-clamp-1">{news.title}</h4>
                <p class="text-xs text-slate-300 line-clamp-2 leading-relaxed">{news.content}</p>

                {#if news.media?.url}
                  <div class="rounded-xl overflow-hidden max-h-36 bg-black/40 border border-cyan-900/40">
                    {#if news.media.type === 'video'}
                      <video src={news.media.url} class="w-full max-h-36 object-cover" controls></video>
                    {:else}
                      <img src={news.media.url} alt="" class="w-full max-h-36 object-cover" />
                    {/if}
                  </div>
                {/if}
              </div>

              <div class="flex items-center justify-between pt-2 border-t border-cyan-900/40">
                <span class="text-[10px] text-cyan-400 font-medium">Офіційна новина</span>
                <button
                  type="button"
                  onclick={() => handleDeleteNews(news.id)}
                  class="px-2.5 py-1 rounded-lg bg-red-500/10 hover:bg-red-500/20 text-red-400 text-xs font-semibold transition-colors flex items-center gap-1 cursor-pointer"
                  title="Видалити новину"
                >
                  <Trash2 class="w-3.5 h-3.5" />
                  <span>Видалити</span>
                </button>
              </div>
            </div>
          {/each}
        </div>
      {/if}
    {/if}
  </div>
</div>

<!-- Modal for Creating Game News -->
{#if gameForNews}
  <CreateGameNewsModal
    game={gameForNews}
    isOpen={!!gameForNews}
    onClose={() => gameForNews = null}
    onCreated={() => loadMyNews()}
  />
{/if}

<!-- Confirmation Modal for Delete -->
{#if gameToDelete}
  <div class="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
    <div class="bg-[#061923] border border-red-500/30 rounded-2xl p-5 max-w-sm w-full shadow-2xl space-y-3 animate-in fade-in zoom-in-95">
      <div class="flex items-center gap-2.5 text-red-400">
        <AlertCircle class="w-5 h-5 shrink-0" />
        <h3 class="text-sm font-bold text-white">Видалити проект?</h3>
      </div>
      <p class="text-xs text-slate-300 leading-relaxed">
        Ви впевнені, що бажаєте видалити гру <strong class="text-white">"{gameToDelete.title}"</strong>? Всі файли та відгуки буде безповоротно стерто.
      </p>
      <div class="flex items-center justify-end gap-2.5 pt-2">
        <button
          onclick={() => gameToDelete = null}
          class="px-3 py-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold transition-all cursor-pointer"
        >
          Скасувати
        </button>
        <button
          onclick={confirmDelete}
          class="px-3 py-1.5 rounded-xl bg-red-600 hover:bg-red-500 text-white text-xs font-semibold transition-all shadow-md cursor-pointer"
        >
          Так, видалити
        </button>
      </div>
    </div>
  </div>
{/if}
