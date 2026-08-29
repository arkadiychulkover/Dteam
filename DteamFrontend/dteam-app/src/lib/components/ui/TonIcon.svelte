<script lang="ts">
  let { class: className = 'w-4 h-4 text-cyan-400 inline-block' }: { class?: string } = $props();
</script>

<svg
  viewBox="0 0 24 24"
  fill="none"
  stroke="currentColor"
  stroke-width="1.8"
  stroke-linecap="round"
  stroke-linejoin="round"
  class={className}
>
  <path d="M12 2L3 8.5L12 22L21 8.5L12 2Z" fill="currentColor" fill-opacity="0.15" />
  <path d="M12 2L3 8.5L12 14L21 8.5L12 2Z" />
  <path d="M12 14V22" />
  <path d="M3 8.5L12 14L21 8.5" />
</svg>
