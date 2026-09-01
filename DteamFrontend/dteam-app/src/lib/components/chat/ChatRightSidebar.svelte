<script lang="ts">
  import { 
    Bell, 
    Image as ImageIcon, 
    FileText, 
    Mic, 
    UserMinus, 
    RotateCcw, 
    Ban, 
    AlertTriangle 
  } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import { friendsStore } from '../../stores/friendsStore';
  import { uiStore } from '../../stores/uiStore';
  import ChatPhotosGallery from './ChatPhotosGallery.svelte';
  import ChatFilesGallery from './ChatFilesGallery.svelte';
  import ChatVoiceGallery from './ChatVoiceGallery.svelte';
  import type { ChatConversation } from '../../types/chat';

  interface Props {
    conversation?: ChatConversation | null;
  }

  let { conversation }: Props = $props();

  const isNotificationsOn = $derived(
    conversation ? ($chatStore.notificationsEnabled[conversation.friendId] ?? true) : true
  );

  const mediaSummary = $derived($chatStore.mediaSummary);

  function handleToggleNotifications() {
    if (conversation) {
      chatStore.toggleNotifications(conversation.friendId);
      uiStore.addToast({
        title: 'Сповіщення',
        message: !isNotificationsOn ? 'Сповіщення увімкнено' : 'Сповіщення вимкнено',
        type: 'info'
      });
    }
  }

  async function handleClearHistory() {
    if (!conversation) return;
    if (confirm('Ви впевнені, що хочете очистити історію цього діалогу?')) {
      await chatStore.clearHistory(conversation.friendId);
      uiStore.addToast({
        title: 'Історія очищена',
        message: 'Повідомлення видалено для вашого облікового запису',
        type: 'info'
      });
    }
  }

  async function handleRemoveFriend() {
    if (!conversation) return;
    if (confirm(`Видалити ${conversation.friendUsername} зі списку друзів?`)) {
      try {
        await friendsStore.removeFriend(conversation.friendId);
        uiStore.addToast({
          title: 'Друзі',
          message: `${conversation.friendUsername} видалено з друзів`,
          type: 'info'
        });
      } catch (e) {
        console.warn('Remove friend failed:', e);
      }
    }
  }

  function handleBlock() {
    if (!conversation) return;
    uiStore.addToast({
      title: 'Блокування',
      message: `Користувача ${conversation.friendUsername} заблоковано`,
      type: 'warning'
    });
  }

  function handleReport() {
    if (!conversation) return;
    uiStore.addToast({
      title: 'Скарга',
      message: `Скаргу на ${conversation.friendUsername} надіслано модераторам`,
      type: 'error'
    });
  }
</script>

<aside class="w-80 h-full bg-[#061820]/95 border-l border-cyan-500/20 flex flex-col shrink-0 overflow-hidden relative backdrop-blur-xl">
  {#if !conversation}
    <div class="h-full flex items-center justify-center p-6 text-center text-slate-500 text-xs">
      Оберіть діалог для перегляду інформації
    </div>
  {:else if $chatStore.rightSidebarTab === 'photos'}
    <ChatPhotosGallery photosByMonth={mediaSummary?.photosByMonth} />
  {:else if $chatStore.rightSidebarTab === 'files'}
    <ChatFilesGallery filesByMonth={mediaSummary?.filesByMonth} />
  {:else if $chatStore.rightSidebarTab === 'voice'}
    <ChatVoiceGallery voiceByMonth={mediaSummary?.voiceByMonth} />
  {:else}
    <!-- Main Profile View -->
    <div class="flex-1 overflow-y-auto p-5 space-y-6">
      <!-- Profile Card with Big Avatar -->
      <div class="flex flex-col items-center text-center">
        <div class="relative w-40 h-40 rounded-3xl overflow-hidden p-1 bg-gradient-to-b from-cyan-400/40 via-teal-500/20 to-transparent border border-cyan-400/40 shadow-[0_0_25px_rgba(0,242,254,0.15)] group">
          {#if conversation.friendAvatarUrl}
            <img
              src={conversation.friendAvatarUrl}
              alt={conversation.friendUsername}
              class="w-full h-full object-cover rounded-2xl group-hover:scale-105 transition-transform duration-300"
            />
          {:else}
            <div class="w-full h-full rounded-2xl bg-gradient-to-br from-cyan-900 to-slate-900 flex items-center justify-center text-cyan-300 text-4xl font-black font-display">
              {conversation.friendUsername.charAt(0).toUpperCase()}
            </div>
          {/if}
        </div>

        <h2 class="mt-4 text-base font-bold text-white tracking-wide">
          {conversation.friendUsername}
        </h2>
        <span class="text-xs font-medium {conversation.friendStatus === 1 ? 'text-cyan-400' : 'text-slate-400'} mt-0.5">
          {conversation.friendStatus === 1 ? 'онлайн' : 'офлайн'}
        </span>
      </div>

      <!-- Settings / Notifications Toggle -->
      <div class="p-3 rounded-2xl bg-[#09222c]/80 border border-cyan-500/20 flex items-center justify-between shadow-sm">
        <div class="flex items-center gap-3 text-xs font-semibold text-slate-200">
          <Bell class="w-4 h-4 text-cyan-400" />
          <span>Сповіщення</span>
        </div>

        <!-- Custom Switch -->
        <button
          type="button"
          onclick={handleToggleNotifications}
          class="w-11 h-6 rounded-full p-0.5 transition-colors cursor-pointer relative shadow-inner
            {isNotificationsOn ? 'bg-rose-500' : 'bg-slate-700'}"
          title="Перемкнути сповіщення"
        >
          <div
            class="w-5 h-5 rounded-full bg-white shadow-md transition-transform duration-200
              {isNotificationsOn ? 'translate-x-5' : 'translate-x-0'}"
          ></div>
        </button>
      </div>

      <!-- Media Categories with Badges -->
      <div class="space-y-2">
        <!-- Photos -->
        <button
          type="button"
          onclick={() => chatStore.setRightSidebarTab('photos')}
          class="w-full flex items-center justify-between p-3 rounded-2xl bg-[#09222c]/80 hover:bg-[#0d2e3b] border border-cyan-500/20 hover:border-cyan-400/50 transition-all group cursor-pointer shadow-sm"
        >
          <div class="flex items-center gap-3 text-xs font-semibold text-slate-200 group-hover:text-white">
            <ImageIcon class="w-4 h-4 text-cyan-400" />
            <span>Фото</span>
          </div>
          <span class="px-2 py-0.5 rounded-full bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 text-[11px] font-bold">
            {mediaSummary?.totalPhotos ?? 0}
          </span>
        </button>

        <!-- Files -->
        <button
          type="button"
          onclick={() => chatStore.setRightSidebarTab('files')}
          class="w-full flex items-center justify-between p-3 rounded-2xl bg-[#09222c]/80 hover:bg-[#0d2e3b] border border-cyan-500/20 hover:border-cyan-400/50 transition-all group cursor-pointer shadow-sm"
        >
          <div class="flex items-center gap-3 text-xs font-semibold text-slate-200 group-hover:text-white">
            <FileText class="w-4 h-4 text-cyan-400" />
            <span>Файли</span>
          </div>
          <span class="px-2 py-0.5 rounded-full bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 text-[11px] font-bold">
            {mediaSummary?.totalFiles ?? 0}
          </span>
        </button>

        <!-- Voice Messages -->
        <button
          type="button"
          onclick={() => chatStore.setRightSidebarTab('voice')}
          class="w-full flex items-center justify-between p-3 rounded-2xl bg-[#09222c]/80 hover:bg-[#0d2e3b] border border-cyan-500/20 hover:border-cyan-400/50 transition-all group cursor-pointer shadow-sm"
        >
          <div class="flex items-center gap-3 text-xs font-semibold text-slate-200 group-hover:text-white">
            <Mic class="w-4 h-4 text-cyan-400" />
            <span>Голосові повідомлення</span>
          </div>
          <span class="px-2 py-0.5 rounded-full bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 text-[11px] font-bold">
            {mediaSummary?.totalVoiceMessages ?? 0}
          </span>
        </button>
      </div>

      <!-- Action Buttons -->
      <div class="space-y-1 pt-2 border-t border-cyan-500/20">
        <!-- Remove Friend -->
        <button
          type="button"
          onclick={handleRemoveFriend}
          class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800/50 transition-all cursor-pointer"
        >
          <UserMinus class="w-4 h-4 text-slate-400" />
          <span>Видалити з друзів</span>
        </button>

        <!-- Clear History -->
        <button
          type="button"
          onclick={handleClearHistory}
          class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800/50 transition-all cursor-pointer"
        >
          <RotateCcw class="w-4 h-4 text-slate-400" />
          <span>Очистити історію</span>
        </button>

        <!-- Block -->
        <button
          type="button"
          onclick={handleBlock}
          class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800/50 transition-all cursor-pointer"
        >
          <Ban class="w-4 h-4 text-slate-400" />
          <span>Заблокувати</span>
        </button>

        <!-- Report -->
        <button
          type="button"
          onclick={handleReport}
          class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-medium text-rose-400 hover:text-rose-300 hover:bg-rose-950/30 transition-all cursor-pointer"
        >
          <AlertTriangle class="w-4 h-4 text-rose-400" />
          <span>Поскаржитись</span>
        </button>
      </div>
    </div>
  {/if}
</aside>
