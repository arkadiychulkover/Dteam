<script lang="ts">
  import { 
    Gamepad2, 
    ShoppingCart, 
    UserPlus, 
    Coins, 
    Star, 
    Sparkles, 
    Trophy, 
    Heart, 
    Clock, 
    ExternalLink,
    ArrowRight
  } from 'lucide-svelte';
  import { UserActivityType, type UserActivity } from '../../types/activity';
  import { profileStore } from '../../stores/profileStore';
  import { gamesStore } from '../../stores/gamesStore';
  import { uiStore } from '../../stores/uiStore';
  import TonIcon from '../ui/TonIcon.svelte';
  import BackendImage from '../ui/BackendImage.svelte';

  let { activity }: { activity: UserActivity } = $props();

  function formatRelativeTime(isoString: string): string {
    try {
      const d = new Date(isoString);
      const now = new Date();
      const diffSec = Math.floor((now.getTime() - d.getTime()) / 1000);

      if (diffSec < 60) return 'Щойно';
      if (diffSec < 3600) return `${Math.floor(diffSec / 60)} хв тому`;
      if (diffSec < 86400) return `${Math.floor(diffSec / 3600)} год тому`;
      if (diffSec < 172800) return 'Вчора';

      return d.toLocaleDateString('uk-UA', { day: 'numeric', month: 'short' });
    } catch {
      return '';
    }
  }

  function handleOpenUser(e: MouseEvent) {
    e.stopPropagation();
    profileStore.viewProfile(activity.userId);
  }

  function handleOpenGame(gameId?: string | null) {
    if (gameId) {
      gamesStore.selectGame(gameId);
      uiStore.setTab('game');
    }
  }

  const parsedDetails = $derived.by(() => {
    if (!activity.details) return null;
    try {
      return JSON.parse(activity.details);
    } catch {
      return null;
    }
  });
</script>

<div class="p-4 sm:p-5 rounded-3xl bg-[#061820]/90 backdrop-blur-xl border border-cyan-500/20 hover:border-cyan-500/40 transition-all shadow-xl hover:shadow-cyan-950/50 space-y-3.5 group text-slate-100">
  
  <!-- Activity Card Header: User Avatar + Action Meta -->
  <div class="flex items-center justify-between gap-3">
    <div class="flex items-center gap-3 min-w-0">
      
      <!-- User Avatar -->
      <button
        type="button"
        onclick={handleOpenUser}
        class="relative w-10 h-10 rounded-2xl overflow-hidden bg-gradient-to-br from-cyan-400 to-emerald-500 flex items-center justify-center text-black font-black text-sm shrink-0 hover:scale-105 transition-transform cursor-pointer border border-cyan-500/30 shadow-md"
        title="Переглянути профіль"
      >
        {#if activity.userAvatarUrl}
          <img src={activity.userAvatarUrl} alt={activity.username} class="w-full h-full object-cover" />
        {:else}
          {activity.username.charAt(0).toUpperCase()}
        {/if}
      </button>

      <!-- Username & Action -->
      <div class="min-w-0">
        <div class="flex items-center gap-2 flex-wrap">
          <button
            type="button"
            onclick={handleOpenUser}
            class="font-black text-xs sm:text-sm text-white hover:text-cyan-300 transition-colors truncate cursor-pointer"
          >
            {activity.username}
          </button>

          <!-- Type Badge -->
          {#if activity.type === UserActivityType.GamePurchased}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-cyan-500/15 text-cyan-300 border border-cyan-500/30 text-[10px] font-extrabold uppercase tracking-wider">
              <ShoppingCart class="w-3 h-3" />
              <span>Покупка</span>
            </span>
          {:else if activity.type === UserActivityType.FriendAdded}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-emerald-500/15 text-emerald-300 border border-emerald-500/30 text-[10px] font-extrabold uppercase tracking-wider">
              <UserPlus class="w-3 h-3" />
              <span>Дружба</span>
            </span>
          {:else if activity.type === UserActivityType.BalanceDeposited}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-amber-500/15 text-amber-300 border border-amber-500/30 text-[10px] font-extrabold uppercase tracking-wider">
              <Coins class="w-3 h-3" />
              <span>Поповнення</span>
            </span>
          {:else if activity.type === UserActivityType.ReviewPosted}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-rose-500/15 text-rose-300 border border-rose-500/30 text-[10px] font-extrabold uppercase tracking-wider">
              <Star class="w-3 h-3" />
              <span>Рецензія</span>
            </span>
          {:else if activity.type === UserActivityType.GamePublished}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-purple-500/15 text-purple-300 border border-purple-500/30 text-[10px] font-extrabold uppercase tracking-wider">
              <Sparkles class="w-3 h-3" />
              <span>Реліз</span>
            </span>
          {:else}
            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-800 text-slate-300 border border-slate-700 text-[10px] font-extrabold uppercase tracking-wider">
              <Trophy class="w-3 h-3" />
              <span>Подія</span>
            </span>
          {/if}
        </div>

        <p class="text-xs text-slate-300 font-medium mt-0.5 leading-snug">
          {activity.title}
        </p>
      </div>
    </div>

    <!-- Timestamp -->
    <div class="flex items-center gap-1 text-[11px] text-slate-500 shrink-0 font-medium">
      <Clock class="w-3 h-3 text-slate-500" />
      <span>{formatRelativeTime(activity.createdAt)}</span>
    </div>
  </div>

  <!-- Activity Attached Content Details -->
  {#if activity.type === UserActivityType.GamePurchased && activity.imageUrl}
    <div
      role="button"
      tabindex="0"
      onclick={() => handleOpenGame(activity.relatedEntityId)}
      onkeydown={(e) => { if (e.key === 'Enter') handleOpenGame(activity.relatedEntityId); }}
      class="mt-2 rounded-2xl overflow-hidden bg-[#04121a] border border-cyan-500/20 hover:border-cyan-400/80 transition-all cursor-pointer group/game shadow-inner"
    >
      <div class="relative w-full aspect-[21/9] sm:aspect-[16/6] overflow-hidden bg-slate-950">
        <img
          src={activity.imageUrl}
          alt={activity.title}
          class="w-full h-full object-cover group-hover/game:scale-105 transition-transform duration-500"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-[#04121a] via-[#04121a]/30 to-transparent"></div>
        <div class="absolute bottom-2.5 left-3 right-3 flex items-center justify-between gap-2">
          <span class="text-xs sm:text-sm font-extrabold text-white group-hover/game:text-cyan-300 transition-colors drop-shadow truncate">
            {parsedDetails?.gameTitle || 'Переглянути гру в крамниці'}
          </span>
          <span class="text-[10px] px-2.5 py-1 rounded-lg bg-cyan-500/90 text-black font-black uppercase tracking-wider shadow shrink-0 flex items-center gap-1">
            <span>У крамницю</span>
            <ArrowRight class="w-3 h-3" />
          </span>
        </div>
      </div>
    </div>
  {:else if activity.type === UserActivityType.BalanceDeposited}
    <div class="p-3 rounded-2xl bg-[#041620] border border-cyan-500/20 flex items-center justify-between gap-3 text-xs">
      <div class="flex items-center gap-2.5">
        <div class="w-8 h-8 rounded-xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center text-cyan-400">
          <TonIcon class="w-4 h-4 text-cyan-400" />
        </div>
        <div>
          <span class="text-slate-400 text-[10px] block font-bold uppercase tracking-wider">Поповнення TON</span>
          <span class="text-white font-mono font-bold text-xs">{parsedDetails?.amount || '1'} TON зараховано на баланс</span>
        </div>
      </div>
      {#if parsedDetails?.txhHash}
        <span class="text-[10px] font-mono text-cyan-400/80 max-w-[120px] truncate" title={parsedDetails.txhHash}>
          TX: {parsedDetails.txhHash}
        </span>
      {/if}
    </div>
  {:else if activity.description}
    <div class="p-3 rounded-2xl bg-[#04121a] border border-cyan-500/15 text-xs text-slate-300 leading-relaxed">
      {activity.description}
    </div>
  {/if}
</div>
