<script lang="ts">
  import { onMount } from 'svelte';
  import { 
    X, 
    Copy, 
    Check, 
    Coins, 
    ArrowRight, 
    CheckCircle2, 
    AlertCircle, 
    Loader2, 
    ExternalLink, 
    ClipboardPaste, 
    Sparkles,
    ShieldCheck,
    Wallet
  } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
  import { authStore, currentUser } from '../../stores/authStore';
  import { paymentService } from '../../services/paymentService';
  import { formatTon, nanoTonToTon } from '../../utils/formatters';
  import TonIcon from '../ui/TonIcon.svelte';

  let depositAddress = $state('Завантаження адреси...');
  let amount = $state<number>(1);
  let txhHash = $state('');
  let isCopied = $state(false);
  let isLoading = $state(false);
  let errorMessage = $state<string | null>(null);
  let successResult = $state<{
    message: string;
    amount: number;
    newBalance: number;
    txhHash: string;
  } | null>(null);

  const presets = [0.5, 1, 3, 5, 10, 25];

  onMount(async () => {
    try {
      depositAddress = await paymentService.getDepositAddress();
    } catch (e) {
      console.warn('Failed to load deposit address', e);
    }
  });

  async function copyAddress() {
    if (!depositAddress || depositAddress.includes('Завантаження')) return;
    try {
      await navigator.clipboard.writeText(depositAddress);
      isCopied = true;
      setTimeout(() => {
        isCopied = false;
      }, 2500);
    } catch (e) {
      console.warn('Copy failed', e);
    }
  }

  async function pasteHash() {
    try {
      const text = await navigator.clipboard.readText();
      if (text) {
        txhHash = text.trim();
      }
    } catch (e) {
      console.warn('Paste failed', e);
    }
  }

  function handlePreset(val: number) {
    amount = val;
    errorMessage = null;
  }

  async function handleVerify() {
    errorMessage = null;
    successResult = null;

    if (!amount || amount <= 0) {
      errorMessage = 'Будь ласка, вкажіть коректну суму поповнення (більше 0 TON).';
      return;
    }

    if (!txhHash.trim()) {
      errorMessage = 'Будь ласка, введіть хеш транзакції (TX Hash або BOC) після переказу.';
      return;
    }

    isLoading = true;
    try {
      const result = await paymentService.verifyTransaction(txhHash.trim(), amount);
      if (result.success) {
        authStore.updateBalance((result.amount || amount) * 1_000_000_000);
        successResult = {
          message: result.message || 'Транзакцію успішно підтверджено!',
          amount: result.amount || amount,
          newBalance: result.newBalanceInNanoTons,
          txhHash: result.txhHash || txhHash.trim()
        };
        uiStore.addToast({
          title: 'Баланс поповнено!',
          message: `Успішно зараховано ${amount} TON на ваш рахунок.`,
          type: 'success'
        });
      } else {
        errorMessage = result.message || 'Не вдалося верифікувати транзакцію в мережі TON.';
      }
    } catch (err: any) {
      errorMessage = err.message || 'Помилка при перевірці транзакції. Переконайтеся, що переказ виконано в мережі TON.';
    } finally {
      isLoading = false;
    }
  }

  function handleClose() {
    uiStore.setDepositModal(false);
    errorMessage = null;
    successResult = null;
    txhHash = '';
  }
</script>

<div class="fixed inset-0 z-50 flex items-center justify-center p-4 overflow-y-auto bg-black/80 backdrop-blur-md animate-in fade-in duration-200">
  <div class="relative w-full max-w-lg max-h-[90vh] overflow-y-auto bg-[#09151e]/95 backdrop-blur-2xl border border-cyan-500/30 rounded-3xl p-6 sm:p-8 shadow-2xl shadow-cyan-950/80 text-slate-200">
    
    <div class="absolute -top-24 -right-24 w-60 h-60 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-24 -left-24 w-60 h-60 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none"></div>

    <button
      onclick={handleClose}
      class="absolute top-5 right-5 p-2 rounded-xl bg-slate-900/60 hover:bg-cyan-500/20 text-slate-400 hover:text-white border border-slate-700/50 hover:border-cyan-500/40 transition-all cursor-pointer z-10"
      title="Закрити"
    >
      <X class="w-4 h-4" />
    </button>

    {#if successResult}
      
      <div class="text-center py-4 space-y-5 animate-in zoom-in-95 duration-300">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-3xl bg-gradient-to-br from-emerald-400 to-cyan-500 text-black shadow-lg shadow-emerald-500/30 mx-auto animate-bounce">
          <CheckCircle2 class="w-9 h-9 stroke-[2.5]" />
        </div>

        <div>
          <h2 class="text-2xl sm:text-3xl font-black text-white font-display tracking-tight">
            Оплата успішна!
          </h2>
          <p class="text-xs sm:text-sm text-slate-400 mt-1.5">
            {successResult.message}
          </p>
        </div>

        <div class="p-4 rounded-2xl bg-[#061820] border border-emerald-500/30 text-left space-y-2">
          <div class="flex items-center justify-between text-xs">
            <span class="text-slate-400">Сума поповнення:</span>
            <span class="text-emerald-400 font-bold font-mono text-sm">+{successResult.amount} TON</span>
          </div>
          <div class="flex items-center justify-between text-xs">
            <span class="text-slate-400">Поточний баланс:</span>
            <span class="text-white font-bold font-mono text-sm">
              {$currentUser ? formatTon(nanoTonToTon($currentUser.balanceInNanoTons)) : `${successResult.amount} TON`}
            </span>
          </div>
          <div class="flex items-center justify-between text-xs pt-1 border-t border-slate-800">
            <span class="text-slate-500 text-[11px]">TX Hash:</span>
            <span class="text-slate-400 font-mono text-[10px] truncate max-w-[200px]" title={successResult.txhHash}>
              {successResult.txhHash}
            </span>
          </div>
        </div>

        <button
          onclick={handleClose}
          class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-cyan-500 via-teal-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2"
        >
          <span>Чудово, повернутися</span>
          <ArrowRight class="w-4 h-4" />
        </button>
      </div>

    {:else}
      
      <div class="flex items-center gap-3.5 mb-6">
        <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 flex items-center justify-center text-black shadow-lg shadow-cyan-500/25 shrink-0">
          <Coins class="w-6 h-6" />
        </div>
        <div>
          <h2 class="text-xl sm:text-2xl font-black text-white font-display tracking-tight flex items-center gap-2">
            Поповнення балансу <span class="text-cyan-400 text-xs px-2 py-0.5 rounded-md bg-cyan-950/80 border border-cyan-500/30 font-mono">TON</span>
          </h2>
          <p class="text-xs text-slate-400 mt-0.5">
            Миттєве поповнення через смарт-контракт або прямий переказ
          </p>
        </div>
      </div>

      {#if $currentUser}
        <div class="flex items-center justify-between px-3.5 py-2.5 rounded-xl bg-[#061820] border border-cyan-500/20 mb-5 text-xs">
          <div class="flex items-center gap-2 text-slate-400">
            <Wallet class="w-3.5 h-3.5 text-cyan-400" />
            <span>Ваш поточний баланс:</span>
          </div>
          <span class="font-bold text-white font-mono flex items-center gap-1.5 text-sm">
            <TonIcon class="w-4 h-4 text-cyan-400" />
            <span>{formatTon(nanoTonToTon($currentUser.balanceInNanoTons))}</span>
          </span>
        </div>
      {/if}

      {#if errorMessage}
        <div class="mb-5 p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-300 text-xs flex items-start gap-2.5 animate-in fade-in">
          <AlertCircle class="w-4 h-4 text-red-400 shrink-0 mt-0.5" />
          <span class="leading-relaxed">{errorMessage}</span>
        </div>
      {/if}

      <div class="space-y-5 text-xs">
        
        <div>
          <div class="flex items-center justify-between mb-1.5">
            <span class="font-bold text-slate-300 flex items-center gap-1.5">
              <span class="w-4 h-4 rounded-full bg-cyan-500/20 text-cyan-400 text-[10px] font-black flex items-center justify-center">1</span>
              Адреса гаманця для переказу:
            </span>
            <span class="text-[10px] text-cyan-400/80 font-mono flex items-center gap-1">
              <ShieldCheck class="w-3 h-3 text-cyan-400" /> TON Mainnet
            </span>
          </div>

          <div class="relative flex items-center bg-[#051117] border border-cyan-500/30 rounded-2xl p-2 pl-3 group hover:border-cyan-400/60 transition-colors">
            <div class="flex-1 font-mono text-[11px] text-cyan-300 truncate select-all pr-2" title={depositAddress}>
              {depositAddress}
            </div>
            <button
              type="button"
              onclick={copyAddress}
              class="px-3 py-1.5 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-300 hover:text-white border border-cyan-500/30 transition-all flex items-center gap-1.5 cursor-pointer font-bold shrink-0"
              title="Скопіювати адресу"
            >
              {#if isCopied}
                <Check class="w-3.5 h-3.5 text-emerald-400" />
                <span class="text-emerald-400 text-[11px]">Скопійовано!</span>
              {:else}
                <Copy class="w-3.5 h-3.5" />
                <span class="text-[11px]">Копіювати</span>
              {/if}
            </button>
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-1.5">
            <span class="font-bold text-slate-300 flex items-center gap-1.5">
              <span class="w-4 h-4 rounded-full bg-cyan-500/20 text-cyan-400 text-[10px] font-black flex items-center justify-center">2</span>
              Сума поповнення (TON):
            </span>
            <span class="text-[11px] text-slate-400">
              ≈ {(amount * 1_000_000_000).toLocaleString()} NanoTONs
            </span>
          </div>

          <div class="relative">
            <input
              type="number"
              min="0.001"
              step="0.1"
              bind:value={amount}
              placeholder="1.0"
              class="w-full pl-4 pr-16 py-2.5 rounded-2xl bg-[#051117] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(13,242,201,0.2)] focus:outline-none text-white text-sm font-bold font-mono transition-all"
            />
            <span class="absolute right-4 top-1/2 -translate-y-1/2 font-black text-xs text-cyan-400 font-mono">
              TON
            </span>
          </div>

          <div class="flex flex-wrap gap-1.5 mt-2">
            {#each presets as p}
              <button
                type="button"
                onclick={() => handlePreset(p)}
                class="px-2.5 py-1 rounded-lg text-[11px] font-bold font-mono transition-all cursor-pointer
                  {amount === p 
                    ? 'bg-cyan-500 text-black font-black shadow-md shadow-cyan-500/20' 
                    : 'bg-[#061820] hover:bg-cyan-950/60 text-slate-300 border border-cyan-500/20 hover:border-cyan-400/40'}"
              >
                +{p} TON
              </button>
            {/each}
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-1.5">
            <span class="font-bold text-slate-300 flex items-center gap-1.5">
              <span class="w-4 h-4 rounded-full bg-cyan-500/20 text-cyan-400 text-[10px] font-black flex items-center justify-center">3</span>
              Хеш транзакції (TX Hash або BOC):
            </span>
          </div>

          <div class="relative flex items-center">
            <input
              type="text"
              bind:value={txhHash}
              class="w-full pl-4 pr-20 py-2.5 rounded-2xl bg-[#051117] border border-cyan-500/30 focus:border-cyan-400 focus:shadow-[0_0_15px_rgba(13,242,201,0.2)] focus:outline-none text-white text-xs font-mono transition-all"
            />
            <button
              type="button"
              onclick={pasteHash}
              class="absolute right-2 px-2.5 py-1 rounded-lg bg-cyan-500/10 hover:bg-cyan-500/20 text-cyan-300 hover:text-white border border-cyan-500/30 transition-all text-[10px] font-bold flex items-center gap-1 cursor-pointer"
              title="Вставити з буфера"
            >
              <ClipboardPaste class="w-3 h-3" />
              <span>Вставити</span>
            </button>
          </div>
          <p class="text-[10px] text-slate-500 mt-1.5 leading-normal">
            💡 Надішліть вказану суму TON з вашого гаманця (Tonkeeper, MyTonWallet, Telegram Wallet) на вказану вище адресу, скопіюйте хеш транзакції та вставте сюди.
          </p>
        </div>

        <button
          type="button"
          onclick={handleVerify}
          disabled={isLoading || !txhHash.trim() || amount <= 0}
          class="w-full py-3.5 rounded-2xl bg-gradient-to-r from-cyan-500 via-teal-500 to-emerald-500 hover:from-cyan-400 hover:to-emerald-400 disabled:opacity-50 disabled:cursor-not-allowed text-black font-black text-sm tracking-wide shadow-lg shadow-cyan-500/25 transition-all cursor-pointer flex items-center justify-center gap-2 mt-4"
        >
          {#if isLoading}
            <Loader2 class="w-4 h-4 animate-spin text-black" />
            <span>Перевірка в блокчейні TON...</span>
          {:else}
            <Sparkles class="w-4 h-4 text-black" />
            <span>Підтвердити та зарахувати {amount} TON</span>
          {/if}
        </button>
      </div>
    {/if}
  </div>
</div>
