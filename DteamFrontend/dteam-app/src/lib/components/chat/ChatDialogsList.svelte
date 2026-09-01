<script lang="ts">
  import { Search, MessageSquareOff } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import { uiStore } from '../../stores/uiStore';
  import { currentUser } from '../../stores/authStore';
  import type { ChatConversation } from '../../types/chat';

  interface Props {
    conversations: ChatConversation[];
    activeFriendId: string | null;
  }

  let { conversations = [], activeFriendId = null }: Props = $props();

  let searchQuery = $state('');

  function handleSelect(friendId: string) {
    chatStore.selectConversation(friendId);
  }

  function formatConversationTime(isoString?: string | null): string {
    if (!isoString) return '';
    try {
      const d = new Date(isoString);
      const today = new Date();
      if (
        d.getDate() === today.getDate() &&
        d.getMonth() === today.getMonth() &&
        d.getFullYear() === today.getFullYear()
      ) {
        const hours = d.getHours().toString().padStart(2, '0');
        const mins = d.getMinutes().toString().padStart(2, '0');
        return `${hours}:${mins}`;
      }

      const months = ['січ.', 'лют.', 'берез.', 'квіт.', 'трав.', 'черв.', 'лип.', 'серп.', 'верес.', 'жовт.', 'листоп.', 'груд.'];
      const day = d.getDate().toString().padStart(2, '0');
      return `${day} ${months[d.getMonth()]}`;
    } catch {
      return '';
    }
  }

  function formatLastMessageSnippet(conv: ChatConversation): string {
    const msg = conv.lastMessage;
    if (!msg) return 'Немає повідомлень';

    const isMine = msg.senderId === $currentUser?.id;
    const prefix = isMine ? 'Ви: ' : '';

    if (msg.type === 1) {
      return `${prefix}${msg.content || 'Фото'}`;
    }
    if (msg.type === 2) {
      return `${prefix}${msg.originalFileName || 'Файл'}`;
    }
    if (msg.type === 3) {
      return `${prefix}Голосове повідомлення`;
    }
    return `${prefix}${msg.content || 'Повідомлення'}`;
  }

  const filteredConversations = $derived(
    conversations.filter(c => 
      !searchQuery.trim() ||
      c.friendUsername.toLowerCase().includes(searchQuery.toLowerCase().trim())
    )
  );
</script>

<aside class="w-80 h-full bg-[#05151c]/95 border-r border-cyan-500/20 flex flex-col shrink-0 overflow-hidden backdrop-blur-xl">
  <!-- Search Input Bar -->
  <div class="p-3.5 border-b border-cyan-500/20">
    <div class="relative">
      <input
        type="text"
        placeholder="Пошук..."
        bind:value={searchQuery}
        class="w-full pl-9 pr-3.5 py-2 rounded-2xl bg-[#081e27] hover:bg-[#0b242e] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_12px_rgba(13,242,201,0.2)] focus:outline-none text-xs text-white placeholder-slate-400 transition-all shadow-inner"
      />
      <Search class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
    </div>
  </div>

  <!-- Dialogs List -->
  <div class="flex-1 overflow-y-auto divide-y divide-cyan-500/10">
    {#if filteredConversations.length === 0}
      <div class="h-72 flex flex-col items-center justify-center p-4 text-center text-slate-500 text-xs gap-2">
        <MessageSquareOff class="w-8 h-8 opacity-40 text-cyan-400" />
        <p class="font-medium text-slate-300">{searchQuery ? 'Нічого не знайдено' : 'Немає активних діалогів'}</p>
        
        <div class="flex flex-col gap-2 w-full max-w-[200px] mt-2">
          <button
            type="button"
            onclick={() => chatStore.seedDemoConversations()}
            class="w-full py-2 px-3 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 text-[#03131a] font-black text-xs shadow-lg shadow-cyan-500/20 hover:opacity-90 transition-all cursor-pointer flex items-center justify-center gap-1.5"
          >
            <span>Створити діалоги</span>
          </button>

          <button
            type="button"
            onclick={() => uiStore.setTab('friends')}
            class="w-full py-1.5 px-3 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-300 border border-cyan-500/30 text-xs font-bold transition-all cursor-pointer"
          >
            Перейти до друзів
          </button>
        </div>
      </div>
    {:else}
      {#each filteredConversations as conv (conv.friendId)}
        {@const isActive = activeFriendId === conv.friendId}

        <button
          type="button"
          onclick={() => handleSelect(conv.friendId)}
          class="w-full text-left p-3 flex items-center gap-3 transition-all cursor-pointer group relative
            {isActive 
              ? 'bg-[#0a2c38] shadow-inner border-l-4 border-cyan-400' 
              : 'hover:bg-[#082029]/80'}"
        >
          <!-- User Avatar + Online status -->
          <div class="relative shrink-0">
            <div class="w-11 h-11 rounded-2xl overflow-hidden bg-gradient-to-br from-cyan-900 to-slate-900 border border-cyan-500/30 flex items-center justify-center text-cyan-300 font-bold text-sm shadow-md">
              {#if conv.friendAvatarUrl}
                <img
                  src={conv.friendAvatarUrl}
                  alt={conv.friendUsername}
                  class="w-full h-full object-cover"
                />
              {:else}
                {conv.friendUsername.charAt(0).toUpperCase()}
              {/if}
            </div>

            <!-- Online Dot -->
            {#if conv.friendStatus === 1}
              <span class="w-3 h-3 rounded-full bg-emerald-400 border-2 border-[#05151c] absolute -bottom-0.5 -right-0.5 shadow-[0_0_6px_#10b981]"></span>
            {/if}
          </div>

          <!-- Middle Info: Username + Snippet -->
          <div class="flex-1 min-w-0 pr-1">
            <div class="flex items-center justify-between gap-1 mb-0.5">
              <span class="text-xs font-bold text-white truncate tracking-wide group-hover:text-cyan-300 transition-colors">
                {conv.friendUsername}
              </span>
              <span class="text-[10px] font-mono text-slate-400 shrink-0">
                {formatConversationTime(conv.lastActivityAt || conv.lastMessage?.createdAt)}
              </span>
            </div>

            <div class="flex items-center justify-between gap-2">
              <p class="text-[11px] text-slate-400 truncate leading-snug">
                {formatLastMessageSnippet(conv)}
              </p>

              <!-- Unread Badge -->
              {#if conv.unreadCount > 0}
                <span class="min-w-[18px] h-[18px] px-1.5 rounded-full bg-rose-500 text-white text-[10px] font-black flex items-center justify-center shrink-0 shadow-sm animate-in zoom-in">
                  {conv.unreadCount > 99 ? '99+' : conv.unreadCount}
                </span>
              {/if}
            </div>
          </div>
        </button>
      {/each}
    {/if}
  </div>
</aside>
