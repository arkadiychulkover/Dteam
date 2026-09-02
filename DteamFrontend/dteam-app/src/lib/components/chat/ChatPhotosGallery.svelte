<script lang="ts">
  import { ArrowLeft, Image as ImageIcon, X } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import type { ChatMonthGroup, ChatMediaItem } from '../../types/chat';

  interface Props {
    photosByMonth?: ChatMonthGroup[];
  }

  let { photosByMonth = [] }: Props = $props();

  let selectedImage = $state<ChatMediaItem | null>(null);

  function handleBack() {
    chatStore.setRightSidebarTab('profile');
  }
</script>

<div class="h-full flex flex-col">
  <!-- Header -->
  <div class="p-4 border-b border-cyan-500/20 flex items-center gap-3">
    <button
      onclick={handleBack}
      class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800/60 transition-all cursor-pointer"
      title="Назад до профілю"
    >
      <ArrowLeft class="w-5 h-5 text-cyan-400" />
    </button>
    <h3 class="text-sm font-bold text-white tracking-wide flex items-center gap-2">
      <span>Фото</span>
    </h3>
  </div>

  <!-- Content List -->
  <div class="flex-1 overflow-y-auto p-4 space-y-6">
    {#if !photosByMonth || photosByMonth.length === 0}
      <div class="h-48 flex flex-col items-center justify-center text-slate-500 text-xs">
        <ImageIcon class="w-8 h-8 mb-2 opacity-40 text-cyan-400" />
        <p>Немає надісланих фотографій</p>
      </div>
    {:else}
      {#each photosByMonth as group}
        <div class="space-y-3">
          <!-- Month Separator -->
          <div class="flex items-center gap-3">
            <div class="h-px flex-1 bg-cyan-500/20"></div>
            <span class="text-xs font-semibold text-slate-400 tracking-wider">
              {group.monthLabel}
            </span>
            <div class="h-px flex-1 bg-cyan-500/20"></div>
          </div>

          <!-- 3-column Grid -->
          <div class="grid grid-cols-3 gap-2">
            {#each group.items as item}
              <button
                type="button"
                onclick={() => selectedImage = item}
                class="aspect-square rounded-xl overflow-hidden bg-slate-900/60 border border-cyan-500/20 hover:border-cyan-400 hover:shadow-[0_0_12px_rgba(13,242,201,0.3)] transition-all cursor-pointer group relative"
              >
                <img
                  src={item.mediaUrl}
                  alt={item.originalFileName}
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                  loading="lazy"
                />
              </button>
            {/each}
          </div>
        </div>
      {/each}
    {/if}
  </div>
</div>

<!-- Image Lightbox Modal -->
{#if selectedImage}
  <div class="fixed inset-0 z-50 bg-black/85 backdrop-blur-md flex items-center justify-center p-4">
    <button
      onclick={() => selectedImage = null}
      class="absolute top-5 right-5 p-2.5 rounded-full bg-slate-900/80 border border-cyan-500/30 text-slate-300 hover:text-white hover:border-cyan-400 transition-all cursor-pointer"
      title="Закрити"
    >
      <X class="w-6 h-6" />
    </button>
    <div class="max-w-4xl max-h-[85vh] rounded-2xl overflow-hidden border border-cyan-500/30 shadow-2xl bg-black">
      <img
        src={selectedImage.mediaUrl}
        alt={selectedImage.originalFileName}
        class="max-w-full max-h-[85vh] object-contain"
      />
    </div>
  </div>
{/if}
