<script lang="ts">
import { authStore, currentUser } from '../../stores/authStore';
  import { libraryStore } from '../../stores/libraryStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { formatPrice, formatAddress } from '../../utils/formatters';
  import { UserStatus } from '../../types';
  import { uiStore } from '../../stores/uiStore';
  import {
    User,
    Wallet,
    Coins,
    ShieldCheck,
    Gamepad2,
    Sparkles,
    Users,
    Copy,
    Check,
    Plus
  } from 'lucide-svelte';

  let copied = $state(false);

  function copyWallet() {
    if ($currentUser?.walletAddress) {
      navigator.clipboard.writeText($currentUser.walletAddress);
      copied = true;
      setTimeout(() => copied = false, 2000);
    }
  }
</script>

<div class="max-w-5xl mx-auto px-4 py-8">
  {#if $currentUser}
    <div class="p-6 md:p-8 rounded-3xl bg-slate-900/60 border border-slate-800 backdrop-blur-xl relative overflow-hidden mb-8">
      <div class="flex flex-col md:flex-row items-center md:items-start gap-6">
        <div class="relative">
          <div class="w-24 h-24 rounded-3xl bg-gradient-to-tr from-cyan-500 to-blue-600 p-1 flex items-center justify-center shadow-xl shadow-cyan-500/20">
            {#if $currentUser.avatarUrl}
              <img src={$currentUser.avatarUrl} alt={$currentUser.username} class="w-full h-full object-cover rounded-[20px]" />
            {:else}
              <div class="w-full h-full bg-slate-900 rounded-[20px] flex items-center justify-center text-3xl font-black text-white">
                {$currentUser.username.charAt(0).toUpperCase()}
              </div>
            {/if}
          </div>
          <span class="absolute -bottom-1 -right-1 px-2 py-0.5 rounded-md bg-cyan-950 text-cyan-400 border border-cyan-500/40 text-[10px] font-extrabold uppercase">
            LVL 42
          </span>
        </div>

        <div class="flex-1">
          <div class="flex flex-wrap items-center gap-3">
            <h2 class="text-2xl md:text-3xl font-black text-white font-display">
              {$currentUser.username}
            </h2>
            {#if $currentUser.isAdmin}
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-md bg-amber-500/20 text-amber-300 border border-amber-500/40 text-xs font-bold">
                <ShieldCheck class="w-3.5 h-3.5" /> ADMIN
              </span>
            {/if}
            {#if $currentUser.isInFamily}
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-md bg-blue-500/20 text-blue-300 border border-blue-500/40 text-xs font-bold">
                <Users class="w-3.5 h-3.5" /> Family Member
              </span>
            {/if}
          </div>

          <p class="text-xs text-slate-400 mt-1.5 font-mono">
            {$currentUser.email}
          </p>

          <p class="text-xs text-slate-300 mt-2 max-w-xl">
            {$currentUser.bio || 'Web3 gamer & collector on Dteam.'}
          </p>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-8 pt-6 border-t border-slate-800">
        <div class="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
          <span class="text-[10px] text-slate-400 uppercase font-semibold flex items-center gap-1">
            <Wallet class="w-3.5 h-3.5 text-cyan-400" /> TON Address
          </span>
          <div class="flex items-center justify-between mt-1">
            <span class="text-xs font-mono font-bold text-slate-200 truncate mr-2">
              {formatAddress($currentUser.walletAddress)}
            </span>
            <button
              onclick={copyWallet}
              class="p-1 text-slate-400 hover:text-white transition-colors cursor-pointer"
              title="Copy TON Address"
            >
              {#if copied}
                <Check class="w-3.5 h-3.5 text-emerald-400" />
              {:else}
                <Copy class="w-3.5 h-3.5" />
              {/if}
            </button>
          </div>
        </div>

        <div class="p-4 rounded-xl bg-slate-900/80 border border-slate-800 flex flex-col justify-between">
          <div class="flex items-center justify-between">
            <span class="text-[10px] text-slate-400 uppercase font-semibold flex items-center gap-1">
              <Coins class="w-3.5 h-3.5 text-cyan-400" /> Wallet Balance
            </span>
            <button
              type="button"
              onclick={() => uiStore.setDepositModal(true)}
              class="px-2 py-0.5 rounded-lg bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-300 text-[10px] font-bold border border-cyan-500/30 cursor-pointer transition-all flex items-center gap-1"
            >
              <Plus class="w-3 h-3" /> Поповнити
            </button>
          </div>
          <span class="text-lg font-black text-cyan-400 font-mono block mt-1">
            {formatPrice($currentUser.balanceInNanoTons)}
          </span>
        </div>

        <div class="p-4 rounded-xl bg-slate-900/80 border border-slate-800">
          <span class="text-[10px] text-slate-400 uppercase font-semibold flex items-center gap-1">
            <Sparkles class="w-3.5 h-3.5 text-amber-400" /> Developer Earnings
          </span>
          <span class="text-lg font-black text-amber-300 font-mono block mt-0.5">
            {formatPrice($currentUser.totalEarningsInNanoTons)}
          </span>
        </div>
      </div>
    </div>

    <div>
      <h3 class="text-xl font-bold text-white mb-4 flex items-center gap-2">
        <Gamepad2 class="w-5 h-5 text-cyan-400" /> Published Games & Creations
      </h3>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        {#each ($currentUser as any)?.createdGames || [] as game}
          <div class="p-4 rounded-2xl bg-slate-900/40 border border-slate-800/80 flex items-center gap-4">
            <img src={game.coverUrl} alt={game.title} class="w-16 h-20 object-cover rounded-xl" />
            <div>
              <h4 class="font-bold text-white text-sm">{game.title}</h4>
              <span class="text-xs text-slate-400 font-mono">{formatPrice(game.priceInNanoTons)}</span>
            </div>
          </div>
        {:else}
          <div class="col-span-2 p-8 rounded-2xl border border-dashed border-slate-800 text-center text-slate-500 text-xs">
            No published games found for this profile.
          </div>
        {/each}
      </div>
    </div>
  {/if}
</div>

