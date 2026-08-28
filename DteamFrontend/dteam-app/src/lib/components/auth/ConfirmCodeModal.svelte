<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { X, ShieldCheck, CheckCircle, Key } from 'lucide-svelte';

  let resetCode = $state('');
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  function closeModal() {
    uiStore.setConfirmCodeModal(false);
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    const trimmedCode = resetCode.trim();

    if (!trimmedCode) {
      errorMessage = 'Введите код подтверждения (GUID)';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.verifyResetCode(trimmedCode);
      uiStore.addToast({
        title: 'Код подтвержден',
        message: 'Успешная верификация!',
        type: 'success'
      });
      closeModal();
      uiStore.setTab('reset-password');
    } catch (err: any) {
      errorMessage = err.message || 'Неверный или истекший код.';
    } finally {
      isSubmitting = false;
    }
  }

  function handleBackdropClick(e: MouseEvent) {
    if (e.target === e.currentTarget) {
      closeModal();
    }
  }
</script>

{#if $uiStore.isConfirmCodeModalOpen}
  <div 
    onclick={handleBackdropClick}
    role="presentation"
    class="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 animate-in fade-in duration-200"
  >
    <div class="relative w-full max-w-md bg-[#09151e] border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80 animate-in zoom-in-95 duration-200">

      <button
        onclick={closeModal}
        class="absolute top-4 right-4 p-2 text-slate-400 hover:text-white rounded-xl hover:bg-slate-800/60 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="text-center mb-6">
        <div class="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 mb-3 shadow-lg shadow-cyan-500/10">
          <ShieldCheck class="w-6 h-6" />
        </div>
        <h2 class="text-xl sm:text-2xl font-extrabold text-white font-display">
          Введите код подтверждения
        </h2>
      </div>

      {#if errorMessage}
        <div class="mb-4 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-6">
        <div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-500">
              <Key class="w-4 h-4" />
            </div>
            <input
              type="text"
              bind:value={resetCode}
              placeholder="3fa85f64-5717-4562-b3fc-2c963f66afa6"
              required
              class="w-full pl-10 pr-4 py-2.5 bg-[#030d12] border border-cyan-500/20 rounded-xl text-sm font-mono text-cyan-300 placeholder-slate-500 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-black border-t-transparent rounded-full animate-spin"></div>
            <span>Проверка...</span>
          {:else}
            <CheckCircle class="w-4 h-4 text-black" />
            <span>Подтвердить</span>
          {/if}
        </button>
      </form>
    </div>
  </div>
{/if}
