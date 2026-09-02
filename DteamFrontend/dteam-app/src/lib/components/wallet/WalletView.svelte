<script lang="ts">
  import { onMount } from 'svelte';
  import { 
    Wallet, 
    ArrowDownLeft, 
    ArrowUpRight, 
    ShoppingBag, 
    Coins, 
    Copy, 
    Check, 
    ExternalLink, 
    RefreshCw, 
    Sparkles, 
    ShieldCheck, 
    Filter,
    Clock,
    CheckCircle2,
    Search
  } from 'lucide-svelte';
  import { currentUser, authStore } from '../../stores/authStore';
  import { uiStore } from '../../stores/uiStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { gamesService } from '../../services/gamesService';
  import { paymentService } from '../../services/paymentService';
  import { formatTon, nanoTonToTon } from '../../utils/formatters';
  import TonIcon from '../ui/TonIcon.svelte';
  import BackendImage from '../ui/BackendImage.svelte';
  import type { UnifiedTransactionItem } from '../../types/payment';

  let transactions = $state<UnifiedTransactionItem[]>([]);
  let isLoading = $state(true);
  let isRefreshing = $state(false);
  let filterType = $state<'all' | 'deposit' | 'purchase'>('all');
  let searchQuery = $state('');
  let depositAmount = $state<number>(1);
  let depositAddress = $state<string>('');
  let isAddressCopied = $state(false);
  let copiedTxId = $state<string | null>(null);

  const presets = [0.5, 1, 3, 5, 10, 25];

  onMount(async () => {
    await Promise.all([
      loadTransactions(),
      loadDepositAddress()
    ]);
  });

  async function loadDepositAddress() {
    try {
      depositAddress = await paymentService.getDepositAddress();
    } catch (e) {
      console.warn('[WalletView] Failed to load deposit address:', e);
    }
  }

  async function loadTransactions() {
    isLoading = true;
    try {
      transactions = await paymentService.getTransactions();
    } catch (e) {
      console.warn('[WalletView] Failed to load transactions:', e);
    } finally {
      isLoading = false;
    }
  }

  async function handleRefresh() {
    isRefreshing = true;
    try {
      transactions = await paymentService.getTransactions();
      if ($currentUser?.id) {
        // Also refresh profile to get updated balance
        await authStore.fetchProfile();
      }
      uiStore.addToast({
        title: 'Оновлено',
        message: 'Історію транзакцій та баланс оновлено',
        type: 'info'
      });
    } catch (e) {
      console.warn('[WalletView] Failed to refresh:', e);
    } finally {
      isRefreshing = false;
    }
  }

  async function copyAddress() {
    if (!depositAddress || depositAddress.includes('Завантаження')) return;
    try {
      await navigator.clipboard.writeText(depositAddress);
      isAddressCopied = true;
      uiStore.addToast({
        title: 'Скопійовано',
        message: 'Адресу для поповнення скопійовано в буфер',
        type: 'success'
      });
      setTimeout(() => {
        isAddressCopied = false;
      }, 2500);
    } catch (e) {
      console.warn('Copy failed', e);
    }
  }

  async function copyTxHash(txHash: string, id: string) {
    try {
      await navigator.clipboard.writeText(txHash);
      copiedTxId = id;
      uiStore.addToast({
        title: 'Скопійовано',
        message: 'Хеш транзакції скопійовано',
        type: 'info'
      });
      setTimeout(() => {
        if (copiedTxId === id) copiedTxId = null;
      }, 2000);
    } catch (e) {
      console.warn('Copy failed', e);
    }
  }

  function handleOpenDepositModal() {
    uiStore.setDepositModal(true);
  }

  function handleOpenGame(gameId?: string | null) {
    if (!gameId) return;
    gamesService.getGameById(gameId).then(game => {
      if (game) {
        gamesStore.selectGame(game);
        uiStore.setTab('game');
      }
    }).catch(err => {
      console.warn('Could not navigate to game:', err);
    });
  }

  function formatDate(isoString: string): string {
    try {
      const d = new Date(isoString);
      const day = d.getDate().toString().padStart(2, '0');
      const month = (d.getMonth() + 1).toString().padStart(2, '0');
      const year = d.getFullYear();
      const hours = d.getHours().toString().padStart(2, '0');
      const mins = d.getMinutes().toString().padStart(2, '0');
      return `${day}.${month}.${year} о ${hours}:${mins}`;
    } catch {
      return '';
    }
  }

  const filteredTransactions = $derived.by(() => {
    let list = transactions;
    if (filterType !== 'all') {
      list = list.filter(t => t.type === filterType);
    }
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      list = list.filter(t => 
        t.title.toLowerCase().includes(q) || 
        (t.txHash && t.txHash.toLowerCase().includes(q))
      );
    }
    return list;
  });

  const totalDepositsCount = $derived(transactions.filter(t => t.type === 'deposit').length);
  const totalPurchasesCount = $derived(transactions.filter(t => t.type === 'purchase').length);
</script>

<div class="min-h-[calc(100vh-65px)] bg-[#030e14] py-8 px-4 sm:px-6 lg:px-8 relative overflow-hidden selection:bg-cyan-500 selection:text-black">
  <!-- Subtle Background Glows -->
  <div class="absolute top-0 left-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>
  <div class="absolute bottom-10 right-1/4 w-96 h-96 bg-teal-500/10 rounded-full blur-3xl pointer-events-none"></div>

  <div class="max-w-6xl mx-auto space-y-8 relative z-10">

    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-cyan-500/20 pb-6">
      <div class="flex items-center gap-3.5">
        <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 flex items-center justify-center text-black shadow-lg shadow-cyan-500/25">
          <Wallet class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-2xl sm:text-3xl font-black text-white tracking-tight font-display flex items-center gap-2">
            Гаманець
            <span class="text-xs px-2.5 py-0.5 rounded-full bg-cyan-500/20 text-cyan-300 border border-cyan-500/30 font-mono font-semibold">
              TON Network
            </span>
          </h1>
          <p class="text-xs sm:text-sm text-slate-400 mt-0.5">
            Керування балансом, поповнення та повна історія транзакцій
          </p>
        </div>
      </div>

      <!-- Quick Actions / Refresh -->
      <div class="flex items-center gap-3">
        <button
          onclick={handleRefresh}
          disabled={isRefreshing}
          class="px-3.5 py-2 rounded-xl bg-[#09222c] hover:bg-cyan-950/80 border border-cyan-500/30 hover:border-cyan-400 text-xs font-bold text-slate-200 transition-all cursor-pointer flex items-center gap-2 shadow-sm disabled:opacity-50"
        >
          <RefreshCw class="w-3.5 h-3.5 text-cyan-400 {isRefreshing ? 'animate-spin' : ''}" />
          <span>{isRefreshing ? 'Оновлення...' : 'Оновити'}</span>
        </button>
      </div>
    </div>

    <!-- Top Grid: Balance Card + Deposit Card (Matching Figma Settings - Account Wallet) -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">

      <!-- 1. Balance Card (5 cols) -->
      <div class="lg:col-span-5 rounded-3xl bg-gradient-to-b from-[#09222c]/90 to-[#05151c]/90 border border-cyan-500/30 p-6 sm:p-7 shadow-xl shadow-cyan-950/30 backdrop-blur-xl flex flex-col justify-between relative overflow-hidden group">
        <div class="absolute -right-10 -bottom-10 w-40 h-40 bg-cyan-500/10 rounded-full blur-2xl group-hover:bg-cyan-500/20 transition-all pointer-events-none"></div>

        <div>
          <div class="flex items-center justify-between">
            <span class="text-xs font-bold text-slate-400 uppercase tracking-wider flex items-center gap-2">
              <Wallet class="w-4 h-4 text-cyan-400" />
              Мій баланс
            </span>
            <div class="w-8 h-8 rounded-xl bg-cyan-500/20 flex items-center justify-center text-cyan-300">
              <TonIcon class="w-4 h-4 text-cyan-400" />
            </div>
          </div>

          <div class="mt-5">
            <div class="flex items-baseline gap-2">
              <span class="text-3xl sm:text-4xl font-black text-white tracking-tight font-mono">
                {$currentUser ? formatTon(nanoTonToTon($currentUser.balanceInNanoTons)) : '0.00'}
              </span>
              <span class="text-lg font-bold text-cyan-400 font-mono">TON</span>
            </div>
            <p class="text-xs text-slate-400 mt-1 font-mono">
              ≈ {$currentUser ? $currentUser.balanceInNanoTons.toLocaleString() : '0'} nanoTON
            </p>
          </div>
        </div>

        <div class="mt-8 pt-5 border-t border-cyan-500/20 space-y-3">
          <div class="flex items-center justify-between text-xs">
            <span class="text-slate-400">Статус гаманця:</span>
            <span class="text-emerald-400 font-semibold flex items-center gap-1.5">
              <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
              Активний (Mainnet)
            </span>
          </div>

          {#if depositAddress}
            <div class="p-3 rounded-2xl bg-[#030e14]/70 border border-cyan-500/20 flex items-center justify-between gap-2">
              <div class="min-w-0 flex-1">
                <p class="text-[10px] text-slate-400 uppercase tracking-wider">Адреса поповнення DTEAM</p>
                <p class="text-xs text-slate-200 font-mono truncate">{depositAddress}</p>
              </div>
              <button
                onclick={copyAddress}
                class="p-2 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-300 hover:text-cyan-200 transition-all cursor-pointer shrink-0"
                title="Скопіювати адресу"
              >
                {#if isAddressCopied}
                  <Check class="w-4 h-4 text-emerald-400" />
                {:else}
                  <Copy class="w-4 h-4 text-cyan-400" />
                {/if}
              </button>
            </div>
          {/if}
        </div>
      </div>

      <!-- 2. Deposit Card (7 cols) -->
      <div class="lg:col-span-7 rounded-3xl bg-gradient-to-b from-[#09222c]/90 to-[#05151c]/90 border border-cyan-500/30 p-6 sm:p-7 shadow-xl shadow-cyan-950/30 backdrop-blur-xl flex flex-col justify-between relative overflow-hidden">
        <div>
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-base sm:text-lg font-bold text-white tracking-wide">
                Поповнення балансу
              </h2>
              <p class="text-xs text-slate-400 mt-0.5">
                Оберіть або введіть суму в TON для миттєвого зарахування
              </p>
            </div>
            <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-cyan-400 to-teal-500 flex items-center justify-center text-black font-black">
              <Coins class="w-4 h-4" />
            </div>
          </div>

          <!-- Quick Presets -->
          <div class="mt-5 space-y-2">
            <label for="deposit-amount-input" class="text-xs font-bold text-slate-300 uppercase tracking-wider">
              Швидкий вибір суми:
            </label>
            <div class="grid grid-cols-3 sm:grid-cols-6 gap-2">
              {#each presets as preset}
                <button
                  type="button"
                  onclick={() => depositAmount = preset}
                  class="py-2 px-3 rounded-xl border text-xs font-bold font-mono transition-all cursor-pointer text-center
                    {depositAmount === preset 
                      ? 'bg-cyan-500 text-black border-cyan-400 shadow-md shadow-cyan-500/30 scale-105' 
                      : 'bg-[#030e14]/70 border-cyan-500/20 text-slate-300 hover:border-cyan-400/60 hover:text-white'}"
                >
                  {preset} TON
                </button>
              {/each}
            </div>
          </div>

          <!-- Custom Amount Input -->
          <div class="mt-4 space-y-2">
            <label for="deposit-amount-input" class="text-xs font-bold text-slate-300 uppercase tracking-wider">
              Або вкажіть довільну суму:
            </label>
            <div class="relative">
              <input
                id="deposit-amount-input"
                type="number"
                min="0.1"
                step="0.1"
                bind:value={depositAmount}
                placeholder="Введіть суму..."
                class="w-full pl-10 pr-16 py-3 rounded-2xl bg-[#030e14]/80 border border-cyan-500/30 focus:border-cyan-400 text-white font-mono text-sm placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-400/20 transition-all"
              />
              <div class="absolute left-3.5 top-1/2 -translate-y-1/2 text-cyan-400">
                <TonIcon class="w-4 h-4 text-cyan-400" />
              </div>
              <span class="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-bold text-slate-400 font-mono">
                TON
              </span>
            </div>
          </div>
        </div>

        <!-- Submit Button -->
        <div class="mt-6 pt-5 border-t border-cyan-500/20 flex flex-col sm:flex-row items-center justify-between gap-3">
          <div class="flex items-center gap-2 text-[11px] text-slate-400">
            <ShieldCheck class="w-4 h-4 text-cyan-400 shrink-0" />
            <span>Безпечна валідація через TonAPI</span>
          </div>

          <button
            onclick={handleOpenDepositModal}
            class="w-full sm:w-auto px-6 py-3 rounded-2xl bg-gradient-to-r from-cyan-400 via-teal-400 to-emerald-400 hover:from-cyan-300 hover:to-emerald-300 text-black font-black text-xs tracking-wider uppercase transition-all cursor-pointer shadow-lg shadow-cyan-500/25 hover:shadow-cyan-500/40 hover:scale-[1.02] flex items-center justify-center gap-2"
          >
            <Sparkles class="w-4 h-4 text-black" />
            <span>Поповнити ({depositAmount || 0} TON)</span>
          </button>
        </div>
      </div>

    </div>

    <!-- Bottom Section: Transaction History -->
    <div class="rounded-3xl bg-[#061820]/90 border border-cyan-500/20 p-6 sm:p-8 backdrop-blur-xl shadow-2xl space-y-6">

      <!-- Header & Filters Bar -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-cyan-500/20 pb-5">
        <div>
          <h2 class="text-lg sm:text-xl font-bold text-white tracking-wide flex items-center gap-2.5">
            <span>Історія транзакцій</span>
            <span class="px-2 py-0.5 rounded-lg bg-cyan-500/20 text-cyan-300 text-xs font-mono font-bold">
              {filteredTransactions.length}
            </span>
          </h2>
          <p class="text-xs text-slate-400 mt-0.5">
            Поповнення балансу та покупки ігор в одному списку
          </p>
        </div>

        <!-- Filter Tabs & Search -->
        <div class="flex flex-wrap items-center gap-3">
          <!-- Filter Tabs -->
          <div class="flex items-center p-1 rounded-2xl bg-[#030e14] border border-cyan-500/20">
            <button
              onclick={() => filterType = 'all'}
              class="px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5
                {filterType === 'all' ? 'bg-cyan-500 text-black shadow-sm' : 'text-slate-400 hover:text-white'}"
            >
              <span>Всі</span>
              <span class="text-[10px] opacity-80 font-mono">({transactions.length})</span>
            </button>

            <button
              onclick={() => filterType = 'deposit'}
              class="px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5
                {filterType === 'deposit' ? 'bg-cyan-500 text-black shadow-sm' : 'text-slate-400 hover:text-white'}"
            >
              <ArrowDownLeft class="w-3.5 h-3.5" />
              <span>Поповнення</span>
              <span class="text-[10px] opacity-80 font-mono">({totalDepositsCount})</span>
            </button>

            <button
              onclick={() => filterType = 'purchase'}
              class="px-3.5 py-1.5 rounded-xl text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5
                {filterType === 'purchase' ? 'bg-cyan-500 text-black shadow-sm' : 'text-slate-400 hover:text-white'}"
            >
              <ShoppingBag class="w-3.5 h-3.5" />
              <span>Покупки</span>
              <span class="text-[10px] opacity-80 font-mono">({totalPurchasesCount})</span>
            </button>
          </div>

          <!-- Search Input -->
          <div class="relative min-w-[200px]">
            <Search class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
            <input
              type="text"
              bind:value={searchQuery}
              placeholder="Пошук за назвою чи хешем..."
              class="w-full pl-8 pr-3 py-1.5 rounded-xl bg-[#030e14] border border-cyan-500/20 focus:border-cyan-400 text-xs text-slate-100 placeholder-slate-500 focus:outline-none"
            />
          </div>
        </div>
      </div>

      <!-- Transaction List -->
      {#if isLoading}
        <!-- Loading Skeletons -->
        <div class="space-y-3">
          {#each Array(4) as _}
            <div class="p-4 rounded-2xl bg-[#09222c]/50 border border-cyan-500/10 animate-pulse flex items-center justify-between">
              <div class="flex items-center gap-3.5">
                <div class="w-10 h-10 rounded-xl bg-slate-800"></div>
                <div class="space-y-1.5">
                  <div class="w-40 h-4 bg-slate-800 rounded"></div>
                  <div class="w-24 h-3 bg-slate-800/60 rounded"></div>
                </div>
              </div>
              <div class="w-20 h-5 bg-slate-800 rounded"></div>
            </div>
          {/each}
        </div>
      {:else if filteredTransactions.length === 0}
        <!-- Empty State -->
        <div class="py-16 flex flex-col items-center justify-center text-center space-y-3 select-none">
          <div class="w-16 h-16 rounded-3xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center text-cyan-400">
            <Clock class="w-8 h-8 opacity-60" />
          </div>
          <h3 class="text-base font-bold text-slate-300">
            {searchQuery ? 'Транзакцій за вашим запитом не знайдено' : 'Історія транзакцій порожня'}
          </h3>
          <p class="text-xs text-slate-500 max-w-sm">
            {searchQuery ? 'Спробуйте змінити пошуковий запит або вибрати інший фільтр' : 'Тут відображатимуться всі ваші поповнення балансу через TON та покупки ігор у крамниці'}
          </p>
          {#if !searchQuery}
            <button
              onclick={handleOpenDepositModal}
              class="mt-2 px-4 py-2 rounded-xl bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-300 border border-cyan-500/30 text-xs font-bold cursor-pointer transition-all"
            >
              Здійснити перше поповнення
            </button>
          {/if}
        </div>
      {:else}
        <!-- Transactions Cards List (Matching Figma Transaction Cards) -->
        <div class="space-y-3">
          {#each filteredTransactions as tx (tx.id)}
            <div class="p-4 rounded-2xl bg-[#09222c]/70 hover:bg-[#0c2b38]/90 border border-cyan-500/20 hover:border-cyan-400/40 transition-all flex flex-col sm:flex-row sm:items-center justify-between gap-3 group">

              <!-- Left Side: Icon / Cover + Info -->
              <div class="flex items-center gap-3.5 min-w-0">
                {#if tx.type === 'deposit'}
                  <!-- Deposit Icon Badge -->
                  <div class="w-11 h-11 rounded-2xl bg-gradient-to-br from-emerald-500/20 to-teal-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400 shrink-0 shadow-sm">
                    <ArrowDownLeft class="w-5 h-5" />
                  </div>
                {:else if tx.type === 'purchase'}
                  <!-- Game Cover Thumbnail or Shopping Bag -->
                  {#if tx.gameCoverUrl}
                    <div class="w-11 h-11 rounded-2xl overflow-hidden shrink-0 border border-cyan-500/30 bg-slate-900">
                      <BackendImage
                        src={tx.gameCoverUrl}
                        alt={tx.title}
                        class="w-full h-full object-cover group-hover:scale-105 transition-transform"
                      />
                    </div>
                  {:else}
                    <div class="w-11 h-11 rounded-2xl bg-gradient-to-br from-rose-500/20 to-pink-500/10 border border-rose-500/30 flex items-center justify-center text-rose-400 shrink-0 shadow-sm">
                      <ShoppingBag class="w-5 h-5" />
                    </div>
                  {/if}
                {/if}

                <div class="min-w-0 flex-1">
                  <!-- Title -->
                  <div class="flex items-center gap-2">
                    {#if tx.type === 'purchase' && tx.gameId}
                      <button
                        onclick={() => handleOpenGame(tx.gameId)}
                        class="text-xs sm:text-sm font-bold text-white hover:text-cyan-300 transition-colors text-left truncate cursor-pointer"
                      >
                        {tx.title}
                      </button>
                    {:else}
                      <span class="text-xs sm:text-sm font-bold text-white truncate">
                        {tx.title}
                      </span>
                    {/if}

                    <span class="text-[10px] px-2 py-0.5 rounded-md font-semibold shrink-0
                      {tx.type === 'deposit' 
                        ? 'bg-emerald-500/15 text-emerald-400 border border-emerald-500/30' 
                        : 'bg-slate-800 text-slate-300 border border-slate-700'}">
                      {tx.type === 'deposit' ? 'Зараховано' : 'Покупка'}
                    </span>
                  </div>

                  <!-- Date & TxHash -->
                  <div class="flex flex-wrap items-center gap-3 mt-1 text-[11px] text-slate-400">
                    <span class="flex items-center gap-1 font-mono">
                      <Clock class="w-3 h-3 text-slate-500" />
                      {formatDate(tx.createdAt)}
                    </span>

                    {#if tx.txHash}
                      <div class="flex items-center gap-1 font-mono text-[10px] text-cyan-400/80">
                        <span>Хеш:</span>
                        <span class="truncate max-w-[120px] sm:max-w-[180px]">{tx.txHash}</span>
                        <button
                          onclick={() => copyTxHash(tx.txHash!, tx.id)}
                          class="p-0.5 rounded hover:bg-cyan-500/20 text-cyan-400 hover:text-cyan-200 transition-all cursor-pointer"
                          title="Скопіювати хеш"
                        >
                          {#if copiedTxId === tx.id}
                            <Check class="w-3 h-3 text-emerald-400" />
                          {:else}
                            <Copy class="w-3 h-3" />
                          {/if}
                        </button>
                      </div>
                    {/if}
                  </div>
                </div>
              </div>

              <!-- Right Side: Amount in TON -->
              <div class="text-right shrink-0 sm:pl-4">
                <div class="text-sm sm:text-base font-black font-mono flex items-center justify-end gap-1
                  {tx.amountTon >= 0 ? 'text-emerald-400' : 'text-slate-200'}">
                  <span>{tx.amountTon >= 0 ? `+${formatTon(tx.amountTon)}` : `${formatTon(tx.amountTon)}`}</span>
                  <span class="text-xs font-bold text-cyan-400">TON</span>
                </div>
                <div class="text-[10px] text-slate-400 font-mono mt-0.5">
                  {tx.amountNanoTon >= 0 ? `+${tx.amountNanoTon.toLocaleString()}` : tx.amountNanoTon.toLocaleString()} nanoTON
                </div>
              </div>

            </div>
          {/each}
        </div>
      {/if}

    </div>

  </div>
</div>
