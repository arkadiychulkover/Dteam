<script lang="ts">
  import { uiStore, type MainTab } from '../../stores/uiStore';
  import { wishlistStore } from '../../stores/wishlistStore';
  import { 
    Gamepad2, 
    Shield, 
    Compass, 
    Heart, 
    ShoppingCart, 
    Settings, 
    Bell,
    Sparkles
  } from 'lucide-svelte';

  const tabs: { id: MainTab; label: string; icon: any }[] = [
    { id: 'store', label: 'Крамниця', icon: Gamepad2 },
    { id: 'catalog', label: 'Каталог', icon: Compass },
    { id: 'admin', label: 'Адмінка', icon: Shield },
  ];
</script>

<header class="sticky top-0 z-40 bg-[#030d12]/95 backdrop-blur-xl border-b border-cyan-950/80 px-4 lg:px-8 py-3 shadow-2xl">
  <div class="max-w-7xl mx-auto flex items-center justify-between gap-4">
    <div class="flex items-center gap-6 lg:gap-8">
      <button 
        onclick={() => uiStore.setTab('store')}
        class="flex items-center gap-3 group cursor-pointer"
      >
        <div class="w-10 h-10 rounded-2xl bg-gradient-to-br from-cyan-400 via-teal-500 to-emerald-400 flex items-center justify-center shadow-lg shadow-cyan-500/25 group-hover:scale-105 group-hover:shadow-cyan-400/40 transition-all">
          <Gamepad2 class="w-5 h-5 text-black font-black" />
        </div>
        <div class="text-left">
          <span class="font-black text-2xl tracking-tighter text-white font-['Outfit'] flex items-center gap-1.5 leading-none">
            DTEAM<span class="text-cyan-400">.</span>
          </span>
          <span class="block text-[8px] font-bold text-cyan-400/90 tracking-widest uppercase mt-0.5">GAMING HUB</span>
        </div>
      </button>

      <nav class="flex items-center gap-1 bg-[#061820]/90 p-1 rounded-2xl border border-cyan-500/20 shadow-inner">
        {#each tabs as tab}
          {@const Icon = tab.icon}
          <button
            onclick={() => uiStore.setTab(tab.id)}
            class="flex items-center gap-2 px-3.5 py-1.5 rounded-xl text-xs font-bold tracking-wide transition-all cursor-pointer relative
              {$uiStore.activeTab === tab.id 
                ? 'bg-gradient-to-r from-cyan-500 to-emerald-500 text-black shadow-lg shadow-cyan-500/25 font-black' 
                : 'text-slate-400 hover:text-white hover:bg-slate-800/40'}"
          >
            <Icon class="w-4 h-4 {$uiStore.activeTab === tab.id ? 'text-black' : tab.id === 'admin' ? 'text-cyan-400' : 'text-slate-400'}" />
            <span>{tab.label}</span>
            {#if tab.id === 'admin'}
              <span class="w-1.5 h-1.5 rounded-full bg-cyan-400 animate-ping absolute top-1.5 right-1.5"></span>
            {/if}
          </button>
        {/each}
      </nav>
    </div>

    <div class="flex items-center gap-2.5">
      <button
        onclick={() => uiStore.setTab('wishlist')}
        class="relative p-2 rounded-xl border transition-all cursor-pointer group
          {$uiStore.activeTab === 'wishlist'
            ? 'bg-cyan-500/20 border-cyan-400 text-rose-400 shadow-md shadow-cyan-500/20'
            : 'bg-[#061820] hover:bg-cyan-950/60 border-cyan-500/20 hover:border-cyan-400 text-slate-300 hover:text-rose-400'}"
        title="Список бажань"
      >
        <Heart class="w-4 h-4 {$wishlistStore.items.length > 0 || $uiStore.activeTab === 'wishlist' ? 'fill-rose-500 text-rose-500' : 'group-hover:scale-110'}" />
        {#if $wishlistStore.items.length > 0}
          <span class="absolute -top-1.5 -right-1.5 min-w-[18px] h-[18px] px-1 rounded-full bg-rose-600 text-white text-[10px] font-bold flex items-center justify-center shadow-md shadow-rose-600/40 animate-in zoom-in">
            {$wishlistStore.items.length}
          </span>
        {/if}
      </button>

      <button
        onclick={() => uiStore.addToast({ title: 'Кошик', message: 'Кошик порожній.', type: 'info' })}
        class="p-2 rounded-xl bg-[#061820] hover:bg-cyan-950/60 border border-cyan-500/20 hover:border-cyan-400 text-slate-300 hover:text-cyan-300 transition-all cursor-pointer relative"
        title="Кошик"
      >
        <ShoppingCart class="w-4 h-4" />
      </button>

      <div class="hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-xl bg-[#061820] border border-cyan-500/30 text-[11px] font-mono text-cyan-300">
        <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
        <span>DTEAM NETWORK</span>
      </div>
    </div>
  </div>
</header>
