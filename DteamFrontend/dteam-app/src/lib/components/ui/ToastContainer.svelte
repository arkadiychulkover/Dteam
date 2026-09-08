<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import ToastItem from './ToastItem.svelte';
</script>

<div
  class="fixed bottom-6 right-6 z-50 flex flex-col gap-3 pointer-events-none max-w-sm sm:max-w-md w-full px-3 sm:px-0"
  aria-live="polite"
>
  {#each $uiStore.toasts as toast (toast.id)}
    <ToastItem {toast} onRemove={(id) => uiStore.removeToast(id)} />
  {/each}
</div>
