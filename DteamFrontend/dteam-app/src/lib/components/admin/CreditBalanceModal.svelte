<script lang="ts">
  import { adminService } from '../../services/adminService';
  import { uiStore } from '../../stores/uiStore';
  import { formatBalance } from '../../utils/formatters';
  import type { Duser } from '../../types';
  import { X, Wallet, PlusCircle, MinusCircle, CheckCircle2 } from 'lucide-svelte';

  interface Props {
    user: Duser | null;
    isOpen: boolean;
    onClose: () => void;
    onBalanceChanged: () => void;
  }

  let { user, isOpen, onClose, onBalanceChanged }: Props = $props();

  type Direction = 'credit' | 'debit';

  let direction = $state<Direction>('credit');
  let amountTon = $state('');
  let reason = $state('');
  let isSubmitting = $state(false);
  let errorMessage = $state('');

  $effect(() => {
    if (isOpen) {
      direction = 'credit';
      amountTon = '';
      reason = '';
      errorMessage = '';
    }
  });

  const currentBalanceLabel = $derived(user ? formatBalance(user.balanceInNanoTons) : '');

  const previewNewBalanceLabel = $derived.by(() => {
    if (!user) return '';
    const parsed = parseFloat(amountTon);
    if (!parsed || isNaN(parsed)) return currentBalanceLabel;

    const amountNanoTons = Math.floor(parsed * 1_000_000_000);
    const signedAmount = direction === 'credit' ? amountNanoTons : -amountNanoTons;
    const current = Number(user.balanceInNanoTons) || 0;
    const next = Math.max(0, current + signedAmount);
    return formatBalance(next);
  });

  async function handleSubmit(e: Event) {
    e.preventDefault();
    if (!user) return;

    const parsed = parseFloat(amountTon);
    if (!parsed || parsed <= 0 || isNaN(parsed)) {
      errorMessage = 'Введіть коректну суму більшу за нуль.';
      return;
    }

    isSubmitting = true;
    errorMessage = '';

    try {
      const amountNanoTons = Math.floor(parsed * 1_000_000_000);
      const signedAmount = direction === 'credit' ? amountNanoTons : -amountNanoTons;

      await adminService.creditUserBalance(user.id, {
        amountInNanoTons: signedAmount,
        reason: reason.trim() || undefined,
      });

      uiStore.addToast({
        title: direction === 'credit' ? 'Кошти нараховано' : 'Кошти списано',
        message: `${direction === 'credit' ? 'Нараховано' : 'Списано'} ${amountTon} TON для '${user.username}'.`,
        type: 'success',
      });

      onBalanceChanged();
      onClose();
    } catch (err: any) {
      errorMessage = err.message || 'Не вдалося змінити баланс користувача.';
    } finally {
      isSubmitting = false;
    }
  }
</script>

{#if isOpen && user}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-in fade-in">
    <div class="relative w-full max-w-md bg-[#061820] border border-cyan-500/30 rounded-2xl shadow-2xl shadow-cyan-950/50 p-6">
      <div class="flex items-center justify-between pb-4 border-b border-cyan-950">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-cyan-500/20 to-emerald-500/20 border border-cyan-500/40 flex items-center justify-center text-cyan-400">
            <Wallet class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-base font-bold text-white tracking-wide">Нарахування коштів</h3>
            <p class="text-xs text-slate-400">{user.username}</p>
          </div>
        </div>
        <button
          onclick={onClose}
          class="p-2 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800/60 transition-colors cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <form onsubmit={handleSubmit} class="py-4 space-y-4">
        {#if errorMessage}
          <div class="p-3 rounded-xl bg-rose-950/50 border border-rose-500/40 text-rose-300 text-xs flex items-center gap-2">
            <span>⚠</span>
            <span>{errorMessage}</span>
          </div>
        {/if}

        <div class="flex items-center justify-between p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20">
          <span class="text-xs text-slate-400">Поточний баланс</span>
          <span class="text-sm font-bold text-cyan-300 font-mono">{currentBalanceLabel}</span>
        </div>

        <div class="grid grid-cols-2 gap-2">
          <button
            type="button"
            onclick={() => (direction = 'credit')}
            class="flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer border
              {direction === 'credit'
                ? 'bg-emerald-500/15 border-emerald-500/50 text-emerald-300'
                : 'bg-[#0a232c] border-slate-800 text-slate-400 hover:text-slate-200'}"
          >
            <PlusCircle class="w-4 h-4" />
            Нарахувати
          </button>
          <button
            type="button"
            onclick={() => (direction = 'debit')}
            class="flex items-center justify-center gap-2 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer border
              {direction === 'debit'
                ? 'bg-rose-500/15 border-rose-500/50 text-rose-300'
                : 'bg-[#0a232c] border-slate-800 text-slate-400 hover:text-slate-200'}"
          >
            <MinusCircle class="w-4 h-4" />
            Списати
          </button>
        </div>

        <div>
          <label for="credit-amount" class="block text-xs font-semibold text-slate-300 mb-1">Сума (TON)</label>
          <div class="relative">
            <Wallet class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-cyan-400" />
            <input
              id="credit-amount"
              type="number"
              step="0.1"
              min="0"
              placeholder="0.00"
              bind:value={amountTon}
              required
              class="w-full pl-9 pr-3 py-2.5 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-sm text-cyan-300 font-mono placeholder-slate-600"
            />
          </div>
        </div>

        <div>
          <label for="credit-reason" class="block text-xs font-semibold text-slate-300 mb-1">Причина (опційно)</label>
          <input
            id="credit-reason"
            type="text"
            placeholder="Наприклад: компенсація, бонус, промо"
            bind:value={reason}
            class="w-full px-3 py-2 rounded-xl bg-[#0a232c] border border-cyan-500/20 focus:border-cyan-400 focus:outline-none text-xs text-slate-200 placeholder-slate-600"
          />
        </div>

        <div class="flex items-center justify-between p-3.5 rounded-xl bg-[#081e26] border border-cyan-500/20">
          <span class="text-xs text-slate-400">Баланс після операції</span>
          <span class="text-sm font-bold text-white font-mono">{previewNewBalanceLabel}</span>
        </div>

        <div class="flex items-center justify-end gap-3 pt-2">
          <button
            type="button"
            onclick={onClose}
            class="px-4 py-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-xs font-semibold text-slate-300 transition-colors cursor-pointer"
          >
            Скасувати
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            class="flex items-center gap-2 px-5 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-bold text-xs tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer disabled:opacity-50"
          >
            {#if isSubmitting}
              <span class="animate-spin">⟳</span>
              <span>Збереження...</span>
            {:else}
              <CheckCircle2 class="w-4 h-4" />
              <span>Підтвердити</span>
            {/if}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}