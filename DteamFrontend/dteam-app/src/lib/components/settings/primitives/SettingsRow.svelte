<script lang="ts">
  import type { Snippet } from 'svelte';

  interface Props {
    title: string;
    description?: string;
    children?: Snippet;
  }
  let { title, description, children }: Props = $props();
</script>

<div class="flex items-center justify-between gap-4 py-2.5 px-3 rounded-xl hover:bg-cyan-950/30 transition-colors">
  <div class="flex-1 min-w-0 pr-4">
    <div class="text-sm font-semibold text-slate-200 truncate">{title}</div>
    {#if description}
      <div class="text-xs text-slate-400 mt-0.5 leading-snug">{description}</div>
    {/if}
  </div>
  {#if children}
    <div class="flex items-center gap-3 shrink-0">
      {@render children()}
    </div>
  {/if}
</div>
