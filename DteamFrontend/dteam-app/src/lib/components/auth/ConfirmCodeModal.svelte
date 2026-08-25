<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { authStore } from '../../stores/authStore';
  import { X, ShieldCheck, CheckCircle, RefreshCw } from 'lucide-svelte';

  let codeDigits = $state(['', '', '', '', '', '']);
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  function closeModal() {
    uiStore.setConfirmCodeModal(false);
  }

  function handleInput(index: number, e: Event) {
    const target = e.target as HTMLInputElement;
    const val = target.value;
    
    if (val.length > 0) {
      codeDigits[index] = val[val.length - 1];
      if (index < 5) {
        const nextInput = document.getElementById(`modal-code-input-${index + 1}`) as HTMLInputElement;
        if (nextInput) nextInput.focus();
      }
    }
  }

  function handleKeyDown(index: number, e: KeyboardEvent) {
    if (e.key === 'Backspace' && !codeDigits[index] && index > 0) {
      const prevInput = document.getElementById(`modal-code-input-${index - 1}`) as HTMLInputElement;
      if (prevInput) {
        prevInput.focus();
        codeDigits[index - 1] = '';
      }
    }
  }

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    const fullCode = codeDigits.join('');

    if (fullCode.length < 6) {
      errorMessage = 'Введите 6-значный код';
      return;
    }

    errorMessage = '';
    isSubmitting = true;

    try {
      await authStore.verifyResetCode(fullCode);
      uiStore.addToast({
        title: 'Код подтвержден',
        message: 'Успешная верификация!',
        type: 'success'
      });
      closeModal();
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
    class="fixed inset-0 z-50 bg-black/75 backdrop-blur-md flex items-center justify-center p-4 animate-in fade-in duration-200"
  >
    <div class="relative w-full max-w-md bg-[#111422] border border-slate-700/80 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/50 animate-in zoom-in-95 duration-200">

      <button
        onclick={closeModal}
        class="absolute top-4 right-4 p-2 text-slate-400 hover:text-white rounded-xl hover:bg-slate-800 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="text-center mb-6">
        <div class="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 mb-3 shadow-lg shadow-cyan-500/10">
          <ShieldCheck class="w-6 h-6" />
        </div>
        <h2 class="text-xl sm:text-2xl font-extrabold text-white font-['Outfit']">
          Введите код подтверждения
        </h2>
      </div>

      {#if errorMessage}
        <div class="mb-4 p-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs font-medium flex items-center gap-2">
          <span>⚠️</span> {errorMessage}
        </div>
      {/if}

      <form onsubmit={handleSubmit} class="space-y-6">
        <div class="flex justify-center gap-2">
          {#each codeDigits as digit, i}
            <input
              id="modal-code-input-{i}"
              type="text"
              inputmode="numeric"
              maxlength="1"
              value={digit}
              oninput={(e) => handleInput(i, e)}
              onkeydown={(e) => handleKeyDown(i, e)}
              class="w-10 h-12 bg-slate-900 border border-slate-800 rounded-xl text-center text-lg font-mono font-extrabold text-cyan-400 focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-500/30 shadow-inner"
            />
          {/each}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          class="w-full py-3 px-4 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold text-sm tracking-wide shadow-lg shadow-cyan-500/25 flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          {#if isSubmitting}
            <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
            <span>Проверка...</span>
          {:else}
            <CheckCircle class="w-4 h-4" />
            <span>Подтвердить</span>
          {/if}
        </button>
      </form>
    </div>
  </div>
{/if}
