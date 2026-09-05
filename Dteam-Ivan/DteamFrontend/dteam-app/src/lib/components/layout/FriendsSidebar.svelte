<script lang="ts">
import { uiStore } from '../../stores/uiStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { profileStore } from '../../stores/profileStore';
  import { UserStatus } from '../../types';
  import { X, UserPlus, Users, MessageSquare } from 'lucide-svelte';

  let newFriendInput = $state('');
  let isAdding = $state(false);

  function handleAddFriend() {
    if (newFriendInput.trim()) {
      friendsStore.addFriend(newFriendInput.trim());
      uiStore.addToast({
        title: 'Friend Added',
        message: `Added ${newFriendInput.trim()} to your friends list.`,
        type: 'success',
      });
      newFriendInput = '';
      isAdding = false;
    }
  }
</script>

{#if $uiStore.isFriendsSidebarOpen}
  <aside
    class="fixed right-0 top-0 bottom-0 w-80 bg-[#0f121d] border-l border-slate-800 shadow-2xl z-50 flex flex-col p-4 animate-in slide-in-from-right"
  >

    <div class="flex items-center justify-between pb-3 border-b border-slate-800">
      <div class="flex items-center gap-2">
        <Users class="w-4 h-4 text-cyan-400" />
        <h3 class="text-sm font-bold text-white uppercase tracking-wider">Friends & Chat</h3>
      </div>
      <button
        onclick={() => uiStore.toggleFriendsSidebar()}
        class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
      >
        <X class="w-4 h-4" />
      </button>
    </div>

    <div class="py-3 border-b border-slate-800">
      {#if isAdding}
        <form onsubmit={(e) => { e.preventDefault(); handleAddFriend(); }} class="flex gap-2">
          <input
            type="text"
            placeholder="Username or email..."
            bind:value={newFriendInput}
            class="flex-1 px-3 py-1.5 rounded-lg bg-slate-900 border border-slate-700 text-xs text-white focus:border-cyan-500 focus:outline-none"
          />
          <button
            type="submit"
            class="px-3 py-1.5 rounded-lg bg-cyan-500 hover:bg-cyan-400 text-black text-xs font-bold transition-colors cursor-pointer"
          >
            Add
          </button>
        </form>
      {:else}
        <button
          onclick={() => isAdding = true}
          class="w-full flex items-center justify-center gap-1.5 py-1.5 rounded-lg bg-slate-800/80 hover:bg-slate-700 text-xs font-semibold text-cyan-300 border border-cyan-500/20 transition-all cursor-pointer"
        >
          <UserPlus class="w-3.5 h-3.5" />
          <span>Add Friend</span>
        </button>
      {/if}
    </div>

    <div class="flex-1 overflow-y-auto py-2 space-y-1">
      {#each $friendsStore.friends as f}
        <div class="flex items-center justify-between p-2 rounded-xl hover:bg-slate-800/60 transition-all group">
          <button
            type="button"
            onclick={() => profileStore.viewProfile(f.friend.id)}
            class="flex items-center gap-2.5 min-w-0 text-left cursor-pointer"
          >
            <div class="relative">
              <img
                src={f.friend.avatarUrl}
                alt={f.friend.username}
                class="w-8 h-8 rounded-lg object-cover ring-1 ring-slate-700"
              />
              <span class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-[#0f121d]
                {f.friend.status === UserStatus.Online ? 'bg-emerald-500' : ''}
                {f.friend.status === UserStatus.InGame ? 'bg-cyan-400' : ''}
                {f.friend.status === UserStatus.Away ? 'bg-amber-500' : ''}
                {f.friend.status === UserStatus.Offline ? 'bg-slate-600' : ''}">
              </span>
            </div>

            <div class="truncate">
              <span class="block text-xs font-bold text-slate-200 truncate hover:text-cyan-300 transition-colors">
                {f.friend.username}
              </span>
              <span class="block text-[10px] text-slate-400 truncate">
                {#if f.friend.status === UserStatus.InGame}
                  <span class="text-cyan-400 font-medium">In-Game: Neon Odyssey</span>
                {:else if f.friend.status === UserStatus.Online}
                  <span class="text-emerald-400">Online</span>
                {:else if f.friend.status === UserStatus.Away}
                  <span class="text-amber-400">Away</span>
                {:else}
                  <span class="text-slate-500">Offline</span>
                {/if}
              </span>
            </div>
          </button>

          <button
            class="p-1.5 rounded-lg text-slate-500 hover:text-cyan-400 hover:bg-slate-700/60 transition-colors opacity-0 group-hover:opacity-100"
            title="Message"
          >
            <MessageSquare class="w-3.5 h-3.5" />
          </button>
        </div>
      {/each}
    </div>
  </aside>
{/if}

