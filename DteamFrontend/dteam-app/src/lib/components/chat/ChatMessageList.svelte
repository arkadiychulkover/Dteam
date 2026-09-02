<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { 
    FileText, 
    Download, 
    Check, 
    CheckCheck, 
    Clock, 
    AlertCircle, 
    RotateCw,
    Loader2,
    MoreHorizontal,
    Trash2,
    X
  } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import { currentUser } from '../../stores/authStore';
  import AudioMessagePlayer from './AudioMessagePlayer.svelte';
  import BackendImage from '../ui/BackendImage.svelte';
  import type { ChatMessage, ChatConversation } from '../../types/chat';

  interface Props {
    conversation: ChatConversation;
    messages: ChatMessage[];
    isLoadingMore?: boolean;
    hasMore?: boolean;
  }

  let { conversation, messages = [], isLoadingMore = false, hasMore = false }: Props = $props();

  let scrollContainer: HTMLDivElement | null = null;
  let isNearBottom = true;
  let isTyping = $derived($chatStore.typingUsers[conversation.friendId] ?? false);

  let messageToDelete = $state<ChatMessage | null>(null);
  let isDeleting = $state(false);

  async function handleDeleteConfirm() {
    if (!messageToDelete || isDeleting) return;
    isDeleting = true;
    try {
      await chatStore.deleteMessage(messageToDelete.id, conversation.friendId);
      messageToDelete = null;
    } catch (err) {
      console.error('Failed to delete message:', err);
    } finally {
      isDeleting = false;
    }
  }

  function formatTime(isoString: string): string {
    try {
      const d = new Date(isoString);
      const hours = d.getHours().toString().padStart(2, '0');
      const mins = d.getMinutes().toString().padStart(2, '0');
      return `${hours}:${mins}`;
    } catch {
      return '';
    }
  }

  function formatDateDivider(isoString: string): string {
    try {
      const d = new Date(isoString);
      const today = new Date();
      if (
        d.getDate() === today.getDate() &&
        d.getMonth() === today.getMonth() &&
        d.getFullYear() === today.getFullYear()
      ) {
        return 'Сьогодні';
      }

      const yesterday = new Date(today);
      yesterday.setDate(today.getDate() - 1);
      if (
        d.getDate() === yesterday.getDate() &&
        d.getMonth() === yesterday.getMonth() &&
        d.getFullYear() === yesterday.getFullYear()
      ) {
        return 'Вчора';
      }

      const months = ['січ.', 'лют.', 'берез.', 'квіт.', 'трав.', 'черв.', 'лип.', 'серп.', 'верес.', 'жовт.', 'листоп.', 'груд.'];
      return `${d.getDate()} ${months[d.getMonth()]}`;
    } catch {
      return 'Сьогодні';
    }
  }

  function formatFileSize(bytes?: number | null): string {
    if (!bytes || bytes === 0) return '0 Б';
    const k = 1024;
    const sizes = ['Б', 'КБ', 'МБ', 'ГБ'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  function handleScroll() {
    if (!scrollContainer) return;
    const { scrollTop, scrollHeight, clientHeight } = scrollContainer;
    isNearBottom = scrollHeight - scrollTop - clientHeight < 100;

    // Load more when scrolled to top
    if (scrollTop === 0 && hasMore && !isLoadingMore) {
      const prevHeight = scrollHeight;
      chatStore.loadMoreHistory(conversation.friendId).then(() => {
        tick().then(() => {
          if (scrollContainer) {
            scrollContainer.scrollTop = scrollContainer.scrollHeight - prevHeight;
          }
        });
      });
    }
  }

  export function scrollToBottom(smooth = false) {
    if (!scrollContainer) return;
    scrollContainer.scrollTo({
      top: scrollContainer.scrollHeight,
      behavior: smooth ? 'smooth' : 'auto'
    });
  }

  $effect(() => {
    // Reactively scroll to bottom when messages count changes
    if (messages.length) {
      tick().then(() => {
        if (isNearBottom) {
          scrollToBottom(false);
        }
      });
    }
  });

  onMount(() => {
    tick().then(() => {
      scrollToBottom(false);
    });
  });

  // Group messages by date
  const groupedMessages = $derived.by(() => {
    const groups: { dateLabel: string; items: ChatMessage[] }[] = [];
    let currentLabel = '';
    let currentItems: ChatMessage[] = [];

    messages.forEach((msg) => {
      const label = formatDateDivider(msg.createdAt);
      if (label !== currentLabel) {
        if (currentItems.length > 0) {
          groups.push({ dateLabel: currentLabel, items: currentItems });
        }
        currentLabel = label;
        currentItems = [msg];
      } else {
        currentItems.push(msg);
      }
    });

    if (currentItems.length > 0) {
      groups.push({ dateLabel: currentLabel, items: currentItems });
    }

    return groups;
  });
</script>

<div
  bind:this={scrollContainer}
  onscroll={handleScroll}
  class="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6 select-text"
>
  <!-- Loading More Spinner at Top -->
  {#if isLoadingMore}
    <div class="flex justify-center py-2">
      <Loader2 class="w-5 h-5 text-cyan-400 animate-spin" />
    </div>
  {/if}

  {#if messages.length === 0}
    <div class="h-64 flex flex-col items-center justify-center text-center text-slate-500 text-xs">
      <p class="font-medium text-slate-400">Повідомлень поки немає</p>
      <p class="text-[11px] text-slate-500 mt-1">Напишіть перше повідомлення, щоб почати бесіду</p>
    </div>
  {:else}
    {#each groupedMessages as group}
      <!-- Date Divider -->
      <div class="flex items-center justify-center gap-3 my-6 select-none">
        <div class="h-px w-16 sm:w-32 bg-gradient-to-r from-transparent to-cyan-500/20"></div>
        <span class="text-[11px] font-semibold text-slate-400/80 px-2 tracking-wider">
          {group.dateLabel}
        </span>
        <div class="h-px w-16 sm:w-32 bg-gradient-to-l from-transparent to-cyan-500/20"></div>
      </div>

      <!-- Messages Group -->
      {#each group.items as msg (msg.id || msg.clientMessageId)}
        {@const isOutgoing = msg.senderId === $currentUser?.id}

        <div class="flex items-center gap-1.5 {isOutgoing ? 'justify-end' : 'justify-start'} group relative">
          <!-- 3 dots button on hover for outgoing messages -->
          {#if isOutgoing && !msg.isOptimistic && !msg.isFailed}
            <button
              type="button"
              onclick={() => { messageToDelete = msg; }}
              class="opacity-0 group-hover:opacity-100 transition-opacity p-1.5 rounded-lg hover:bg-slate-800/80 text-slate-400 hover:text-rose-400 cursor-pointer select-none shrink-0"
              title="Видалити повідомлення"
            >
              <MoreHorizontal class="w-3.5 h-3.5" />
            </button>
          {/if}

          <!-- Outgoing Outer Timestamp & Status Checks -->
          {#if isOutgoing}
            <div class="flex items-center gap-1 text-[11px] font-mono text-slate-400/80 mb-1 select-none shrink-0">
              {#if msg.isFailed}
                <span class="text-rose-400 flex items-center gap-1" title="Помилка відправки">
                  <AlertCircle class="w-3.5 h-3.5" />
                </span>
              {:else if msg.isOptimistic}
                <Clock class="w-3 h-3 text-slate-400 animate-pulse" title="Відправлення..." />
              {:else if msg.status === 2}
                <CheckCheck class="w-3.5 h-3.5 text-cyan-400" title="Прочитано" />
              {:else}
                <Check class="w-3.5 h-3.5 text-slate-400" title="Відправлено" />
              {/if}
              <span>{formatTime(msg.createdAt)}</span>
            </div>
          {/if}

          <!-- Message Bubble Container -->
          <div
            class="max-w-md sm:max-w-lg rounded-2xl overflow-hidden shadow-md transition-all
              {isOutgoing 
                ? 'bg-gradient-to-br from-[#0e483e] to-[#0c3e35] text-slate-100 border border-emerald-500/30 rounded-br-sm' 
                : 'bg-gradient-to-br from-[#102d38] to-[#0e2730] text-slate-100 border border-cyan-500/25 rounded-bl-sm'}"
          >
            <!-- 1. Voice Message -->
            {#if msg.type === 3 && msg.mediaUrl}
              <div class="p-3">
                <AudioMessagePlayer
                  src={msg.mediaUrl}
                  messageId={msg.id}
                  durationSeconds={msg.duration || 10}
                  {isOutgoing}
                />
              </div>

            <!-- 2. Photo / Image Message -->
            {:else if msg.type === 1 && msg.mediaUrl}
              <div class="flex flex-col">
                <a
                  href={msg.mediaUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  class="block max-w-sm max-h-80 overflow-hidden bg-black/40"
                >
                  <BackendImage
                    src={msg.mediaUrl}
                    alt="Вкладене фото"
                    class="w-full h-auto object-cover max-h-80 hover:scale-102 transition-transform duration-200"
                  />
                </a>
                {#if msg.content}
                  <div class="px-3.5 py-2.5 text-xs text-slate-100 break-words leading-relaxed">
                    {msg.content}
                  </div>
                {/if}
              </div>

            <!-- 3. File / Document Message -->
            {:else if msg.type === 2 && msg.mediaUrl}
              <a
                href={msg.mediaUrl}
                download={msg.originalFileName}
                target="_blank"
                class="flex items-center gap-3 p-3.5 hover:bg-white/5 transition-colors group/file cursor-pointer"
              >
                <!-- File Icon -->
                <div class="w-10 h-10 rounded-xl bg-cyan-400/20 text-cyan-300 flex items-center justify-center shrink-0 group-hover/file:bg-cyan-400 group-hover/file:text-black transition-colors">
                  <FileText class="w-5 h-5" />
                </div>
                <div class="flex-1 min-w-0 pr-2">
                  <div class="text-xs font-bold text-white truncate group-hover/file:text-cyan-300 transition-colors">
                    {msg.originalFileName || 'Документ'}
                  </div>
                  <div class="text-[10px] text-slate-300 font-mono mt-0.5">
                    {formatFileSize(msg.fileSize)}
                  </div>
                </div>
                <div class="text-slate-400 group-hover/file:text-cyan-400 shrink-0 p-1">
                  <Download class="w-4 h-4" />
                </div>
              </a>
              {#if msg.content}
                <div class="px-3.5 pb-2.5 text-xs text-slate-100 break-words leading-relaxed">
                  {msg.content}
                </div>
              {/if}

            <!-- 4. Plain Text Message -->
            {:else}
              <div class="px-4 py-2.5 text-xs text-slate-100 break-words leading-relaxed whitespace-pre-wrap">
                {msg.content}
              </div>
            {/if}
          </div>

          <!-- Incoming Outer Timestamp -->
          {#if !isOutgoing}
            <div class="text-[11px] font-mono text-slate-400/80 mb-1 select-none shrink-0">
              {formatTime(msg.createdAt)}
            </div>
          {/if}
        </div>
      {/each}
    {/each}
  {/if}

  <!-- Typing Indicator -->
  {#if isTyping}
    <div class="flex items-center gap-2 text-xs text-cyan-400/90 pl-1 select-none animate-in fade-in">
      <span class="font-bold">{conversation.friendUsername}</span>
      <span>друкує</span>
      <span class="flex items-center gap-0.5">
        <span class="w-1 h-1 rounded-full bg-cyan-400 animate-bounce"></span>
        <span class="w-1 h-1 rounded-full bg-cyan-400 animate-bounce [animation-delay:0.2s]"></span>
        <span class="w-1 h-1 rounded-full bg-cyan-400 animate-bounce [animation-delay:0.4s]"></span>
      </span>
    </div>
  {/if}
</div>

<!-- Delete Message Confirmation Modal -->
{#if messageToDelete}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-md animate-in fade-in duration-150">
    <!-- Click outside backdrop to close -->
    <button
      type="button"
      aria-label="Закрити"
      onclick={() => { if (!isDeleting) messageToDelete = null; }}
      class="absolute inset-0 w-full h-full cursor-default bg-transparent"
    ></button>

    <div 
      class="relative w-full max-w-sm bg-slate-900 border border-slate-700/80 rounded-2xl p-5 shadow-2xl space-y-4 text-slate-100 animate-in zoom-in-95 duration-150 z-10"
    >
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2.5 text-rose-400 font-bold text-sm">
          <div class="w-8 h-8 rounded-xl bg-rose-500/15 border border-rose-500/20 flex items-center justify-center">
            <Trash2 class="w-4 h-4" />
          </div>
          <span>Видалити повідомлення?</span>
        </div>
        <button
          type="button"
          onclick={() => { if (!isDeleting) messageToDelete = null; }}
          class="p-1 rounded-lg hover:bg-slate-800 text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <!-- Preview message snippet -->
      {#if messageToDelete.content}
        <div class="px-3.5 py-2.5 rounded-xl bg-slate-950/70 border border-slate-800 text-xs text-slate-300 line-clamp-3 italic">
          «{messageToDelete.content}»
        </div>
      {:else if messageToDelete.type === 1}
        <div class="px-3.5 py-2 rounded-xl bg-slate-950/70 border border-slate-800 text-xs text-slate-400">
          📷 Вкладене фото
        </div>
      {:else if messageToDelete.type === 2}
        <div class="px-3.5 py-2 rounded-xl bg-slate-950/70 border border-slate-800 text-xs text-slate-400">
          📄 {messageToDelete.originalFileName || 'Документ'}
        </div>
      {:else if messageToDelete.type === 3}
        <div class="px-3.5 py-2 rounded-xl bg-slate-950/70 border border-slate-800 text-xs text-slate-400">
          🎙️ Голосове повідомлення
        </div>
      {/if}

      <p class="text-xs text-slate-400 leading-relaxed">
        Ви впевнені, що хочете видалити це повідомлення? Його буде видалено для вас і для <span class="text-cyan-300 font-medium">{conversation.friendUsername}</span>.
      </p>

      <div class="flex items-center justify-end gap-2.5 pt-2 border-t border-slate-800/80">
        <button
          type="button"
          onclick={() => { messageToDelete = null; }}
          disabled={isDeleting}
          class="px-4 py-2 rounded-xl text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition-all cursor-pointer disabled:opacity-50"
        >
          Скасувати
        </button>
        <button
          type="button"
          onclick={handleDeleteConfirm}
          disabled={isDeleting}
          class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-semibold bg-rose-500/20 text-rose-300 border border-rose-500/40 hover:bg-rose-600 hover:text-white transition-all cursor-pointer shadow-lg shadow-rose-950/30 disabled:opacity-50"
        >
          {#if isDeleting}
            <Loader2 class="w-3.5 h-3.5 animate-spin" />
            <span>Видалення...</span>
          {:else}
            <Trash2 class="w-3.5 h-3.5" />
            <span>Видалити</span>
          {/if}
        </button>
      </div>
    </div>
  </div>
{/if}
