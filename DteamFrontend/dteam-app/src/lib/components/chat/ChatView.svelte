<script lang="ts">
  import { onMount } from 'svelte';
  import { chatStore } from '../../stores/chatStore';
  import ChatDialogsList from './ChatDialogsList.svelte';
  import ChatMessageList from './ChatMessageList.svelte';
  import ChatInputBar from './ChatInputBar.svelte';
  import ChatRightSidebar from './ChatRightSidebar.svelte';

  onMount(() => {
    chatStore.loadConversations();
  });

  const activeConversation = $derived(
    $chatStore.activeFriendId 
      ? $chatStore.conversations.find(c => c.friendId === $chatStore.activeFriendId) || {
          friendId: $chatStore.activeFriendId,
          friendUsername: 'Співрозмовник',
          friendStatus: 1,
          unreadCount: 0
        }
      : null
  );

  const activeMessages = $derived(
    $chatStore.activeFriendId ? ($chatStore.messages[$chatStore.activeFriendId] || []) : []
  );
</script>

<div class="h-[calc(100vh-65px)] flex overflow-hidden bg-[#030e14] text-slate-100 relative">
  <!-- 1. Left Dialogs Sidebar -->
  <ChatDialogsList
    conversations={$chatStore.conversations}
    activeFriendId={$chatStore.activeFriendId}
  />

  <!-- 2. Middle Messages Area & Input -->
  <section class="flex-1 flex flex-col min-w-0 h-full relative bg-gradient-to-b from-[#04121a] via-[#051721] to-[#030e14]">
    {#if !activeConversation}
      <!-- Empty State: No Chat Selected (Matching mockup Chat - No chat selected.png) -->
      <div class="flex-1 flex flex-col items-center justify-center p-6 text-center select-none">
        <h2 class="text-lg sm:text-xl font-bold text-slate-300 tracking-wide font-display">
          Оберіть користувача, щоб почати листування
        </h2>
        <p class="text-xs text-slate-500 mt-2 max-w-sm">
          Виберіть друга зі списку ліворуч, щоб переглянути історію повідомлень, фотографії та надіслані файли
        </p>
      </div>
    {:else}
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

  <!-- 3. Right Profile & Media Sidebar -->
  {#if activeConversation}
    <ChatRightSidebar
      conversation={activeConversation}
    />
  {/if}
</div>
