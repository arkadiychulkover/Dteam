<script lang="ts">
  import { authStore, currentUser } from '../../stores/authStore';
  import { uiStore, type MainTab } from '../../stores/uiStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { formatPrice, formatAddress } from '../../utils/formatters';
  import { UserStatus } from '../../types';
  import { 
    Gamepad2, 
    Layers, 
    Users, 
    User, 
    Wallet, 
    PlusCircle, 
    Bell, 
    Sparkles, 
    ChevronDown 
  } from 'lucide-svelte';

  let isStatusDropdownOpen = $state(false);

  const tabs: { id: MainTab; label: string; icon: any }[] = [
    { id: 'store', label: 'STORE', icon: Gamepad2 },
    { id: 'library', label: 'LIBRARY', icon: Layers },
    { id: 'community', label: 'COMMUNITY', icon: Users },
    { id: 'profile', label: 'PROFILE', icon: User },
  ];

  const onlineFriendsCount = $derived(
    $friendsStore.friends.filter(f => f.friend.status !== UserStatus.Offline).length
  );

  function handleStatusChange(status: UserStatus) {
    authStore.setStatus(status);
    isStatusDropdownOpen = false;
  }
</script>

<header class="sticky top-0 z-40 bg-[#0f121d]/95 backdrop-blur-md border-b border-slate-800/80 px-4 lg:px-8 py-3">
  <div class="max-w-7xl mx-auto flex items-center justify-between gap-4">
    <!-- Left: Logo & Brand -->
    <div class="flex items-center gap-8">
      <button 
        onclick={() => uiStore.setTab('store')}
        class="flex items-center gap-2.5 group cursor-pointer"
      >
        <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center shadow-lg shadow-cyan-500/20 group-hover:scale-105 transition-all">
          <Gamepad2 class="w-6 h-6 text-white" />
        </div>
        <div class="text-left">
          <span class="font-extrabold text-xl tracking-wider text-white font-['Outfit'] flex items-center gap-1">
            D<span class="text-cyan-400">TEAM</span>
            <span class="text-[10px] uppercase font-bold tracking-widest px-1.5 py-0.5 rounded bg-cyan-950 text-cyan-400 border border-cyan-500/30">
              TON
            </span>
          </span>
          <span class="block text-[10px] text-slate-400 tracking-wider">WEB3 GAMING</span>
        </div>
      </button>

      <!-- Nav Tabs -->
      <nav class="hidden md:flex items-center gap-1 bg-slate-900/60 p-1 rounded-xl border border-slate-800">
        {#each tabs as tab}
          {@const Icon = tab.icon}
          <button
            onclick={() => uiStore.setTab(tab.id)}
            class="flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-bold tracking-wider transition-all cursor-pointer
              {$uiStore.activeTab === tab.id 
                ? 'bg-gradient-to-r from-cyan-500 to-blue-600 text-white shadow-md shadow-cyan-500/20' 
                : 'text-slate-400 hover:text-slate-100 hover:bg-slate-800/60'}"
          >
            <Icon class="w-4 h-4" />
            {tab.label}
          </button>
        {/each}
      </nav>
    </div>

    <!-- Right: Wallet, Actions, User Profile -->
    <div class="flex items-center gap-3">
      <!-- Publish Game Button -->
      <button
        onclick={() => uiStore.setPublishGameModal(true)}
        class="hidden sm:flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800/80 hover:bg-slate-700 text-xs font-semibold text-cyan-300 border border-cyan-500/30 transition-all cursor-pointer"
      >
        <PlusCircle class="w-4 h-4 text-cyan-400" />
        <span>Publish Game</span>
      </button>

      <!-- Wallet Balance Pill -->
      <button
        onclick={() => uiStore.setWalletModal(true)}
        class="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-gradient-to-r from-cyan-950/80 to-blue-950/80 border border-cyan-500/40 hover:border-cyan-400/80 text-white transition-all cursor-pointer group shadow-inner"
        title="Open TON Wallet"
      >
        <div class="w-5 h-5 rounded-full bg-cyan-500/20 flex items-center justify-center text-cyan-400 group-hover:scale-110 transition-transform">
          <Wallet class="w-3.5 h-3.5" />
        </div>
        <div class="text-right">
          <span class="text-xs font-black text-cyan-300 font-mono tracking-tight">
            {formatPrice($currentUser?.balanceInNanoTons ?? 0)}
          </span>
        </div>
      </button>

      <!-- Friends Panel Toggle -->
      <button
        onclick={() => uiStore.toggleFriendsSidebar()}
        class="relative p-2 rounded-xl bg-slate-900/80 hover:bg-slate-800 border border-slate-800 text-slate-300 hover:text-white transition-colors cursor-pointer"
        title="Friends List"
      >
        <Users class="w-5 h-5" />
        {#if onlineFriendsCount > 0}
          <span class="absolute -top-1 -right-1 w-4 h-4 bg-emerald-500 text-black text-[10px] font-extrabold rounded-full flex items-center justify-center ring-2 ring-[#0f121d]">
            {onlineFriendsCount}
          </span>
        {/if}
      </button>

      <!-- User Profile & Status -->
      {#if $currentUser}
        <div class="relative">
          <button
            onclick={() => isStatusDropdownOpen = !isStatusDropdownOpen}
            class="flex items-center gap-2 p-1 pl-2 rounded-xl bg-slate-900/60 hover:bg-slate-800 border border-slate-800 transition-all cursor-pointer"
          >
            <div class="relative">
              <img
                src={$currentUser.avatarUrl || 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80'}
                alt={$currentUser.username}
                class="w-8 h-8 rounded-lg object-cover ring-1 ring-slate-700"
              />
              <span class="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-[#0f121d]
                {$currentUser.status === UserStatus.Online ? 'bg-emerald-500' : ''}
                {$currentUser.status === UserStatus.InGame ? 'bg-cyan-400 animate-pulse' : ''}
                {$currentUser.status === UserStatus.Away ? 'bg-amber-500' : ''}
                {$currentUser.status === UserStatus.Offline ? 'bg-slate-500' : ''}">
              </span>
            </div>
            <span class="hidden sm:block text-xs font-semibold text-slate-200">
              {$currentUser.username}
            </span>
            <ChevronDown class="w-3.5 h-3.5 text-slate-400" />
          </button>

          {#if isStatusDropdownOpen}
            <div class="absolute right-0 mt-2 w-48 bg-[#141724] border border-slate-700/80 rounded-xl shadow-2xl p-1 z-50 animate-in fade-in slide-in-from-top-2">
              <div class="px-3 py-2 border-b border-slate-800 text-[11px] text-slate-400">
                Set Steam Status:
              </div>
              <button
                onclick={() => handleStatusChange(UserStatus.Online)}
                class="w-full text-left px-3 py-2 text-xs rounded-lg flex items-center gap-2 hover:bg-slate-800 text-slate-200"
              >
                <span class="w-2 h-2 rounded-full bg-emerald-500"></span> Online
              </button>
              <button
                onclick={() => handleStatusChange(UserStatus.InGame)}
                class="w-full text-left px-3 py-2 text-xs rounded-lg flex items-center gap-2 hover:bg-slate-800 text-slate-200"
              >
                <span class="w-2 h-2 rounded-full bg-cyan-400"></span> In-Game
              </button>
              <button
                onclick={() => handleStatusChange(UserStatus.Away)}
                class="w-full text-left px-3 py-2 text-xs rounded-lg flex items-center gap-2 hover:bg-slate-800 text-slate-200"
              >
                <span class="w-2 h-2 rounded-full bg-amber-500"></span> Away
              </button>
              <button
                onclick={() => handleStatusChange(UserStatus.Offline)}
                class="w-full text-left px-3 py-2 text-xs rounded-lg flex items-center gap-2 hover:bg-slate-800 text-slate-200"
              >
                <span class="w-2 h-2 rounded-full bg-slate-500"></span> Offline (Invisible)
              </button>
              <div class="border-t border-slate-800 my-1"></div>
              <button
                onclick={() => { uiStore.setTab('profile'); isStatusDropdownOpen = false; }}
                class="w-full text-left px-3 py-2 text-xs rounded-lg flex items-center gap-2 hover:bg-slate-800 text-slate-300"
              >
                <User class="w-3.5 h-3.5" /> View Profile
              </button>
            </div>
          {/if}
        </div>
      {/if}
    </div>
  </div>
</header>
