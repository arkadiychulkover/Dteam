<script lang="ts">
import { onMount, onDestroy } from 'svelte';
  import { adminService, activeAdminIdStore } from '../../services/adminService';
  import { uiStore } from '../../stores/uiStore';
  import { formatPrice } from '../../utils/formatters';
  import type { Duser, Game } from '../../types';
  import CreateUserModal from './CreateUserModal.svelte';
  import EditUserModal from './EditUserModal.svelte';
  import CreateGameModal from './CreateGameModal.svelte';
  import EditGameModal from './EditGameModal.svelte';
  import AdminGuidModal from './AdminGuidModal.svelte';
  import {
    Gamepad2,
    Users,
    Shield,
    Plus,
    Search,
    RefreshCw,
    Trash2,
    Edit,
    KeyRound,
    Sparkles,
    CheckCircle2,
    XCircle,
    Ban,
    Star,
    Download,
    Eye,
    EyeOff,
    Activity,
    AlertTriangle,
    Layers,
  } from 'lucide-svelte';

  let activeSubTab = $state<'games' | 'users'>('games');
  let users = $state<Duser[]>([]);
  let games = $state<Game[]>([]);
  let isLoading = $state(false);
  let searchQuery = $state('');
  let filterRole = $state<'all' | 'admin' | 'user' | 'banned'>('all');
  let filterGameType = $state<'all' | 'base' | 'dlc'>('all');

  let isBackendOnline = $state<boolean | null>(null);
  let backendLatency = $state<number | null>(null);
  let healthPollTimer: any = null;

  let isCreateUserOpen = $state(false);
  let isEditUserOpen = $state(false);
  let isCreateGameOpen = $state(false);
  let isEditGameOpen = $state(false);
  let isGuidModalOpen = $state(false);
  let isDeleteConfirmOpen = $state(false);

  let selectedUser = $state<Duser | null>(null);
  let selectedGame = $state<Game | null>(null);
  let itemToDelete = $state<{ type: 'user' | 'game'; id: string; name: string } | null>(null);

  const totalUsersCount = $derived(users.length);
  const adminUsersCount = $derived(users.filter((u) => u.isAdmin).length);
  const totalGamesCount = $derived(games.length);
  const publishedGamesCount = $derived(games.filter((g) => g.isPublished).length);
  const dlcGamesCount = $derived(games.filter((g) => g.isDlc).length);
  const baseGamesCount = $derived(games.filter((g) => !g.isDlc).length);

  const filteredUsers = $derived(
    users.filter((u) => {
      const matchSearch =
        u.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
        u.email.toLowerCase().includes(searchQuery.toLowerCase());
      if (!matchSearch) return false;

      if (filterRole === 'admin') return u.isAdmin;
      if (filterRole === 'user') return !u.isAdmin;
      if (filterRole === 'banned') return u.isBanned;
      return true;
    })
  );

  const filteredGames = $derived(
    games.filter((g) => {
      const matchSearch =
        g.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (g.shortDescription && g.shortDescription.toLowerCase().includes(searchQuery.toLowerCase()));
      if (!matchSearch) return false;

      if (filterGameType === 'base') return !g.isDlc;
      if (filterGameType === 'dlc') return g.isDlc;
      return true;
    })
  );

  async function pollHealth() {
    const startTime = performance.now();
    try {
      const res = await adminService.checkHealth();
      const endTime = performance.now();
      backendLatency = Math.round(endTime - startTime);
      isBackendOnline = res.status === 'Healthy';

      if (res.adminId && !$activeAdminIdStore) {
        adminService.setActiveAdminId(res.adminId);
      }
    } catch {
      isBackendOnline = false;
      backendLatency = null;
    }
  }

  async function loadData() {
    isLoading = true;
    try {
      await pollHealth();

      let adminId = adminService.getActiveAdminId();
      if (!adminId) {
        adminId = await adminService.autoDiscoverAdminGuid();
      }

      const [fetchedUsers, fetchedGames] = await Promise.all([
        adminService.getUsers(adminId).catch((err) => {
          console.warn('[AdminView] Users fetch error:', err);
          return [];
        }),
        adminService.getGames(adminId).catch((err) => {
          console.warn('[AdminView] Games fetch error:', err);
          return [];
        }),
      ]);

      users = fetchedUsers;
      games = fetchedGames;
    } catch (err: any) {
      uiStore.addToast({
        title: 'Ошибка загрузки данных',
        message: err.message || 'Проверьте запущен ли backend сервер.',
        type: 'warning',
      });
    } finally {
      isLoading = false;
    }
  }

  onMount(() => {
    loadData();
    pollHealth();
    healthPollTimer = setInterval(() => {
      pollHealth();
    }, 3000);
  });

  onDestroy(() => {
    if (healthPollTimer) {
      clearInterval(healthPollTimer);
    }
  });

  function openEditUser(user: Duser) {
    selectedUser = user;
    isEditUserOpen = true;
  }

  function openEditGame(game: Game) {
    selectedGame = game;
    isEditGameOpen = true;
  }

  function confirmDelete(type: 'user' | 'game', id: string, name: string) {
    itemToDelete = { type, id, name };
    isDeleteConfirmOpen = true;
  }

  async function executeDelete() {
    if (!itemToDelete) return;
    try {
      if (itemToDelete.type === 'user') {
        await adminService.deleteUser(itemToDelete.id);
        uiStore.addToast({
          title: 'User Deleted',
          message: `User '${itemToDelete.name}' has been deleted.`,
          type: 'success',
        });
      } else {
        await adminService.deleteGame(itemToDelete.id);
        uiStore.addToast({
          title: 'Game Deleted',
          message: `Game '${itemToDelete.name}' has been removed from catalog.`,
          type: 'success',
        });
      }
      isDeleteConfirmOpen = false;
      itemToDelete = null;
      await loadData();
    } catch (err: any) {
      uiStore.addToast({
        title: 'Delete Failed',
        message: err.message || 'Could not delete item.',
        type: 'error',
      });
    }
  }

  async function toggleUserAdmin(user: Duser) {
    try {
      await adminService.updateUser(user.id, { isAdmin: !user.isAdmin });
      uiStore.addToast({
        title: 'Role Updated',
        message: `Admin status for '${user.username}' changed to ${!user.isAdmin}`,
        type: 'info',
      });
      await loadData();
    } catch (err: any) {
      uiStore.addToast({ title: 'Update Error', message: err.message, type: 'error' });
    }
  }

  async function toggleUserBan(user: Duser) {
    try {
      await adminService.updateUser(user.id, { isBanned: !user.isBanned });
      uiStore.addToast({
        title: user.isBanned ? 'User Unbanned' : 'User Banned',
        message: `Account '${user.username}' is now ${user.isBanned ? 'active' : 'banned'}`,
        type: user.isBanned ? 'success' : 'warning',
      });
      await loadData();
    } catch (err: any) {
      uiStore.addToast({ title: 'Update Error', message: err.message, type: 'error' });
    }
  }

  async function toggleGamePublish(game: Game) {
    try {
      await adminService.updateGame(game.id, { isPublished: !game.isPublished });
      uiStore.addToast({
        title: 'Visibility Changed',
        message: `Game '${game.title}' is now ${!game.isPublished ? 'Published' : 'Hidden'}`,
        type: 'info',
      });
      await loadData();
    } catch (err: any) {
      uiStore.addToast({ title: 'Update Error', message: err.message, type: 'error' });
    }
  }
</script>

<div class="max-w-7xl mx-auto px-4 lg:px-8 py-8 animate-in fade-in">
  <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8 pb-6 border-b border-cyan-950/80">
    <div>
      <div class="flex items-center gap-3">
        <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-cyan-500 to-emerald-500 flex items-center justify-center text-black font-black shadow-lg shadow-cyan-500/30">
          <Shield class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-2xl lg:text-3xl font-black text-white tracking-tight flex items-center gap-2">
            <span>ADMIN <span class="text-cyan-400">CONTROL CENTER</span></span>
            <span class="text-[11px] uppercase font-mono font-bold tracking-wider px-2.5 py-0.5 rounded-full bg-cyan-950 text-cyan-300 border border-cyan-500/40">
              CRUD Mode
            </span>
          </h1>
          <p class="text-xs text-slate-400 mt-0.5">Manage games catalog, user accounts and system status</p>
        </div>
      </div>
    </div>

    <div class="flex flex-wrap items-center gap-2.5">
      <button
        onclick={() => isGuidModalOpen = true}
        class="flex items-center gap-2 px-3.5 py-2 rounded-xl bg-[#061e27] border border-cyan-500/40 hover:border-cyan-400 text-xs text-cyan-300 font-mono transition-all cursor-pointer shadow-inner group"
        title="Click to view or switch active Admin GUID"
      >
        <KeyRound class="w-4 h-4 text-cyan-400 group-hover:rotate-45 transition-transform" />
        <span>Admin ID:</span>
        <span class="font-bold text-white">
          {$activeAdminIdStore ? `${$activeAdminIdStore.slice(0, 8)}...` : 'Auto-Detect'}
        </span>
        <span class="w-2 h-2 rounded-full {isBackendOnline ? 'bg-emerald-400 animate-pulse' : 'bg-rose-500'}"></span>
      </button>

      <button
        onclick={loadData}
        disabled={isLoading}
        class="p-2.5 rounded-xl bg-slate-900/90 hover:bg-slate-800 border border-slate-700/80 text-slate-300 hover:text-white transition-all cursor-pointer"
        title="Reload Database"
      >
        <RefreshCw class="w-4 h-4 {isLoading ? 'animate-spin text-cyan-400' : ''}" />
      </button>

      <button
        onclick={() => isCreateGameOpen = true}
        class="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer"
      >
        <Plus class="w-4 h-4" />
        <span>New Game</span>
      </button>

      <button
        onclick={() => isCreateUserOpen = true}
        class="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 border border-cyan-500/30 text-cyan-300 hover:text-white font-semibold text-xs transition-all cursor-pointer"
      >
        <Plus class="w-4 h-4" />
        <span>New User</span>
      </button>
    </div>
  </div>

  <div class="grid grid-cols-1 md:grid-cols-3 gap-5 mb-8">
    <div class="p-5 rounded-2xl bg-[#061820]/90 border border-cyan-500/20 shadow-lg shadow-cyan-950/30 relative overflow-hidden group hover:border-cyan-500/50 transition-all">
      <div class="absolute -right-6 -bottom-6 w-24 h-24 bg-cyan-500/10 rounded-full blur-xl group-hover:scale-125 transition-transform"></div>
      <div class="flex items-center justify-between mb-2">
        <span class="text-xs font-bold text-slate-400 tracking-wider uppercase">Catalog Games</span>
        <Gamepad2 class="w-5 h-5 text-cyan-400" />
      </div>
      <div class="text-3xl font-black text-white tracking-tight">{totalGamesCount}</div>
      <div class="text-[11px] text-emerald-400 mt-1.5 flex items-center gap-2 font-semibold">
        <span class="flex items-center gap-1"><CheckCircle2 class="w-3.5 h-3.5" /> {publishedGamesCount} Published</span>
        {#if dlcGamesCount > 0}
          <span class="text-purple-300">• {dlcGamesCount} DLCs</span>
        {/if}
      </div>
    </div>

    <div class="p-5 rounded-2xl bg-[#061820]/90 border border-cyan-500/20 shadow-lg shadow-cyan-950/30 relative overflow-hidden group hover:border-cyan-500/50 transition-all">
      <div class="absolute -right-6 -bottom-6 w-24 h-24 bg-emerald-500/10 rounded-full blur-xl group-hover:scale-125 transition-transform"></div>
      <div class="flex items-center justify-between mb-2">
        <span class="text-xs font-bold text-slate-400 tracking-wider uppercase">Total Accounts</span>
        <Users class="w-5 h-5 text-emerald-400" />
      </div>
      <div class="text-3xl font-black text-white tracking-tight">{totalUsersCount}</div>
      <div class="text-[11px] text-cyan-300 mt-1.5 flex items-center gap-1 font-semibold">
        <Shield class="w-3.5 h-3.5" />
        <span>{adminUsersCount} Administrators</span>
      </div>
    </div>

    <div class="p-5 rounded-2xl {isBackendOnline ? 'bg-[#061820]/90 border-cyan-500/20 hover:border-cyan-500/50' : 'bg-rose-950/30 border-rose-500/40'} border shadow-lg relative overflow-hidden transition-all">
      <div class="flex items-center justify-between mb-2">
        <span class="text-xs font-bold text-slate-400 tracking-wider uppercase">Backend Status (3s Polling)</span>
        <Activity class="w-5 h-5 {isBackendOnline ? 'text-emerald-400' : 'text-rose-500'}" />
      </div>

      {#if isBackendOnline === true}
        <div class="text-2xl font-black text-emerald-400 flex items-center gap-2 tracking-tight">
          <span class="w-3 h-3 rounded-full bg-emerald-400 animate-ping"></span>
          <span>Online & Synced</span>
        </div>
        <div class="text-[11px] text-slate-400 mt-1.5 flex items-center gap-2">
          <span>ASP.NET Core API</span>
          {#if backendLatency !== null}
            <span class="font-mono text-emerald-300">({backendLatency}ms)</span>
          {/if}
        </div>
      {:else if isBackendOnline === false}
        <div class="text-2xl font-black text-rose-500 flex items-center gap-2 tracking-tight">
          <span class="w-3 h-3 rounded-full bg-rose-500 animate-pulse"></span>
          <span>Offline / Error</span>
        </div>
        <div class="text-[11px] text-rose-300 mt-1.5 flex items-center gap-1 font-medium">
          <AlertTriangle class="w-3.5 h-3.5" />
          <span>Cannot connect to /api/health/check-status</span>
        </div>
      {:else}
        <div class="text-2xl font-bold text-slate-400 flex items-center gap-2">
          <span class="animate-spin">⟳</span>
          <span>Checking...</span>
        </div>
        <div class="text-[11px] text-slate-500 mt-1.5">Connecting to API...</div>
      {/if}
    </div>
  </div>

  <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
    <div class="flex items-center gap-1 bg-[#061820] p-1.5 rounded-2xl border border-cyan-500/20">
      <button
        onclick={() => activeSubTab = 'games'}
        class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold tracking-wide transition-all cursor-pointer
          {activeSubTab === 'games'
            ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black shadow-md shadow-cyan-500/30 font-black'
            : 'text-slate-400 hover:text-white hover:bg-slate-800/50'}"
      >
        <Gamepad2 class="w-4 h-4" />
        <span>Games Catalog ({games.length})</span>
      </button>

      <button
        onclick={() => activeSubTab = 'users'}
        class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold tracking-wide transition-all cursor-pointer
          {activeSubTab === 'users'
            ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black shadow-md shadow-cyan-500/30 font-black'
            : 'text-slate-400 hover:text-white hover:bg-slate-800/50'}"
      >
        <Users class="w-4 h-4" />
        <span>Users & Roles ({users.length})</span>
      </button>
    </div>

    <div class="relative max-w-md flex-1">
      <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
      <input
        type="text"
        placeholder={activeSubTab === 'games' ? 'Search games by title...' : 'Search users by name or email...'}
        bind:value={searchQuery}
        class="w-full pl-10 pr-4 py-2.5 rounded-xl bg-[#061820] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-white placeholder-slate-500 shadow-inner"
      />
    </div>
  </div>

  {#if activeSubTab === 'games'}
    <div>
      <div class="flex items-center justify-between gap-3 mb-6 pb-3 border-b border-cyan-950/60">
        <div class="flex items-center gap-2">
          <span class="text-xs font-semibold text-slate-400">Показать:</span>
          <button
            onclick={() => filterGameType = 'all'}
            class="px-3 py-1 rounded-lg text-xs font-semibold uppercase tracking-wider transition-all cursor-pointer
              {filterGameType === 'all'
                ? 'bg-cyan-500 text-black shadow-md shadow-cyan-500/20 font-bold'
                : 'bg-slate-900/80 hover:bg-slate-800 text-slate-400 border border-slate-800'}"
          >
            Все игры ({games.length})
          </button>
          <button
            onclick={() => filterGameType = 'base'}
            class="px-3 py-1 rounded-lg text-xs font-semibold uppercase tracking-wider transition-all cursor-pointer
              {filterGameType === 'base'
                ? 'bg-cyan-500 text-black shadow-md shadow-cyan-500/20 font-bold'
                : 'bg-slate-900/80 hover:bg-slate-800 text-slate-400 border border-slate-800'}"
          >
            Базовые игры ({baseGamesCount})
          </button>
          <button
            onclick={() => filterGameType = 'dlc'}
            class="px-3 py-1 rounded-lg text-xs font-semibold uppercase tracking-wider transition-all cursor-pointer flex items-center gap-1
              {filterGameType === 'dlc'
                ? 'bg-purple-600 text-white shadow-md shadow-purple-600/30 font-bold'
                : 'bg-purple-950/50 hover:bg-purple-900/60 text-purple-300 border border-purple-500/30'}"
          >
            <Layers class="w-3.5 h-3.5" />
            <span>DLC / Дополнения ({dlcGamesCount})</span>
          </button>
        </div>

        <div class="text-xs text-slate-400">
          Найдено: <span class="text-white font-bold">{filteredGames.length}</span>
        </div>
      </div>

      {#if filteredGames.length > 0}
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {#each filteredGames as game (game.id)}
            <div class="group relative rounded-2xl bg-[#061820]/90 border {game.isDlc ? 'border-purple-500/40 hover:border-purple-400' : 'border-cyan-500/20 hover:border-cyan-400/60'} overflow-hidden shadow-xl hover:shadow-cyan-500/20 transition-all flex flex-col">
              <div class="relative aspect-[3/4] w-full overflow-hidden bg-slate-950">
                <img
                  src={game.coverImageUrl || 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800'}
                  alt={game.title}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />
                <div class="absolute inset-0 bg-gradient-to-t from-[#061820] via-transparent to-black/30"></div>

                <div class="absolute top-3 left-3 flex items-center gap-1.5">
                  <div class="px-2 py-0.5 rounded-md bg-black/70 backdrop-blur-md border border-cyan-500/40 text-[10px] font-bold text-cyan-300 flex items-center gap-1">
                    <Star class="w-3 h-3 text-amber-400 fill-amber-400" />
                    <span>{game.averageRating > 0 ? game.averageRating.toFixed(1) : '5.0'}</span>
                  </div>
                  {#if game.isDlc}
                    <span class="px-2 py-0.5 rounded-md bg-purple-950/90 backdrop-blur-md border border-purple-500/50 text-[9px] font-extrabold text-purple-300 uppercase tracking-widest flex items-center gap-1">
                      <Layers class="w-3 h-3 text-purple-400" />
                      <span>DLC</span>
                    </span>
                  {/if}
                </div>

                <button
                  onclick={() => toggleGamePublish(game)}
                  class="absolute top-3 right-3 p-1.5 rounded-lg backdrop-blur-md border transition-all cursor-pointer
                    {game.isPublished
                      ? 'bg-emerald-950/80 border-emerald-500/50 text-emerald-300'
                      : 'bg-rose-950/80 border-rose-500/50 text-rose-300'}"
                  title={game.isPublished ? 'Published (Click to unpublish)' : 'Unpublished (Click to publish)'}
                >
                  {#if game.isPublished}
                    <Eye class="w-3.5 h-3.5" />
                  {:else}
                    <EyeOff class="w-3.5 h-3.5" />
                  {/if}
                </button>

                <div class="absolute bottom-3 left-3 right-3">
                  <h3 class="text-base font-extrabold text-white leading-tight line-clamp-1 group-hover:text-cyan-300 transition-colors">
                    {game.title}
                  </h3>
                  <div class="flex items-center justify-between mt-1">
                    <div class="flex items-center gap-1.5">
                      {#if (game.discountPercentage || 0) > 0}
                        <span class="px-1.5 py-0.2 rounded bg-rose-600 text-white font-extrabold text-[9px]">
                          -{game.discountPercentage}%
                        </span>
                      {/if}
                      <div class="text-sm font-black text-cyan-300 font-mono">
                        {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                      </div>
                    </div>
                    <div class="text-[10px] text-slate-400 flex items-center gap-1">
                      <Download class="w-3 h-3 text-slate-500" />
                      <span>{game.downloadCount}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="p-3 bg-[#08202b] border-t border-cyan-950 flex items-center justify-between gap-2">
                <span class="text-[10px] font-mono text-slate-400">v{game.version}</span>
                <div class="flex items-center gap-1.5">
                  <button
                    onclick={() => openEditGame(game)}
                    class="p-1.5 rounded-lg bg-slate-800/80 hover:bg-cyan-500 hover:text-black text-slate-300 border border-slate-700 transition-colors cursor-pointer"
                    title="Edit Game"
                  >
                    <Edit class="w-3.5 h-3.5" />
                  </button>
                  <button
                    onclick={() => confirmDelete('game', game.id, game.title)}
                    class="p-1.5 rounded-lg bg-slate-800/80 hover:bg-rose-600 hover:text-white text-slate-300 border border-slate-700 transition-colors cursor-pointer"
                    title="Delete Game"
                  >
                    <Trash2 class="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>
          {/each}
        </div>
      {:else}
        <div class="text-center py-16 bg-[#061820]/60 rounded-2xl border border-cyan-500/20">
          <Gamepad2 class="w-12 h-12 text-slate-600 mx-auto mb-3" />
          <h4 class="text-base font-bold text-slate-300">No games found in catalog</h4>
          <p class="text-xs text-slate-500 mt-1">Click "New Game" above to add games to the database.</p>
        </div>
      {/if}
    </div>
  {/if}

  {#if activeSubTab === 'users'}
    <div class="rounded-2xl bg-[#061820]/90 border border-cyan-500/20 overflow-hidden shadow-xl">
      <div class="p-4 border-b border-cyan-950 flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <span class="text-xs font-semibold text-slate-400">Filter:</span>
          {#each ['all', 'admin', 'user', 'banned'] as role}
            <button
              onclick={() => filterRole = role as any}
              class="px-3 py-1 rounded-lg text-xs font-semibold uppercase tracking-wider transition-all cursor-pointer
                {filterRole === role
                  ? 'bg-cyan-500 text-black shadow-md shadow-cyan-500/20 font-bold'
                  : 'bg-slate-900/80 hover:bg-slate-800 text-slate-400 border border-slate-800'}"
            >
              {role}
            </button>
          {/each}
        </div>
        <div class="text-xs text-slate-400">
          Showing <span class="text-white font-bold">{filteredUsers.length}</span> of {users.length} users
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-[#08202b] text-slate-400 uppercase tracking-wider text-[10px] border-b border-cyan-950">
            <tr>
              <th class="py-3.5 px-4">User</th>
              <th class="py-3.5 px-4">Email</th>
              <th class="py-3.5 px-4">TON Balance</th>
              <th class="py-3.5 px-4">Role</th>
              <th class="py-3.5 px-4">Status</th>
              <th class="py-3.5 px-4 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-cyan-950/40">
            {#each filteredUsers as user (user.id)}
              <tr class="hover:bg-cyan-950/20 transition-colors">
                <td class="py-3.5 px-4">
                  <div class="flex items-center gap-3">
                    <img
                      src={user.avatarUrl || 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150'}
                      alt={user.username}
                      class="w-8 h-8 rounded-xl object-cover border border-cyan-500/30"
                    />
                    <div>
                      <div class="font-bold text-white flex items-center gap-1.5">
                        <span>{user.username}</span>
                        {#if user.isAdmin}
                          <Shield class="w-3.5 h-3.5 text-cyan-400" />
                        {/if}
                      </div>
                      <div class="text-[10px] text-slate-500 font-mono">{user.id.slice(0, 8)}...</div>
                    </div>
                  </div>
                </td>

                <td class="py-3.5 px-4 text-slate-300 font-mono">{user.email}</td>

                <td class="py-3.5 px-4 font-mono font-bold text-cyan-300">
                  {formatPrice(user.balanceInNanoTons)}
                </td>

                <td class="py-3.5 px-4">
                  <button
                    onclick={() => toggleUserAdmin(user)}
                    class="px-2.5 py-1 rounded-full text-[10px] font-extrabold uppercase tracking-wider transition-all cursor-pointer
                      {user.isAdmin
                        ? 'bg-cyan-950 text-cyan-300 border border-cyan-500/40 hover:bg-cyan-900'
                        : 'bg-slate-900 text-slate-400 border border-slate-800 hover:border-slate-600'}"
                    title="Click to toggle Admin permissions"
                  >
                    {user.isAdmin ? 'Admin' : 'Player'}
                  </button>
                </td>

                <td class="py-3.5 px-4">
                  {#if user.isBanned}
                    <span class="px-2 py-0.5 rounded-md bg-rose-950 text-rose-300 border border-rose-500/40 text-[10px] font-bold">
                      Banned
                    </span>
                  {:else}
                    <span class="px-2 py-0.5 rounded-md bg-emerald-950 text-emerald-300 border border-emerald-500/40 text-[10px] font-bold">
                      Active
                    </span>
                  {/if}
                </td>

                <td class="py-3.5 px-4 text-right">
                  <div class="flex items-center justify-end gap-1.5">
                    <button
                      onclick={() => toggleUserBan(user)}
                      class="p-1.5 rounded-lg border transition-colors cursor-pointer
                        {user.isBanned
                          ? 'bg-emerald-950/80 hover:bg-emerald-900 border-emerald-500/40 text-emerald-300'
                          : 'bg-slate-900 hover:bg-rose-950/80 border-slate-800 hover:border-rose-500/40 text-slate-400 hover:text-rose-300'}"
                      title={user.isBanned ? 'Unban User' : 'Ban User'}
                    >
                      <Ban class="w-3.5 h-3.5" />
                    </button>

                    <button
                      onclick={() => openEditUser(user)}
                      class="p-1.5 rounded-lg bg-slate-900 hover:bg-cyan-500 hover:text-black border border-slate-800 text-slate-300 transition-colors cursor-pointer"
                      title="Edit User"
                    >
                      <Edit class="w-3.5 h-3.5" />
                    </button>

                    <button
                      onclick={() => confirmDelete('user', user.id, user.username)}
                      class="p-1.5 rounded-lg bg-slate-900 hover:bg-rose-600 hover:text-white border border-slate-800 text-slate-400 transition-colors cursor-pointer"
                      title="Delete User"
                    >
                      <Trash2 class="w-3.5 h-3.5" />
                    </button>
                  </div>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
    </div>
  {/if}
</div>

<CreateUserModal
  isOpen={isCreateUserOpen}
  onClose={() => isCreateUserOpen = false}
  onUserCreated={loadData}
/>

<EditUserModal
  user={selectedUser}
  isOpen={isEditUserOpen}
  onClose={() => isEditUserOpen = false}
  onUserUpdated={loadData}
/>

<CreateGameModal
  allGames={games}
  isOpen={isCreateGameOpen}
  onClose={() => isCreateGameOpen = false}
  onGameCreated={loadData}
/>

<EditGameModal
  game={selectedGame}
  allGames={games}
  isOpen={isEditGameOpen}
  onClose={() => isEditGameOpen = false}
  onGameUpdated={loadData}
  onOpenEditOtherGame={(other) => openEditGame(other)}
/>

<AdminGuidModal
  isOpen={isGuidModalOpen}
  onClose={() => isGuidModalOpen = false}
  {users}
  onGuidChanged={loadData}
/>

{#if isDeleteConfirmOpen && itemToDelete}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-md bg-[#061820] border border-rose-500/40 rounded-2xl shadow-2xl p-6">
      <div class="w-12 h-12 rounded-2xl bg-rose-950/60 border border-rose-500/40 flex items-center justify-center text-rose-400 mx-auto mb-4">
        <Trash2 class="w-6 h-6" />
      </div>
      <h3 class="text-base font-bold text-white text-center">Confirm Permanent Deletion</h3>
      <p class="text-xs text-slate-400 text-center mt-1">
        Are you sure you want to permanently delete {itemToDelete.type} <strong class="text-white">'{itemToDelete.name}'</strong>? This action cannot be undone.
      </p>

      <div class="flex items-center justify-center gap-3 mt-6">
        <button
          onclick={() => isDeleteConfirmOpen = false}
          class="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-slate-300 transition-colors cursor-pointer"
        >
          Cancel
        </button>
        <button
          onclick={executeDelete}
          class="px-5 py-2 rounded-xl bg-rose-600 hover:bg-rose-500 text-white font-bold text-xs shadow-lg shadow-rose-600/30 transition-all cursor-pointer"
        >
          Delete Permanently
        </button>
      </div>
    </div>
  </div>
{/if}

