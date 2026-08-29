<script lang="ts">
  import { X, UserPlus, Loader2, Send, Copy, Check, Fingerprint } from 'lucide-svelte';
  import { friendsStore } from '../../stores/friendsStore';
  import { currentUser } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';

  let { isOpen = $bindable(false) } = $props<{ isOpen: boolean }>();

  let usernameOrId = $state('');
  let isSubmitting = $state(false);
  let isCopied = $state(false);

  async function handleSend() {
    if (!usernameOrId.trim()) return;

    isSubmitting = true;
    try {
      const success = await friendsStore.sendRequest(usernameOrId.trim());
      if (success) {
        usernameOrId = '';
        isOpen = false;
      }
    } finally {
      isSubmitting = false;
    }
  }

  async function copyMyId() {
    if (!$currentUser?.id) return;
    try {
      await navigator.clipboard.writeText($currentUser.id);
      isCopied = true;
      setTimeout(() => isCopied = false, 2500);
      uiStore.addToast({
        title: 'ID скопійовано',
        message: 'Ваш ID скопійовано в буфер обміну.',
        type: 'success'
      });
    } catch (e) {
      console.warn('Copy failed', e);
    }
  }

  function handleClose() {
    isOpen = false;
    usernameOrId = '';
  }
</script>

{#if isOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in duration-200">
    <div class="relative w-full max-w-md bg-[#051a22]/95 backdrop-blur-2xl border border-cyan-500/30 rounded-3xl p-6 sm:p-7 shadow-2xl shadow-cyan-950/80 text-slate-200">
      <button
        onclick={handleClose}
        class="absolute top-5 right-5 p-2 rounded-xl bg-[#031218] hover:bg-cyan-500/20 text-slate-400 hover:text-white border border-cyan-500/20 hover:border-cyan-500/40 transition-all cursor-pointer"
      >
        <X class="w-4 h-4" />
      </button>

      <div class="flex items-center gap-3.5 mb-5">
        <div class="w-11 h-11 rounded-2xl bg-gradient-to-br from-cyan-400 to-teal-500 flex items-center justify-center text-black shadow-lg shadow-cyan-500/25 shrink-0">
          <UserPlus class="w-5 h-5" />
        </div>
        <div>
          <h2 class="text-xl font-black text-white font-display tracking-tight">
            Додати друга
          </h2>
          <p class="text-xs text-slate-400 mt-0.5">
            Знайдіть гравця за нікнеймом або персональним ID
          </p>
        </div>
      </div>

      {#if $currentUser}
        <div class="p-3 rounded-2xl bg-[#031218] border border-cyan-500/20 flex items-center justify-between text-xs mb-4">
          <div class="min-w-0 pr-2">
            <span class="text-[10px] text-slate-400 block font-semibold">Ваш персональний ID:</span>
            <span class="font-mono text-[#0df2c9] font-bold truncate block select-all text-[11px]">{$currentUser.id}</span>
          </div>
          <button
            type="button"
            onclick={copyMyId}
            class="px-2.5 py-1 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 text-[#0df2c9] hover:text-white border border-cyan-500/30 text-[11px] font-bold flex items-center gap-1 cursor-pointer transition-all shrink-0"
          >
            {#if isCopied}
              <Check class="w-3 h-3 text-emerald-400" />
              <span class="text-emerald-400">Скопійовано</span>
            {:else}
              <Copy class="w-3 h-3" />
              <span>Копіювати</span>
            {/if}
          </button>
        </div>
      {/if}

      <form onsubmit={(e) => { e.preventDefault(); handleSend(); }} class="space-y-4">
        <div>
          <label for="friend-input" class="block text-xs font-bold text-slate-300 mb-1.5">
            Нікнейм або ID користувача
          </label>
          <div class="relative">
            <input
              id="friend-input"
              type="text"
              bind:value={usernameOrId}
              placeholder="Наприклад: GamerPro або guid"
              class="w-full pl-4 pr-10 py-3 rounded-2xl bg-[#031218] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(13,242,201,0.25)] focus:outline-none text-white text-xs transition-all"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting || !usernameOrId.trim()}
          class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-teal-400 to-cyan-400 hover:from-teal-300 hover:to-cyan-300 disabled:opacity-50 disabled:cursor-not-allowed text-black font-black text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2 mt-2"
        >
          {#if isSubmitting}
            <Loader2 class="w-4 h-4 animate-spin text-black" />
            <span>Надсилання запиту...</span>
          {:else}
            <Send class="w-4 h-4 text-black" />
            <span>Надіслати запит у друзі</span>
          {/if}
        </button>
      </form>
    </div>
  </div>
{/if}
