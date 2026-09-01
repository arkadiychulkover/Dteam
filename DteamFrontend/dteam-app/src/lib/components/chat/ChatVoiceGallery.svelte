<script lang="ts">
  import { ArrowLeft, Mic, MicOff } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import AudioMessagePlayer from './AudioMessagePlayer.svelte';
  import type { ChatMonthGroup } from '../../types/chat';

  interface Props {
    voiceByMonth?: ChatMonthGroup[];
  }

  let { voiceByMonth = [] }: Props = $props();

  function handleBack() {
    chatStore.setRightSidebarTab('profile');
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
      <span>Голосові повідомлення</span>
    </h3>
  </div>

  <!-- Content List -->
  <div class="flex-1 overflow-y-auto p-4 space-y-6">
    {#if !voiceByMonth || voiceByMonth.length === 0}
      <div class="h-48 flex flex-col items-center justify-center text-slate-500 text-xs">
        <MicOff class="w-8 h-8 mb-2 opacity-40 text-cyan-400" />
        <p>Немає надісланих голосових повідомлень</p>
      </div>
    {:else}
      {#each voiceByMonth as group}
        <div class="space-y-3">
          <!-- Month Separator -->
          <div class="flex items-center gap-3">
            <div class="h-px flex-1 bg-cyan-500/20"></div>
            <span class="text-xs font-semibold text-slate-400 tracking-wider">
              {group.monthLabel}
            </span>
            <div class="h-px flex-1 bg-cyan-500/20"></div>
          </div>

          <!-- Voice Items List -->
          <div class="space-y-2">
            {#each group.items as item}
              <div class="p-3 rounded-2xl bg-[#0b242e]/80 border border-cyan-500/20 space-y-2 shadow-sm">
                <!-- Header inside item: Sender + Date -->
                <div class="flex items-center justify-between text-xs">
                  <span class="font-bold text-white tracking-wide truncate max-w-[140px]">
                    {item.senderUsername}
                  </span>
                  <span class="text-[10px] text-slate-400 font-mono">
                    {formatDate(item.createdAt)}
                  </span>
                </div>

                <!-- Custom Audio Player -->
                <AudioMessagePlayer
                  src={item.mediaUrl}
                  messageId={item.messageId}
                  durationSeconds={item.duration || 10}
                />
              </div>
            {/each}
          </div>
        </div>
      {/each}
    {/if}
  </div>
</div>
