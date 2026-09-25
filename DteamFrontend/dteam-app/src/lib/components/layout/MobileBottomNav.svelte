<script lang="ts">
  import {
    Store,
    Gamepad2,
    MessageSquare,
    ShoppingCart,
    Users,
    User,
  } from 'lucide-svelte';
  import { uiStore } from '../../stores/uiStore';
  import { cartStore } from '../../stores/cartStore';
  import { currentUser } from '../../stores/authStore';

  const cartCount = $derived($cartStore.items.length);

  function navigate(tab: any) {
    if (typeof navigator !== 'undefined' && navigator.vibrate) {
      navigator.vibrate(10);
    }
    uiStore.setTab(tab);
  }
</script>

<nav
  class="fixed bottom-0 inset-x-0 z-40 md:hidden bg-[#030d12]/95 backdrop-blur-xl border-t border-cyan-500/20 px-2 py-1.5 shadow-[0_-10px_25px_rgba(0,0,0,0.8)] pb-[max(0.375rem,env(safe-area-inset-bottom))]"
  aria-label="Мобільна навігація"
>
  <div class="flex items-center justify-around max-w-md mx-auto">

    <button
      type="button"
      onclick={() => navigate('store')}
      class="flex flex-col items-center justify-center py-1 px-3 rounded-xl transition-all cursor-pointer relative group
        {$uiStore.activeTab === 'store' || $uiStore.activeTab === 'catalog' || $uiStore.activeTab === 'game' || $uiStore.activeTab === 'all-dlcs'
          ? 'text-cyan-300'
          : 'text-slate-400 hover:text-white'}"
    >
      <Store class="w-5 h-5 transition-transform group-active:scale-90" />
      <span class="text-[10px] font-bold mt-0.5 tracking-tight">Крамниця</span>
      {#if $uiStore.activeTab === 'store' || $uiStore.activeTab === 'catalog' || $uiStore.activeTab === 'game' || $uiStore.activeTab === 'all-dlcs'}
        <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(13,242,201,0.9)] absolute -top-1"></span>
      {/if}
    </button>

    <button
      type="button"
      onclick={() => navigate('library')}
      class="flex flex-col items-center justify-center py-1 px-3 rounded-xl transition-all cursor-pointer relative group
        {$uiStore.activeTab === 'library'
          ? 'text-cyan-300'
          : 'text-slate-400 hover:text-white'}"
    >
      <Gamepad2 class="w-5 h-5 transition-transform group-active:scale-90" />
      <span class="text-[10px] font-bold mt-0.5 tracking-tight">Бібліотека</span>
      {#if $uiStore.activeTab === 'library'}
        <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(13,242,201,0.9)] absolute -top-1"></span>
      {/if}
    </button>

    <button
      type="button"
      onclick={() => navigate('community')}
      class="flex flex-col items-center justify-center py-1 px-3 rounded-xl transition-all cursor-pointer relative group
        {$uiStore.activeTab === 'community'
          ? 'text-cyan-300'
          : 'text-slate-400 hover:text-white'}"
    >
      <MessageSquare class="w-5 h-5 transition-transform group-active:scale-90" />
      <span class="text-[10px] font-bold mt-0.5 tracking-tight">Спільнота</span>
      {#if $uiStore.activeTab === 'community'}
        <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(13,242,201,0.9)] absolute -top-1"></span>
      {/if}
    </button>

    <button
      type="button"
      onclick={() => navigate('cart')}
      class="flex flex-col items-center justify-center py-1 px-3 rounded-xl transition-all cursor-pointer relative group
        {$uiStore.activeTab === 'cart'
          ? 'text-cyan-300'
          : 'text-slate-400 hover:text-white'}"
    >
      <div class="relative">
        <ShoppingCart class="w-5 h-5 transition-transform group-active:scale-90" />
        {#if cartCount > 0}
          <span class="absolute -top-1.5 -right-2 min-w-[15px] h-[15px] rounded-full bg-cyan-400 text-black font-black text-[9px] flex items-center justify-center px-1 shadow-md">
            {cartCount}
          </span>
        {/if}
      </div>
      <span class="text-[10px] font-bold mt-0.5 tracking-tight">Кошик</span>
      {#if $uiStore.activeTab === 'cart'}
        <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(13,242,201,0.9)] absolute -top-1"></span>
      {/if}
    </button>

    <button
      type="button"
      onclick={() => navigate($currentUser ? 'my-profile' : 'friends')}
      class="flex flex-col items-center justify-center py-1 px-3 rounded-xl transition-all cursor-pointer relative group
        {$uiStore.activeTab === 'friends' || $uiStore.activeTab === 'my-profile' || $uiStore.activeTab === 'profile'
          ? 'text-cyan-300'
          : 'text-slate-400 hover:text-white'}"
    >
      {#if $currentUser}
        <User class="w-5 h-5 transition-transform group-active:scale-90" />
        <span class="text-[10px] font-bold mt-0.5 tracking-tight">Профіль</span>
      {:else}
        <Users class="w-5 h-5 transition-transform group-active:scale-90" />
        <span class="text-[10px] font-bold mt-0.5 tracking-tight">Друзі</span>
      {/if}
      {#if $uiStore.activeTab === 'friends' || $uiStore.activeTab === 'my-profile' || $uiStore.activeTab === 'profile'}
        <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(13,242,201,0.9)] absolute -top-1"></span>
      {/if}
    </button>
  </div>
</nav>
