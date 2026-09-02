<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { profileStore } from '../../stores/profileStore';
  import { chatStore } from '../../stores/chatStore';
  import { UserStatus } from '../../types';
  import { X, UserPlus, Users, MessageSquare } from 'lucide-svelte';

  let newFriendInput = $state('');
  let isAdding = $state(false);

  async function handleAddFriend() {
    if (newFriendInput.trim()) {
      try {
        await friendsStore.sendRequest(newFriendInput.trim());
        newFriendInput = '';
        isAdding = false;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err?.message || 'Не вдалося надіслати запит',
          type: 'error'
        });
      }
    }
  }

  function handleStartChat(friendId: string) {
    chatStore.selectConversation(friendId);
    uiStore.setTab('chat');
    uiStore.toggleFriendsSidebar();
  }
</script>

{#if $uiStore.isFriendsSidebarOpen}
  <aside
    class="fixed right-0 top-0 bottom-0 w-80 bg-[#061820] border-l border-cyan-500/20 shadow-2xl z-50 flex flex-col p-4 animate-in slide-in-from-right"
  >
    
    <div class="flex items-center justify-between pb-3 border-b border-cyan-950/80">
      <div class="flex items-center gap-2">
        <Users class="w-4 h-4 text-cyan-400" />
        <h3 class="text-sm font-bold text-white uppercase tracking-wider">Друзі та чат</h3>
      </div>
      <button
        onclick={() => uiStore.toggleFriendsSidebar()}
        class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
      >
        <X class="w-4 h-4" />
      </button>
    </div>

    <div class="py-3 border-b border-cyan-950/80">
      {#if isAdding}
        <form onsubmit={(e) => { e.preventDefault(); handleAddFriend(); }} class="flex gap-2">
          <input
            type="text"
            placeholder="Username або ID..."
            bind:value={newFriendInput}
            class="flex-1 px-3 py-1.5 rounded-lg bg-[#041219] border border-cyan-500/30 text-xs text-white focus:border-cyan-400 focus:outline-none"
          />
          <button
            type="submit"
            class="px-3 py-1.5 rounded-lg bg-cyan-500 hover:bg-cyan-400 text-black text-xs font-bold transition-colors cursor-pointer"
          >
            Додати
          </button>
        </form>
      {:else}
        <button
          onclick={() => isAdding = true}
          class="w-full flex items-center justify-center gap-1.5 py-1.5 rounded-lg bg-[#0a232c] hover:bg-[#0d2f3b] text-xs font-semibold text-cyan-300 border border-cyan-500/20 transition-all cursor-pointer"
        >
          <UserPlus class="w-3.5 h-3.5" />
          <span>Додати друга</span>
        </button>
      {/if}
    </div>

    <div class="flex-1 overflow-y-auto py-2 space-y-1">
      {#each $friendsStore.friends as f (f.id)}
        <div class="flex items-center justify-between p-2 rounded-xl hover:bg-cyan-950/40 transition-all group">
          <button
            type="button"
            onclick={() => { profileStore.viewProfile(f.id); if ($uiStore.isFriendsSidebarOpen) uiStore.toggleFriendsSidebar(); }}
            class="flex items-center gap-2.5 min-w-0 text-left cursor-pointer flex-1"
          >
            <div class="relative">
              {#if f.avatarUrl}
                <img
                  src={f.avatarUrl}
                  alt={f.username}
                  class="w-8 h-8 rounded-lg object-cover ring-1 ring-cyan-500/30"
                />
              {:else}
                <div class="w-8 h-8 rounded-lg bg-cyan-500/20 text-cyan-300 font-bold flex items-center justify-center text-xs">
                  {f.username.charAt(0).toUpperCase()}
                </div>
              {/if}
              <span class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-[#061820]
                {f.status === UserStatus.Online ? 'bg-emerald-500' : ''}
                {f.status === UserStatus.InGame ? 'bg-cyan-400' : ''}
                {f.status === UserStatus.Away ? 'bg-amber-500' : ''}
                {f.status === UserStatus.Offline ? 'bg-slate-600' : ''}">
              </span>
            </div>

            <div class="truncate">
              <span class="block text-xs font-bold text-slate-200 truncate hover:text-cyan-300 transition-colors">
                {f.username}
              </span>
              <span class="block text-[10px] text-slate-400 truncate">
                {#if f.status === UserStatus.InGame}
                  <span class="text-cyan-400 font-medium">У грі</span>
                {:else if f.status === UserStatus.Online}
                  <span class="text-emerald-400">Онлайн</span>
                {:else if f.status === UserStatus.Away}
                  <span class="text-amber-400">Відійшов</span>
                {:else}
                  <span class="text-slate-500">Офлайн</span>
                {/if}
              </span>
            </div>
          </button>

          <button
            onclick={() => handleStartChat(f.id)}
            class="p-1.5 rounded-lg text-slate-500 hover:text-cyan-400 hover:bg-cyan-500/10 transition-colors opacity-0 group-hover:opacity-100 cursor-pointer"
            title="Почати чат"
          >
            <MessageSquare class="w-3.5 h-3.5" />
          </button>
        </div>
      {/each}
    </div>
  </aside>
{/if}
