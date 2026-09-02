<script lang="ts">
  import { onMount } from 'svelte';
  import { 
    ArrowLeft, 
    UserPlus, 
    Search, 
    MoreHorizontal, 
    Check, 
    X, 
    Gamepad2, 
    ChevronDown, 
    Shield, 
    UserMinus, 
    Ban, 
    Users,
    User,
    Activity,
    MessageSquare,
    Loader2,
    Copy,
    Fingerprint
  } from 'lucide-svelte';
  import { profileStore } from '../../stores/profileStore';
  import { uiStore } from '../../stores/uiStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { chatStore } from '../../stores/chatStore';
  import { currentUser } from '../../stores/authStore';
  import { UserStatus } from '../../types';
  import type { FriendDto, FriendRequestDto } from '../../types/friend';
  import AddFriendModal from './AddFriendModal.svelte';
  import FriendsActivityFeed from '../activity/FriendsActivityFeed.svelte';
  import { activityStore } from '../../stores/activityStore';

  let isIdCopied = $state(false);

  type ActiveFriendTab = 'all' | 'online' | 'activity' | 'blocked' | 'requests';

  let activeTab = $state<ActiveFriendTab>('all');
  let searchQuery = $state('');
  let searchByGame = $state(false);
  let isAddFriendOpen = $state(false);
  let activeMenuFriendId = $state<string | null>(null);

  const friends = $derived($friendsStore.friends);
  const requests = $derived($friendsStore.requests);
  const blocked = $derived($friendsStore.blocked);
  const isLoading = $derived($friendsStore.isLoading);

  const onlineFriends = $derived(
    friends.filter(f => f.status === UserStatus.Online || f.status === UserStatus.InGame)
  );

  const filteredFriends = $derived.by(() => {
    let list = activeTab === 'online' ? onlineFriends : friends;
    const q = searchQuery.trim().toLowerCase();
    if (q) {
      list = list.filter(f => 
        f.username.toLowerCase().includes(q) || 
        (searchByGame && f.currentGame?.toLowerCase().includes(q))
      );
    }
    if (searchByGame && !q) {
      list = list.filter(f => Boolean(f.currentGame));
    }
    return list;
  });

  const filteredBlocked = $derived.by(() => {
    const q = searchQuery.trim().toLowerCase();
    if (!q) return blocked;
    return blocked.filter(b => b.username.toLowerCase().includes(q));
  });

  const filteredRequests = $derived.by(() => {
    const q = searchQuery.trim().toLowerCase();
    if (!q) return requests;
    return requests.filter(r => r.senderUsername.toLowerCase().includes(q));
  });

  onMount(() => {
    friendsStore.loadAll();
  });

  function getStatusDotColor(status: UserStatus): string {
    if (status === UserStatus.Online) return 'bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.8)]';
    if (status === UserStatus.InGame) return 'bg-pink-500 shadow-[0_0_8px_rgba(244,63,94,0.8)]';
    return 'bg-slate-500';
  }

  function getStatusLabel(status: UserStatus, currentGame?: string | null): string {
    if (status === UserStatus.InGame) return currentGame ? `Грає в ${currentGame}` : 'У грі';
    if (status === UserStatus.Online) return 'У мережі';
    if (status === UserStatus.Away) return 'Відійшов';
    if (status === UserStatus.Busy) return 'Зайнятий';
    return 'Не в мережі';
  }

  function toggleMenu(friendId: string, e: MouseEvent) {
    e.stopPropagation();
    activeMenuFriendId = activeMenuFriendId === friendId ? null : friendId;
  }

  function closeMenu() {
    activeMenuFriendId = null;
  }

  async function copyMyId() {
    if (!$currentUser?.id) return;
    try {
      await navigator.clipboard.writeText($currentUser.id);
      isIdCopied = true;
      setTimeout(() => isIdCopied = false, 2500);
      uiStore.addToast({
        title: 'ID скопійовано',
        message: 'Ваш ID скопійовано в буфер обміну. Поділіться ним з друзями!',
        type: 'success'
      });
    } catch (e) {
      console.warn('Copy failed', e);
    }
  }
</script>

<svelte:window onclick={closeMenu} />

<div class="min-h-screen bg-[#03131a] text-slate-200 py-6 px-3 sm:px-6 max-w-2xl mx-auto">
  <div class="flex items-center justify-between gap-4 mb-6">
    <div class="flex items-center gap-3">
      <button
        onclick={() => uiStore.setTab('store')}
        class="p-2 rounded-xl text-slate-300 hover:text-white hover:bg-cyan-950/40 transition-colors cursor-pointer"
        title="Назад до крамниці"
      >
        <ArrowLeft class="w-5 h-5" />
      </button>
      <h1 class="text-xl sm:text-2xl font-black text-white tracking-tight font-display">
        Друзі
      </h1>
    </div>

    <button
      onclick={() => isAddFriendOpen = true}
      class="px-5 py-2 rounded-full bg-[#0df2c9] hover:bg-[#12e3bd] text-[#03131a] font-black text-xs sm:text-sm tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center gap-1.5 shrink-0"
    >
      <span>Додати друга</span>
    </button>
  </div>

  {#if $currentUser}
    <div class="flex items-center justify-between p-3 sm:p-3.5 rounded-2xl bg-[#06242e]/90 border border-cyan-500/25 mb-5 shadow-lg shadow-cyan-950/40">
      <div class="flex items-center gap-3 min-w-0">
        <div class="w-8 h-8 rounded-xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center text-[#0df2c9] shrink-0">
          <Fingerprint class="w-4 h-4" />
        </div>
        <div class="min-w-0">
          <span class="block text-[10px] text-slate-400 font-bold uppercase tracking-wider">Ваш персональний ID:</span>
          <span class="block text-xs font-mono font-bold text-[#0df2c9] truncate select-all" title={$currentUser.id}>
            {$currentUser.id}
          </span>
        </div>
      </div>

      <button
        type="button"
        onclick={copyMyId}
        class="px-3 py-1.5 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 text-[#0df2c9] hover:text-white border border-cyan-500/30 text-xs font-bold transition-all flex items-center gap-1.5 cursor-pointer shrink-0 ml-2 shadow-sm"
        title="Скопіювати свій ID"
      >
        {#if isIdCopied}
          <Check class="w-3.5 h-3.5 text-emerald-400" />
          <span class="text-emerald-400 text-[11px]">Скопійовано!</span>
        {:else}
          <Copy class="w-3.5 h-3.5" />
          <span class="text-[11px]">Копіювати ID</span>
        {/if}
      </button>
    </div>
  {/if}

  <div class="flex items-center gap-4 sm:gap-6 border-b border-cyan-950/80 pb-1 mb-5 overflow-x-auto no-scrollbar select-none text-xs sm:text-sm">
    <button
      onclick={() => activeTab = 'all'}
      class="flex items-center gap-2 pb-2.5 transition-all cursor-pointer font-bold shrink-0 relative
        {activeTab === 'all' 
          ? 'text-white border-b-2 border-[#0df2c9]' 
          : 'text-slate-400 hover:text-slate-200 border-b-2 border-transparent'}"
    >
      <span>Усі друзі</span>
      <span class="px-2.5 py-0.5 rounded-full text-[11px] font-mono font-black
        {activeTab === 'all' ? 'bg-[#0a3542] text-[#0df2c9]' : 'bg-[#061d24] text-slate-400'}">
        {friends.length}
      </span>
    </button>

    <button
      onclick={() => activeTab = 'online'}
      class="flex items-center gap-2 pb-2.5 transition-all cursor-pointer font-bold shrink-0 relative
        {activeTab === 'online' 
          ? 'text-white border-b-2 border-[#0df2c9]' 
          : 'text-slate-400 hover:text-slate-200 border-b-2 border-transparent'}"
    >
      <span>Онлайн</span>
      <span class="px-2.5 py-0.5 rounded-full text-[11px] font-mono font-black
        {activeTab === 'online' ? 'bg-[#0a3542] text-[#0df2c9]' : 'bg-[#061d24] text-slate-400'}">
        {onlineFriends.length}
      </span>
    </button>

    <button
      onclick={() => activeTab = 'activity'}
      class="flex items-center gap-2 pb-2.5 transition-all cursor-pointer font-bold shrink-0 relative
        {activeTab === 'activity' 
          ? 'text-white border-b-2 border-[#0df2c9]' 
          : 'text-slate-400 hover:text-slate-200 border-b-2 border-transparent'}"
    >
      <Activity class="w-3.5 h-3.5 {activeTab === 'activity' ? 'text-[#0df2c9]' : 'text-slate-400'}" />
      <span>Активність</span>
      {#if $activityStore.friendsActivities.length > 0}
        <span class="px-2.5 py-0.5 rounded-full text-[11px] font-mono font-black
          {activeTab === 'activity' ? 'bg-[#0a3542] text-[#0df2c9]' : 'bg-[#061d24] text-slate-400'}">
          {$activityStore.friendsActivities.length}
        </span>
      {/if}
    </button>

    <button
      onclick={() => activeTab = 'blocked'}
      class="flex items-center gap-2 pb-2.5 transition-all cursor-pointer font-bold shrink-0 relative
        {activeTab === 'blocked' 
          ? 'text-white border-b-2 border-[#0df2c9]' 
          : 'text-slate-400 hover:text-slate-200 border-b-2 border-transparent'}"
    >
      <span>Заблоковані</span>
      <span class="px-2.5 py-0.5 rounded-full text-[11px] font-mono font-black
        {activeTab === 'blocked' ? 'bg-[#0a3542] text-[#0df2c9]' : 'bg-[#061d24] text-slate-400'}">
        {blocked.length}
      </span>
    </button>

    <button
      onclick={() => activeTab = 'requests'}
      class="flex items-center gap-2 pb-2.5 transition-all cursor-pointer font-bold shrink-0 relative
        {activeTab === 'requests' 
          ? 'text-white border-b-2 border-[#0df2c9]' 
          : 'text-slate-400 hover:text-slate-200 border-b-2 border-transparent'}"
    >
      <span>Поточні запити</span>
      <span class="px-2.5 py-0.5 rounded-full text-[11px] font-mono font-black
        {activeTab === 'requests' ? 'bg-[#0a3542] text-[#0df2c9]' : 'bg-[#061d24] text-slate-400'}">
        {requests.length}
      </span>
    </button>
  </div>

  <div class="space-y-3 mb-6">
    {#if activeTab === 'all' || activeTab === 'online'}
      <div class="flex items-center gap-2">
        <label class="flex items-center gap-2 text-xs font-semibold text-slate-400 cursor-pointer select-none">
          <input
            type="checkbox"
            bind:checked={searchByGame}
            class="w-4 h-4 rounded-md border-cyan-500/30 bg-[#062029] text-cyan-400 focus:ring-0 focus:outline-none accent-[#0df2c9]"
          />
          <span class="flex items-center gap-1 hover:text-cyan-300 transition-colors">
            Шукати лише тих, хто грає
            <ChevronDown class="w-3.5 h-3.5 text-cyan-400" />
          </span>
        </label>
      </div>

      <div class="relative">
        <input
          type="text"
          placeholder="Пошук за нікнеймом{searchByGame ? ' або назвою гри' : ''}..."
          bind:value={searchQuery}
          class="w-full pl-10 pr-4 py-2.5 rounded-2xl bg-[#04151b] border border-cyan-500/20 text-xs sm:text-sm text-white focus:outline-none focus:border-[#0df2c9] transition-colors"
        />
        <Search class="w-4 h-4 text-cyan-400/60 absolute left-3.5 top-1/2 -translate-y-1/2" />
        {#if searchQuery}
          <button
            type="button"
            onclick={() => searchQuery = ''}
            class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white cursor-pointer"
          >
            <X class="w-4 h-4" />
          </button>
        {/if}
      </div>
    {/if}
  </div>

  {#if isLoading}
    <div class="p-12 text-center">
      <Loader2 class="w-8 h-8 mx-auto text-cyan-400 animate-spin" />
    </div>

  {:else if activeTab === 'all' || activeTab === 'online'}
    <div class="space-y-2.5">
      {#each filteredFriends as friend (friend.id)}
        <div class="relative flex items-center justify-between p-3 sm:p-3.5 rounded-2xl bg-[#06242e]/90 hover:bg-[#08303d] border border-cyan-500/15 hover:border-cyan-500/35 transition-all group">
          
          <button
            type="button"
            onclick={() => profileStore.viewProfile(friend.id)}
            class="flex items-center gap-3 min-w-0 text-left cursor-pointer group/user flex-1"
            title="Переглянути профіль {friend.username}"
          >
            <div class="relative w-11 h-11 rounded-full shrink-0 select-none group-hover/user:scale-105 transition-transform">
              {#if friend.avatarUrl}
                <img
                  src={friend.avatarUrl}
                  alt={friend.username}
                  class="w-full h-full rounded-full object-cover border border-cyan-500/30 group-hover/user:border-cyan-400"
                />
              {:else}
                <div class="w-full h-full rounded-full bg-gradient-to-tr from-cyan-600 to-teal-500 flex items-center justify-center text-white font-black text-sm group-hover/user:border group-hover/user:border-cyan-400">
                  {friend.username.charAt(0).toUpperCase()}
                </div>
              {/if}

              <span class="absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-[#06242e] {getStatusDotColor(friend.status)}"></span>
            </div>

            <div class="min-w-0">
              <span class="block text-sm font-bold text-white group-hover/user:text-cyan-300 transition-colors truncate max-w-[200px] sm:max-w-[280px]">
                {friend.username}
              </span>
              <span class="block text-[10px] text-slate-400 truncate max-w-[200px] sm:max-w-[280px] mt-0.5">
                {getStatusLabel(friend.status, friend.currentGame)}
              </span>
            </div>
          </button>

          <div class="flex items-center gap-2 shrink-0">
            <button
              type="button"
              onclick={() => {
                chatStore.selectConversation(friend.id);
                uiStore.setTab('chat');
              }}
              class="p-2 text-cyan-400 hover:text-white bg-cyan-500/10 hover:bg-cyan-500/20 border border-cyan-500/30 rounded-xl transition-all cursor-pointer shadow-sm"
              title="Написати повідомлення"
            >
              <MessageSquare class="w-4 h-4" />
            </button>

            <div class="relative w-7 h-7 flex items-center justify-center select-none" title="Рівень гравця: {friend.level ?? 0}">
              <svg viewBox="0 0 24 24" class="w-full h-full fill-[#041920] stroke-[#0df2c9] stroke-[2]">
                <polygon points="12,2 22,7.5 22,17.5 12,23 2,17.5 2,7.5" />
              </svg>
              <span class="absolute inset-0 flex items-center justify-center font-mono font-black text-[10px] text-[#0df2c9]">
                {friend.level ?? 0}
              </span>
            </div>

            <div class="relative">
              <button
                type="button"
                onclick={(e) => toggleMenu(friend.id, e)}
                class="p-1.5 text-slate-400 hover:text-white rounded-xl hover:bg-cyan-950/60 transition-colors cursor-pointer"
                title="Опції"
              >
                <MoreHorizontal class="w-5 h-5" />
              </button>

              {#if activeMenuFriendId === friend.id}
                <div class="absolute right-0 mt-2 w-48 bg-[#091f28] border border-cyan-500/30 rounded-2xl shadow-2xl p-1.5 z-30 animate-in fade-in zoom-in-95">
                  <button
                    onclick={() => { profileStore.viewProfile(friend.id); activeMenuFriendId = null; }}
                    class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-cyan-500/10 text-cyan-300 font-semibold cursor-pointer mb-0.5"
                  >
                    <User class="w-3.5 h-3.5 text-cyan-400" />
                    <span>Переглянути профіль</span>
                  </button>
                  <button
                    onclick={() => friendsStore.blockUser(friend)}
                    class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-amber-500/10 text-amber-300 font-semibold cursor-pointer"
                  >
                    <Ban class="w-3.5 h-3.5 text-amber-400" />
                    <span>Заблокувати</span>
                  </button>
                  <button
                    onclick={() => friendsStore.removeFriend(friend.id, friend.username)}
                    class="w-full text-left px-3 py-2 text-xs rounded-xl flex items-center gap-2 hover:bg-red-500/10 text-red-400 font-semibold cursor-pointer mt-0.5"
                  >
                    <UserMinus class="w-3.5 h-3.5" />
                    <span>Видалити з друзів</span>
                  </button>
                </div>
              {/if}
            </div>
          </div>
        </div>
      {:else}
        <div class="p-12 rounded-3xl bg-[#062029]/40 border border-cyan-500/15 text-center text-slate-400">
          <Users class="w-10 h-10 mx-auto text-cyan-400/60 mb-3" />
          <p class="text-sm font-bold text-slate-300">
            {activeTab === 'online' ? 'Немає друзів у мережі' : 'Список друзів порожній'}
          </p>
          <p class="text-xs text-slate-500 mt-1">
            Натисніть кнопку «Додати друга», щоб надіслати перший запит!
          </p>
        </div>
      {/each}
    </div>

  {:else if activeTab === 'activity'}
    <FriendsActivityFeed />

  {:else if activeTab === 'blocked'}
    <div class="space-y-2.5">
      {#each filteredBlocked as b (b.id)}
        <div class="p-3.5 sm:p-4 rounded-2xl bg-[#06242e]/90 border border-cyan-500/15 space-y-3">
          <div class="flex items-center gap-3">
            <div class="relative w-10 h-10 rounded-full shrink-0">
              {#if b.avatarUrl}
                <img src={b.avatarUrl} alt={b.username} class="w-full h-full rounded-full object-cover border border-cyan-500/30" />
              {:else}
                <div class="w-full h-full rounded-full bg-slate-800 flex items-center justify-center text-slate-400 font-bold text-xs">
                  {b.username.charAt(0).toUpperCase()}
                </div>
              {/if}
              <span class="absolute bottom-0 right-0 w-2.5 h-2.5 rounded-full bg-pink-500 border border-[#06242e]"></span>
            </div>
            <span class="text-sm font-bold text-white truncate">
              {b.username}
            </span>
          </div>

          <button
            type="button"
            onclick={() => friendsStore.unblockUser(b.id)}
            class="w-full py-2 rounded-full border border-cyan-500/30 hover:border-[#0df2c9] text-cyan-300 hover:text-white hover:bg-cyan-500/10 text-xs font-bold transition-all cursor-pointer text-center"
          >
            Розблокувати
          </button>
        </div>
      {:else}
        <div class="p-12 rounded-3xl bg-[#062029]/40 border border-cyan-500/15 text-center text-slate-400">
          <Shield class="w-10 h-10 mx-auto text-cyan-400/60 mb-3" />
          <p class="text-sm font-bold text-slate-300">Немає заблокованих користувачів</p>
        </div>
      {/each}
    </div>

  {:else if activeTab === 'requests'}
    <div class="space-y-2.5">
      {#each filteredRequests as req (req.id)}
        <div class="flex items-center justify-between p-3 sm:p-3.5 rounded-2xl bg-[#06242e]/90 hover:bg-[#08303d] border border-cyan-500/15 transition-all">
          <div class="flex items-center gap-3 min-w-0">
            <div class="relative w-10 h-10 rounded-full shrink-0">
              {#if req.senderAvatarUrl}
                <img src={req.senderAvatarUrl} alt={req.senderUsername} class="w-full h-full rounded-full object-cover border border-cyan-500/30" />
              {:else}
                <div class="w-full h-full rounded-full bg-gradient-to-tr from-teal-600 to-cyan-500 flex items-center justify-center text-white font-bold text-xs">
                  {req.senderUsername.charAt(0).toUpperCase()}
                </div>
              {/if}
              <span class="absolute bottom-0 right-0 w-2.5 h-2.5 rounded-full bg-pink-500 border border-[#06242e]"></span>
            </div>

            <div class="min-w-0">
              <span class="block text-sm font-bold text-white truncate max-w-[180px] sm:max-w-[280px]">
                {req.senderUsername}
              </span>
              <span class="block text-[10px] text-slate-400 mt-0.5">
                Запит у друзі
              </span>
            </div>
          </div>

          <div class="flex items-center gap-2 shrink-0">
            <button
              type="button"
              onclick={() => friendsStore.acceptRequest(req.id, req.senderUsername)}
              class="w-9 h-9 rounded-xl bg-[#54e346] hover:bg-[#48ce3b] text-black font-black flex items-center justify-center shadow-md shadow-emerald-500/20 transition-all cursor-pointer"
              title="Прийняти запит"
            >
              <Check class="w-5 h-5 stroke-[2.5]" />
            </button>

            <button
              type="button"
              onclick={() => friendsStore.rejectRequest(req.id, req.senderUsername)}
              class="w-9 h-9 rounded-xl bg-[#f87171] hover:bg-[#ef4444] text-white font-black flex items-center justify-center shadow-md shadow-rose-500/20 transition-all cursor-pointer"
              title="Відхилити запит"
            >
              <X class="w-5 h-5 stroke-[2.5]" />
            </button>
          </div>
        </div>
      {:else}
        <div class="p-12 rounded-3xl bg-[#062029]/40 border border-cyan-500/15 text-center text-slate-400">
          <UserPlus class="w-10 h-10 mx-auto text-cyan-400/60 mb-3" />
          <p class="text-sm font-bold text-slate-300">Немає вхідних запитів</p>
          <p class="text-xs text-slate-500 mt-1">Нові запити в друзі будуть з'являтися тут.</p>
        </div>
      {/each}
    </div>
  {/if}
</div>

<AddFriendModal bind:isOpen={isAddFriendOpen} />
