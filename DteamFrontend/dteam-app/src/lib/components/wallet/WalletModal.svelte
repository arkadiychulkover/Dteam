<script lang="ts">
  import { uiStore } from '../../stores/uiStore';
  import { walletStore } from '../../stores/walletStore';
  import { authStore, currentUser } from '../../stores/authStore';
  import { formatPrice, formatAddress } from '../../utils/formatters';
  import { X, Wallet, ArrowDownToLine, ArrowUpFromLine, CheckCircle2, Coins } from 'lucide-svelte';

  let depositAmount = $state('5.0');

  function close() {
    uiStore.setWalletModal(false);
  }

  function handleDeposit() {
    const tons = parseFloat(depositAmount);
    if (!isNaN(tons) && tons > 0) {
      walletStore.deposit(tons);
      uiStore.addToast({
        title: 'Balance Topped Up',
        message: `Successfully added ${tons} TON to your account.`,
        type: 'success',
      });
      close();
    }
  }
</script>

{#if $uiStore.isWalletModalOpen}
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-fade-in">
    <div class="relative w-full max-w-md bg-[#121524] border border-slate-700 rounded-2xl shadow-2xl p-6">
      <!-- Close Button -->
      <button
        onclick={close}
        class="absolute top-4 right-4 p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer"
      >
        <X class="w-5 h-5" />
      </button>

      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-cyan-500/20 text-cyan-400 flex items-center justify-center">
          <Wallet class="w-5 h-5" />
        </div>
        <div>
          <h3 class="text-lg font-bold text-white">TON Wallet & Balance</h3>
          <span class="text-xs text-slate-400">Decentralized Gaming Treasury</span>
        </div>
      </div>

      <!-- Wallet Status Box -->
      <div class="p-4 rounded-xl bg-slate-900/90 border border-slate-800 space-y-3">
        <div class="flex items-center justify-between">
          <span class="text-xs text-slate-400 font-medium">Connected Address:</span>
          <span class="text-xs font-mono font-bold text-cyan-400 flex items-center gap-1">
            <CheckCircle2 class="w-3.5 h-3.5 text-emerald-400" />
            {formatAddress($walletStore.address)}
          </span>
        </div>

        <div class="flex items-center justify-between border-t border-slate-800 pt-3">
          <span class="text-xs text-slate-400 font-medium">Available in Dteam:</span>
          <span class="text-base font-black text-cyan-300 font-mono">
            {formatPrice($currentUser?.balanceInNanoTons ?? 0)}
          </span>
        </div>
      </div>

      <!-- Quick Deposit Simulation -->
      <div class="mt-6 space-y-3">
        <label for="depositInput" class="block text-xs font-bold text-slate-300 uppercase tracking-wider">
          Top up Balance (TON)
        </label>
        <div class="relative">
          <input
            id="depositInput"
            type="number"
            step="0.5"
            min="0.1"
            bind:value={depositAmount}
            class="w-full px-4 py-3 rounded-xl bg-slate-900 border border-slate-700 text-white font-mono text-sm focus:border-cyan-500 focus:outline-none"
          />
          <span class="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-bold text-cyan-400">
            TON
          </span>
        </div>

        <div class="flex gap-2">
          {#each ['1.0', '5.0', '10.0', '25.0'] as quickVal}
            <button
              onclick={() => depositAmount = quickVal}
              class="flex-1 py-1.5 rounded-lg bg-slate-800/80 hover:bg-slate-700 text-xs font-mono text-slate-300 border border-slate-700 transition-colors cursor-pointer"
            >
              +{quickVal}
            </button>
          {/each}
        </div>

        <button
          onclick={handleDeposit}
          class="w-full mt-4 py-3.5 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold text-sm shadow-lg shadow-cyan-500/20 transition-all cursor-pointer flex items-center justify-center gap-2"
        >
          <ArrowDownToLine class="w-4 h-4" />
          <span>Confirm TON Deposit</span>
        </button>
      </div>
    </div>
  </div>
{/if}
