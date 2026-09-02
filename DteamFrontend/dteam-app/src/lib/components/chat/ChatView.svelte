<script lang="ts">
  import { onMount } from 'svelte';
  import { ArrowLeft, User, Info, X } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import ChatDialogsList from './ChatDialogsList.svelte';
  import ChatMessageList from './ChatMessageList.svelte';
  import ChatInputBar from './ChatInputBar.svelte';
  import ChatRightSidebar from './ChatRightSidebar.svelte';

  let isMobileProfileDrawerOpen = $state(false);

  onMount(() => {
    chatStore.loadConversations();
  });

  const activeConversation = $derived.by(() => {
    if (!$chatStore.activeFriendId) return null;
    const targetId = $chatStore.activeFriendId.toLowerCase();
    return $chatStore.conversations.find(c => c.friendId.toLowerCase() === targetId) || {
      friendId: $chatStore.activeFriendId,
      friendUsername: 'Співрозмовник',
      friendStatus: 1,
      unreadCount: 0
    };
  });

  const activeMessages = $derived.by(() => {
    if (!$chatStore.activeFriendId) return [];
    const targetId = $chatStore.activeFriendId.toLowerCase();
    const key = Object.keys($chatStore.messages).find(k => k.toLowerCase() === targetId);
    return key ? ($chatStore.messages[key] || []) : ($chatStore.messages[$chatStore.activeFriendId] || []);
  });

  function handleBackToDialogs() {
    chatStore.selectConversation('');
    isMobileProfileDrawerOpen = false;
  }
</script>

<div class="h-[calc(100vh-60px)] sm:h-[calc(100vh-65px)] flex overflow-hidden bg-[#030e14] text-slate-100 relative">
  <!-- 1. Left Dialogs Sidebar (Full width on mobile if no active conversation, hidden on mobile if conversation is selected) -->
  <div class="w-full md:w-80 shrink-0 h-full {activeConversation ? 'hidden md:flex flex-col' : 'flex flex-col'}">
    <ChatDialogsList
      conversations={$chatStore.conversations}
      activeFriendId={$chatStore.activeFriendId}
    />
  </div>

  <!-- 2. Middle Messages Area & Input (Hidden on mobile if no active conversation, full width if selected) -->
  <section class="flex-1 flex flex-col min-w-0 h-full relative bg-gradient-to-b from-[#04121a] via-[#051721] to-[#030e14] {activeConversation ? 'flex' : 'hidden md:flex'}">
    {#if !activeConversation}
      <!-- Empty State: No Chat Selected -->
      <div class="flex-1 flex flex-col items-center justify-center p-6 text-center select-none">
        <h2 class="text-lg sm:text-xl font-bold text-slate-300 tracking-wide font-display">
          Оберіть користувача, щоб почати листування
        </h2>
        <p class="text-xs text-slate-500 mt-2 max-w-sm">
          Виберіть друга зі списку ліворуч, щоб переглянути історію повідомлень, фотографії та надіслані файли
        </p>
      </div>
    {:else}
      <!-- Top Mobile / Compact Chat Header -->
      <div class="p-3 sm:p-4 border-b border-cyan-500/20 bg-[#061820]/90 flex items-center justify-between gap-3 shrink-0 z-10 backdrop-blur-md">
        <div class="flex items-center gap-2.5 min-w-0">
          <!-- Back button on mobile -->
          <button
            onclick={handleBackToDialogs}
            class="md:hidden p-1.5 rounded-xl bg-[#030e14] border border-cyan-500/30 text-cyan-400 hover:text-white cursor-pointer shrink-0"
            title="Назад до діалогів"
          >
            <ArrowLeft class="w-4 h-4" />
          </button>

          <!-- Avatar & User Info -->
          <div class="w-8 h-8 sm:w-9 sm:h-9 rounded-xl overflow-hidden bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-xs shrink-0">
            {#if activeConversation.friendAvatarUrl}
              <img src={activeConversation.friendAvatarUrl} alt={activeConversation.friendUsername} class="w-full h-full object-cover" />
            {:else}
              {activeConversation.friendUsername.charAt(0).toUpperCase()}
            {/if}
          </div>

          <div class="min-w-0">
            <h3 class="text-xs sm:text-sm font-bold text-white tracking-wide truncate">
              {activeConversation.friendUsername}
            </h3>
            <p class="text-[10px] font-medium {activeConversation.friendStatus === 1 ? 'text-cyan-400' : 'text-slate-400'}">
              {activeConversation.friendStatus === 1 ? 'онлайн' : 'офлайн'}
            </p>
          </div>
        </div>

        <!-- Toggle Profile / Media Drawer on screens < 1280px -->
        <button
          onclick={() => isMobileProfileDrawerOpen = !isMobileProfileDrawerOpen}
          class="xl:hidden p-2 rounded-xl bg-[#09222c] hover:bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 hover:text-white transition-all cursor-pointer flex items-center gap-1.5 text-xs font-bold"
          title="Медіа та профіль"
        >
          <Info class="w-4 h-4 text-cyan-400" />
          <span class="hidden sm:inline">Профіль / Медіа</span>
        </button>
      </div>

      <!-- Active Conversation Message Feed -->
      <ChatMessageList
        conversation={activeConversation}
        messages={activeMessages}
        isLoadingMore={$chatStore.isLoadingMore}
        hasMore={$chatStore.hasMore[activeConversation.friendId] ?? false}
      />

      <!-- Bottom Input Bar -->
      <ChatInputBar
        receiverId={activeConversation.friendId}
      />
    {/if}
  </section>

  <!-- 3. Right Profile & Media Sidebar (Desktop persistent on >= 1280px) -->
  {#if activeConversation}
    <div class="hidden xl:flex w-80 shrink-0 h-full border-l border-cyan-500/20">
      <ChatRightSidebar
        conversation={activeConversation}
      />
    </div>
  {/if}

  <!-- Mobile / Tablet Slide-over Drawer for Right Sidebar (< 1280px) -->
  {#if activeConversation && isMobileProfileDrawerOpen}
    <div class="fixed inset-0 z-50 xl:hidden flex justify-end">
      <!-- Backdrop -->
      <button
        type="button"
        onclick={() => isMobileProfileDrawerOpen = false}
        class="fixed inset-0 bg-black/80 backdrop-blur-sm animate-in fade-in"
        aria-label="Закрити профіль"
      ></button>

      <!-- Sidebar Drawer Panel -->
      <div class="relative w-full max-w-xs sm:max-w-sm h-full bg-[#061820] border-l border-cyan-500/30 shadow-2xl z-10 flex flex-col animate-in slide-in-from-right duration-200">
        <div class="p-3 border-b border-cyan-500/20 flex items-center justify-between bg-[#04121a]">
          <span class="text-xs font-bold text-white uppercase tracking-wider">Інформація та медіа</span>
          <button
            onclick={() => isMobileProfileDrawerOpen = false}
            class="p-1 rounded-lg text-slate-400 hover:text-white"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
        <div class="flex-1 overflow-y-auto">
          <ChatRightSidebar
            conversation={activeConversation}
          />
        </div>
      </div>
    </div>
  {/if}
</div>
