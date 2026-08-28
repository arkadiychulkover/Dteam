<script lang="ts">
  import { onMount } from 'svelte';
  import { libraryStore } from '../../stores/libraryStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import { formatPlayTime, formatDate, formatBytes } from '../../utils/formatters';
  import { 
    Play, 
    Star, 
    Clock, 
    Download, 
    Settings, 
    Folder, 
    CheckCircle, 
    Sparkles, 
    Gamepad2,
    Loader2
  } from 'lucide-svelte';

  onMount(() => {
    libraryStore.loadLibrary();
    if ($gamesStore.games.length === 0) {
      gamesStore.loadGames();
    }
  });

  const userGames = $derived($libraryStore.items);
  const selectedUserGame = $derived(
    userGames.find(g => g.gameId === $libraryStore.selectedGameId) || userGames[0]
  );
  const selectedGameDetails = $derived(
    selectedUserGame
      ? (selectedUserGame.game || $gamesStore.games.find(g => g.id === selectedUserGame.gameId))
      : null
  );

  function getGameFor(item: (typeof userGames)[number]) {
    return item.game || $gamesStore.games.find((x) => x.id === item.gameId);
  }

  let isLaunching = $state(false);

  function launchGame() {
    isLaunching = true;
    uiStore.addToast({
      title: 'Launching Game...',
      message: `Starting ${selectedGameDetails?.title}. Have fun!`,
      type: 'info',
    });
    setTimeout(() => {
      isLaunching = false;
    }, 2000);
  }
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-6">
  {#if $libraryStore.isLoading && !$libraryStore.hasLoaded}
    <div class="flex items-center justify-center py-24 bg-slate-900/60 rounded-2xl border border-slate-800">
      <Loader2 class="w-10 h-10 text-cyan-400 animate-spin" />
    </div>
  {:else if userGames.length === 0}
    <div class="text-center py-24 bg-slate-900/60 rounded-2xl border border-slate-800">
      <Gamepad2 class="w-16 h-16 text-cyan-400/40 mx-auto mb-4" />
      <h3 class="text-2xl font-bold text-white">Your Library is Empty</h3>
      <p class="text-sm text-slate-400 mt-2 max-w-md mx-auto">
        Explore the decentralized Web3 store and acquire your favorite games with TON.
      </p>
      <button
        onclick={() => uiStore.setTab('store')}
        class="mt-6 px-6 py-3 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black font-bold text-sm transition-all cursor-pointer"
      >
        Browse Store
      </button>
    </div>
  {:else}
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 bg-[#0e111a] border border-slate-800/80 rounded-2xl overflow-hidden shadow-2xl min-h-[680px]">
      <!-- Left Sidebar: Games List -->
      <div class="lg:col-span-4 border-b lg:border-b-0 lg:border-r border-slate-800/80 p-4 flex flex-col bg-[#0b0d14]/70">
        <div class="flex items-center justify-between px-2 mb-3">
          <span class="text-xs font-bold text-slate-400 uppercase tracking-wider">All Games ({userGames.length})</span>
        </div>

        <!-- Games List -->
        <div class="space-y-1 overflow-y-auto flex-1 pr-1">
          {#each userGames as item (item.gameId)}
            {@const g = getGameFor(item)}
            {#if g}
              <div
                role="button"
                tabindex="0"
                onclick={() => libraryStore.selectGame(item.gameId)}
                onkeydown={(e) => e.key === 'Enter' && libraryStore.selectGame(item.gameId)}
                class="w-full flex items-center justify-between p-2.5 rounded-xl text-left transition-all cursor-pointer group
                  {$libraryStore.selectedGameId === item.gameId
                    ? 'bg-gradient-to-r from-cyan-950/90 to-blue-950/60 border border-cyan-500/40 text-white shadow-md'
                    : 'hover:bg-slate-800/50 text-slate-300 border border-transparent'}"
              >
                <div class="flex items-center gap-3 min-w-0">
                  <img
                    src={g.coverImageUrl || g.headerImageUrl}
                    alt={g.title}
                    class="w-9 h-9 rounded-lg object-cover ring-1 ring-slate-700"
                  />
                  <div class="truncate">
                    <span class="block text-xs font-bold truncate group-hover:text-cyan-300 transition-colors">
                      {g.title}
                    </span>
                    <span class="block text-[10px] text-slate-500 mt-0.5">
                      {formatPlayTime(item.playTimeMinutes)} played
                    </span>
                  </div>
                </div>

                <button
                  type="button"
                  onclick={(e) => { e.stopPropagation(); libraryStore.toggleFavorite(item.gameId); }}
                  class="p-1 text-slate-500 hover:text-amber-400 transition-colors cursor-pointer"
                  title="Toggle Favorite"
                >
                  <Star class="w-3.5 h-3.5 {item.isFavorite ? 'fill-amber-400 text-amber-400' : ''}" />
                </button>
              </div>
            {/if}
          {/each}
        </div>
      </div>

      <!-- Right Panel: Active Game Launcher & Details -->
      {#if selectedGameDetails && selectedUserGame}
        <div class="lg:col-span-8 flex flex-col justify-between p-6 md:p-8 relative bg-gradient-to-b from-[#131726]/60 to-[#0e111a]">
          <!-- Hero background art -->
          <div class="absolute inset-0 opacity-20 pointer-events-none overflow-hidden">
            <img
              src={selectedGameDetails.headerImageUrl || selectedGameDetails.coverImageUrl}
              alt=""
              class="w-full h-full object-cover filter blur-md"
            />
          </div>

          <div class="relative z-10">
            <!-- Top Bar -->
            <div class="flex items-center justify-between gap-4">
              <span class="text-xs px-2.5 py-1 rounded-md bg-cyan-950 text-cyan-400 border border-cyan-500/30 font-semibold uppercase tracking-wider">
                Installed & Verified
              </span>
              <span class="text-xs text-slate-400">
                Version: <span class="font-mono text-slate-200">{selectedGameDetails.version}</span>
              </span>
            </div>

            <h2 class="text-3xl md:text-5xl font-black text-white font-display mt-4">
              {selectedGameDetails.title}
            </h2>

            <p class="text-xs md:text-sm text-slate-300 max-w-2xl mt-2 leading-relaxed">
              {selectedGameDetails.description}
            </p>

            <!-- Stats Bar -->
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6 p-4 rounded-xl bg-slate-900/80 border border-slate-800">
              <div>
                <span class="text-[10px] text-slate-400 block uppercase font-semibold">Time Played</span>
                <span class="text-sm font-bold text-white flex items-center gap-1.5 mt-0.5">
                  <Clock class="w-3.5 h-3.5 text-cyan-400" />
                  {formatPlayTime(selectedUserGame.playTimeMinutes)}
                </span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase font-semibold">Last Played</span>
                <span class="text-sm font-bold text-white mt-0.5 block">
                  {selectedUserGame.lastPlayedAt ? formatDate(selectedUserGame.lastPlayedAt) : 'Never'}
                </span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase font-semibold">Storage</span>
                <span class="text-sm font-bold text-white mt-0.5 block">
                  {formatBytes(selectedGameDetails.sizeInBytes)}
                </span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase font-semibold">Cloud Sync</span>
                <span class="text-sm font-bold text-emerald-400 flex items-center gap-1 mt-0.5">
                  <CheckCircle class="w-3.5 h-3.5" /> Synchronized
                </span>
              </div>
            </div>
          </div>

          <!-- Bottom Action Controls -->
          <div class="relative z-10 mt-8 pt-6 border-t border-slate-800/80 flex flex-wrap items-center justify-between gap-4">
            <button
              onclick={launchGame}
              disabled={isLaunching}
              class="flex items-center gap-3 px-8 py-4 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-400 hover:to-teal-500 text-black font-black text-base tracking-wider shadow-lg shadow-emerald-500/25 hover:shadow-emerald-500/40 hover:scale-105 active:scale-95 transition-all cursor-pointer"
            >
              <Play class="w-5 h-5 fill-black" />
              <span>{isLaunching ? 'STARTING...' : 'PLAY GAME'}</span>
            </button>

            <div class="flex items-center gap-2">
              <button
                class="p-3 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors"
                title="Browse Local Archive"
              >
                <Folder class="w-4 h-4" />
              </button>
              <button
                class="p-3 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 hover:text-white border border-slate-700 transition-colors"
                title="Game Settings"
              >
                <Settings class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      {/if}
    </div>
  {/if}
</div>
