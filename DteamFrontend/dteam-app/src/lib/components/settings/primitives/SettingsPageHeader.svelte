<script lang="ts">
  interface Props {
    title: string;
    subtitle?: string;
  }
  let { title, subtitle }: Props = $props();
</script>

<div class="mb-6 pb-2 border-b border-cyan-900/20">
  <h2 class="text-2xl font-black tracking-tight text-white font-display flex items-center gap-2">
    {title}
  </h2>
  {#if subtitle}
    <p class="text-xs text-slate-400 mt-1 font-medium leading-relaxed">
      {subtitle}
    </p>
  {/if}
</div>
