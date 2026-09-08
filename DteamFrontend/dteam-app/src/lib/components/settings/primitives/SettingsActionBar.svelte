<script lang="ts">
  import { Loader2 } from 'lucide-svelte';

  interface Props {
    isSaving?: boolean;
    isDirty?: boolean;
    saveText?: string;
    cancelText?: string;
    onsave?: () => void;
    oncancel?: () => void;
  }
  let {
    isSaving = false,
    isDirty = true,
    saveText = 'Зберегти',
    cancelText = 'Скасувати',
    onsave,
    oncancel,
  }: Props = $props();
</script>

<div class="mt-8 pt-4 border-t border-cyan-900/20 flex items-center justify-end gap-3">
  {#if oncancel}
    <button
      type="button"
      onclick={oncancel}
      disabled={isSaving}
      class="px-5 py-2 rounded-full text-xs font-bold text-slate-300 hover:text-white hover:bg-slate-800/40 transition-all cursor-pointer disabled:opacity-50"
    >
      {cancelText}
    </button>
  {/if}
  {#if onsave}
    <button
      type="button"
      onclick={onsave}
      disabled={isSaving || !isDirty}
      class="px-6 py-2 rounded-full text-xs font-black bg-[#0df2c9] text-black hover:bg-[#00e5b0] hover:scale-105 active:scale-95 shadow-lg shadow-cyan-500/20 transition-all cursor-pointer disabled:opacity-50 disabled:hover:scale-100 disabled:cursor-not-allowed flex items-center gap-2"
    >
      {#if isSaving}
        <Loader2 class="w-3.5 h-3.5 animate-spin" />
      {/if}
      <span>{saveText}</span>
    </button>
  {/if}
</div>
