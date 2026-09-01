<script lang="ts">
  import { ArrowLeft, FileText, Download, FileCode } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import type { ChatMonthGroup } from '../../types/chat';

  interface Props {
    filesByMonth?: ChatMonthGroup[];
  }

  let { filesByMonth = [] }: Props = $props();

  function handleBack() {
    chatStore.setRightSidebarTab('profile');
  }

  function formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '0 Б';
    const k = 1024;
    const sizes = ['Б', 'КБ', 'МБ', 'ГБ'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  function formatDate(isoString: string): string {
    try {
      const d = new Date(isoString);
      const day = d.getDate().toString().padStart(2, '0');
      const month = (d.getMonth() + 1).toString().padStart(2, '0');
      const year = d.getFullYear();
      const hours = d.getHours().toString().padStart(2, '0');
      const mins = d.getMinutes().toString().padStart(2, '0');
      return `${day}.${month}.${year} о ${hours}:${mins}`;
    } catch {
      return '';
    }
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
      <span>Файли</span>
    </h3>
  </div>

  <!-- Content List -->
  <div class="flex-1 overflow-y-auto p-4 space-y-6">
    {#if !filesByMonth || filesByMonth.length === 0}
      <div class="h-48 flex flex-col items-center justify-center text-slate-500 text-xs">
        <FileCode class="w-8 h-8 mb-2 opacity-40 text-cyan-400" />
        <p>Немає надісланих файлів</p>
      </div>
    {:else}
      {#each filesByMonth as group}
        <div class="space-y-3">
          <!-- Month Separator -->
          <div class="flex items-center gap-3">
            <div class="h-px flex-1 bg-cyan-500/20"></div>
            <span class="text-xs font-semibold text-slate-400 tracking-wider">
              {group.monthLabel}
            </span>
            <div class="h-px flex-1 bg-cyan-500/20"></div>
          </div>

          <!-- Files List -->
          <div class="space-y-2">
            {#each group.items as item}
              <a
                href={item.mediaUrl}
                download={item.originalFileName}
                target="_blank"
                class="flex items-center gap-3 p-3 rounded-2xl bg-[#0b242e]/80 hover:bg-[#0e303d] border border-cyan-500/20 hover:border-cyan-400/60 transition-all group cursor-pointer shadow-sm"
              >
                <!-- File Icon -->
                <div class="w-10 h-10 rounded-xl bg-cyan-500/20 border border-cyan-500/30 flex items-center justify-center text-cyan-400 group-hover:bg-cyan-400 group-hover:text-black group-hover:scale-105 transition-all shrink-0">
                  <FileText class="w-5 h-5" />
                </div>

                <!-- Info -->
                <div class="flex-1 min-w-0">
                  <div class="text-xs font-bold text-white truncate group-hover:text-cyan-300 transition-colors">
                    {item.originalFileName}
                  </div>
                  <div class="text-[11px] text-slate-400 flex items-center gap-2 mt-0.5">
                    <span>{formatFileSize(item.fileSize)}</span>
                    <span>•</span>
                    <span>{formatDate(item.createdAt)}</span>
                  </div>
                </div>

                <!-- Download button -->
                <div class="p-1.5 text-slate-400 group-hover:text-cyan-400 shrink-0">
                  <Download class="w-4 h-4" />
                </div>
              </a>
            {/each}
          </div>
        </div>
      {/each}
    {/if}
  </div>
</div>
