<script lang="ts">
  import { onMount } from 'svelte';
  import { Sparkles, RefreshCw, Activity, UserPlus, Gamepad2, ShoppingCart } from 'lucide-svelte';
  import { activityStore } from '../../stores/activityStore';
  import { uiStore } from '../../stores/uiStore';
  import ActivityCard from './ActivityCard.svelte';
  import type { UserActivity } from '../../types/activity';

  let isRefreshing = $state(false);

  onMount(() => {
    activityStore.loadFriendsActivities();
  });

  async function handleRefresh() {
    isRefreshing = true;
    try {
      await activityStore.loadFriendsActivities(true);
      uiStore.addToast({
        title: 'Оновлено',
        message: 'Стрічку активностей оновлено',
        type: 'info'
      });
    } catch {
      // Handled in store
    } finally {
      isRefreshing = false;
    }
  }

  function formatDateDivider(isoString: string): string {
    try {
      const d = new Date(isoString);
      const today = new Date();
      if (
        d.getDate() === today.getDate() &&
        d.getMonth() === today.getMonth() &&
        d.getFullYear() === today.getFullYear()
      ) {
        return 'Сьогодні';
      }

      const yesterday = new Date(today);
      yesterday.setDate(today.getDate() - 1);
      if (
        d.getDate() === yesterday.getDate() &&
        d.getMonth() === yesterday.getMonth() &&
        d.getFullYear() === yesterday.getFullYear()
      ) {
        return 'Вчора';
      }

      return d.toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' });
    } catch {
      return '';
    }
  }

  // Group activities by date
  const groupedActivities = $derived.by(() => {
    const list = $activityStore.friendsActivities;
    const groups: { dateLabel: string; items: UserActivity[] }[] = [];
    let currentLabel = '';
    let currentItems: UserActivity[] = [];

    list.forEach((act) => {
      const label = formatDateDivider(act.createdAt);
      if (label !== currentLabel) {
        if (currentItems.length > 0) {
          groups.push({ dateLabel: currentLabel, items: currentItems });
        }
        currentLabel = label;
        currentItems = [act];
      } else {
        currentItems.push(act);
      }
    });

    if (currentItems.length > 0) {
      groups.push({ dateLabel: currentLabel, items: currentItems });
    }

    return groups;
  });
</script>

<div class="space-y-6">
  
  <!-- Feed Header Toolbar -->
  <div class="flex items-center justify-between gap-4 pb-2 border-b border-cyan-950/80">
    <div class="flex items-center gap-2">
      <Sparkles class="w-4 h-4 text-cyan-400" />
      <h2 class="text-sm sm:text-base font-black text-white uppercase tracking-wider font-display">
        Стрічка активності друзів
      </h2>
      <span class="text-xs text-slate-500 font-mono font-bold">
        ({$activityStore.friendsActivities.length})
      </span>
    </div>

    <button
      onclick={handleRefresh}
      disabled={isRefreshing || $activityStore.isLoading}
      class="p-2 rounded-xl bg-[#061820] hover:bg-cyan-950/80 border border-cyan-500/30 text-cyan-300 hover:text-white transition-all cursor-pointer flex items-center gap-1.5 text-xs font-bold"
      title="Оновити стрічку"
    >
      <RefreshCw class="w-3.5 h-3.5 {isRefreshing || $activityStore.isLoading ? 'animate-spin text-cyan-400' : ''}" />
      <span class="hidden sm:inline">Оновити</span>
    </button>
  </div>

  {#if $activityStore.isLoading && $activityStore.friendsActivities.length === 0}
    <!-- Loading Skeleton -->
    <div class="space-y-4 py-8">
      {#each Array(3) as _}
        <div class="p-5 rounded-3xl bg-[#061820]/60 border border-cyan-500/15 animate-pulse space-y-3">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-2xl bg-slate-800"></div>
            <div class="space-y-1.5 flex-1">
              <div class="w-28 h-3.5 bg-slate-800 rounded"></div>
              <div class="w-48 h-2.5 bg-slate-800/60 rounded"></div>
            </div>
          </div>
          <div class="w-full h-24 bg-slate-800/40 rounded-2xl"></div>
        </div>
      {/each}
    </div>

  {:else if $activityStore.friendsActivities.length === 0}
    <!-- Empty State Matching Figma -->
    <div class="py-16 text-center rounded-3xl bg-[#061820]/40 border border-dashed border-cyan-500/20 p-8 space-y-4 max-w-lg mx-auto">
      <div class="w-16 h-16 rounded-3xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center text-cyan-400 mx-auto shadow-lg shadow-cyan-950/50">
        <Activity class="w-8 h-8" />
      </div>
      <div>
        <h3 class="text-base font-black text-white">Активностей поки немає</h3>
        <p class="text-xs text-slate-400 mt-1.5 leading-relaxed">
          Коли ваші друзі купують нові ігри, поповнюють баланс або додають користувачів — їхні події з'являтимуться тут.
        </p>
      </div>
      <div class="flex items-center justify-center gap-3 pt-2">
        <button
          onclick={() => uiStore.setTab('catalog')}
          class="px-4 py-2 rounded-xl bg-cyan-500 hover:bg-cyan-400 text-black text-xs font-black transition-all cursor-pointer flex items-center gap-1.5"
        >
          <ShoppingCart class="w-3.5 h-3.5" />
          <span>Перейти до каталогу</span>
        </button>
      </div>
    </div>

  {:else}
    <!-- Timeline grouped by date -->
    <div class="space-y-6">
      {#each groupedActivities as group (group.dateLabel)}
        <div class="space-y-3">
          
          <!-- Date Divider (Matching Figma Date divider component) -->
          <div class="flex items-center gap-4 py-1">
            <div class="flex-1 h-[1px] bg-gradient-to-r from-transparent via-cyan-500/30 to-transparent"></div>
            <span class="text-[11px] font-black uppercase tracking-wider text-cyan-300 px-3 py-0.5 rounded-full bg-[#061820] border border-cyan-500/20 shadow-sm">
              {group.dateLabel}
            </span>
            <div class="flex-1 h-[1px] bg-gradient-to-r from-transparent via-cyan-500/30 to-transparent"></div>
          </div>

          <!-- Cards in this date group -->
          <div class="space-y-3">
            {#each group.items as activity (activity.id)}
              <ActivityCard {activity} />
            {/each}
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
