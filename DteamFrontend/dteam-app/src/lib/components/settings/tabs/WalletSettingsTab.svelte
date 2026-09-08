<script lang="ts">
  import { onMount } from 'svelte';
  import { settingsStore } from '../../../stores/settingsStore';
  import { uiStore } from '../../../stores/uiStore';
  import SettingsPageHeader from '../primitives/SettingsPageHeader.svelte';
  import { Wallet, Plus, Loader2, ArrowDownLeft, ArrowUpRight } from 'lucide-svelte';
  import { formatDate } from '../../../utils/formatters';

  const { serverState, uiState } = settingsStore;

  let depositAmount = $state('');

  onMount(() => {
    settingsStore.loadTransactions(1);
  });

  function handleDeposit() {
    uiStore.setDepositModal(true);
  }

  function goToPage(p: number) {
    if (p < 1 || p > $uiState.txTotalPages || p === $uiState.txPage) return;
    settingsStore.loadTransactions(p);
  }
</script>

<div>
  <SettingsPageHeader
    title="Гаманець"
    subtitle="Переглядайте актуальний баланс, поповнюйте рахунок та відстежуйте історію операцій."
  />

  <!-- Hero Balance Card -->
  <div class="mb-7 p-6 rounded-2xl bg-gradient-to-br from-[#022a38] via-[#011f2a] to-[#01141c] border border-cyan-500/20 shadow-xl flex flex-col sm:flex-row items-center justify-between gap-6">
    <div class="flex items-center gap-5">
      <div class="w-16 h-16 rounded-2xl bg-gradient-to-br from-cyan-400 to-emerald-400 p-0.5 shadow-lg shadow-cyan-500/20 flex items-center justify-center shrink-0">
        <div class="w-full h-full rounded-[14px] bg-[#021822] flex items-center justify-center text-cyan-300">
          <Wallet class="w-8 h-8" />
        </div>
      </div>
      <div>
        <div class="text-xs font-mono font-bold uppercase tracking-wider text-slate-400">
          Мій баланс
        </div>
        <div class="text-3xl font-black text-white font-mono tracking-tight flex items-baseline gap-2 mt-0.5">
          <span>{$serverState.walletSummary ? $serverState.walletSummary.formattedBalance.toFixed(2) : '0.00'}</span>
          <span class="text-base font-bold text-cyan-400">{$serverState.walletSummary?.currency || 'TON'}</span>
        </div>
      </div>
    </div>

    <!-- Quick Deposit action -->
    <div class="w-full sm:w-auto flex items-center gap-2.5">
      <input
        type="number"
        placeholder="Сума"
        bind:value={depositAmount}
        class="w-full sm:w-32 px-4 py-2.5 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-500 text-sm focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 font-mono text-center"
      />
      <button
        type="button"
        onclick={handleDeposit}
        class="px-6 py-2.5 rounded-full text-xs font-black bg-[#0df2c9] text-black hover:bg-[#00e5b0] hover:scale-105 active:scale-95 shadow-lg shadow-cyan-500/20 transition-all cursor-pointer whitespace-nowrap flex items-center gap-1.5"
      >
        <Plus class="w-3.5 h-3.5" />
        <span>Поповнити</span>
      </button>
    </div>
  </div>

  <!-- Transactions History List -->
  <div>
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-lg font-black text-white font-display">
        Історія транзакцій
      </h3>
      <span class="text-xs text-slate-400 font-mono">
        Всього операцій: {$uiState.txTotalCount}
      </span>
    </div>

    {#if $uiState.txLoading}
      <div class="py-12 flex flex-col items-center justify-center text-cyan-400 gap-2">
        <Loader2 class="w-6 h-6 animate-spin" />
        <span class="text-xs font-mono text-slate-400">Завантаження історії...</span>
      </div>
    {:else if $uiState.transactions.length === 0}
      <div class="py-12 text-center text-slate-500 text-xs font-mono bg-[#011c27]/40 rounded-2xl border border-cyan-900/20">
        У вас поки що немає фінансових операцій
      </div>
    {:else}
      <!-- Transactions Table/List -->
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b border-cyan-900/30 text-[11px] font-mono uppercase tracking-wider text-slate-400">
              <th class="py-3 px-4">Сума</th>
              <th class="py-3 px-4">Найменування</th>
              <th class="py-3 px-4 text-right">Дата</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-cyan-950/40 text-sm">
            {#each $uiState.transactions as tx}
              {@const isIncome = tx.type.toLowerCase() === 'deposit' || tx.type.toLowerCase() === 'refund'}
              <tr class="hover:bg-cyan-950/20 transition-colors">
                <td class="py-3.5 px-4 font-mono font-bold whitespace-nowrap {isIncome ? 'text-emerald-400' : 'text-slate-300'}">
                  <div class="flex items-center gap-1.5">
                    {#if isIncome}
                      <ArrowDownLeft class="w-3.5 h-3.5 text-emerald-400" />
                      <span>+{tx.amount.toFixed(2)} {tx.currency}</span>
                    {:else}
                      <ArrowUpRight class="w-3.5 h-3.5 text-slate-400" />
                      <span>-{tx.amount.toFixed(2)} {tx.currency}</span>
                    {/if}
                  </div>
                </td>
                <td class="py-3.5 px-4 font-semibold text-slate-100 max-w-md truncate">
                  {tx.title}
                </td>
                <td class="py-3.5 px-4 text-right text-xs font-mono text-slate-400 whitespace-nowrap">
                  {formatDate(tx.date)}
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      {#if $uiState.txTotalPages > 1}
        <div class="mt-6 flex items-center justify-between text-xs font-mono text-slate-400 pt-3 border-t border-cyan-900/20">
          <div>
            Сторінка <span class="text-white font-bold">{$uiState.txPage}</span> з {$uiState.txTotalPages}
          </div>
          <div class="flex items-center gap-2">
            <button
              type="button"
              onclick={() => goToPage($uiState.txPage - 1)}
              disabled={$uiState.txPage <= 1}
              class="px-3 py-1.5 rounded-lg bg-[#011c27] hover:bg-cyan-950/40 border border-cyan-800/40 text-slate-200 disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
            >
              ← Попередня
            </button>
            <button
              type="button"
              onclick={() => goToPage($uiState.txPage + 1)}
              disabled={$uiState.txPage >= $uiState.txTotalPages}
              class="px-3 py-1.5 rounded-lg bg-[#011c27] hover:bg-cyan-950/40 border border-cyan-800/40 text-slate-200 disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
            >
              Наступна →
            </button>
          </div>
        </div>
      {/if}
    {/if}
  </div>
</div>
