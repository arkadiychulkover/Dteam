<script lang="ts">
  import type { Snippet } from 'svelte';

  interface Props {
    title?: string;
    subtitle?: string;
    children: Snippet;
  }
  let { title, subtitle, children }: Props = $props();
</script>

<div class="mb-7">
  {#if title}
    <div class="mb-3">
      <h3 class="text-sm font-bold text-slate-200 tracking-wide uppercase font-mono flex items-center gap-2">
        {title}
      </h3>
      {#if subtitle}
        <p class="text-xs text-slate-400 mt-0.5">{subtitle}</p>
      {/if}
    </div>
  {/if}
  <div class="space-y-2">
    {@render children()}
  </div>
</div>
