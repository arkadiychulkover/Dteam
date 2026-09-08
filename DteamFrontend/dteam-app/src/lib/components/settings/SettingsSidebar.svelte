<script lang="ts">
  import { settingsStore } from '../../stores/settingsStore';
  import SettingsToggle from './primitives/SettingsToggle.svelte';
  import {
    Search,
    Moon,
    User,
    Lock,
    Bell,
    Wallet,
    UserX,
  } from 'lucide-svelte';
  import type { SettingsTabId } from '../../types/settings';

  const { uiState } = settingsStore;

  let isDarkTheme = $state(true);

  const navItems: { id: SettingsTabId; label: string; icon: any; keywords: string[] }[] = [
    { id: 'general', label: 'Загальні налаштування', icon: User, keywords: ['профіль', 'аватар', 'банер', 'нікнейм', 'пошта', 'мова', 'біо'] },
    { id: 'password', label: 'Пароль', icon: Lock, keywords: ['безпека', 'пароль', 'зміна паролю', 'сесії'] },
    { id: 'notifications', label: 'Сповіщення', icon: Bell, keywords: ['пуші', 'звук', 'чат', 'розпродаж', 'бажане', 'друзі'] },
    { id: 'wallet', label: 'Гаманець', icon: Wallet, keywords: ['баланс', 'транзакції', 'поповнення', 'гроші', 'ton'] },
    { id: 'delete-account', label: 'Видалення акаунта', icon: UserX, keywords: ['видалити', 'деактивація', 'акаунт'] },
  ];

  const filteredNavItems = $derived(
    $uiState.searchQuery.trim()
      ? navItems.filter(item =>
          item.label.toLowerCase().includes($uiState.searchQuery.toLowerCase()) ||
          item.keywords.some(k => k.toLowerCase().includes($uiState.searchQuery.toLowerCase()))
        )
      : navItems
  );
</script>

<aside class="w-full lg:w-72 shrink-0 bg-[#02222d]/80 border border-cyan-900/30 rounded-3xl p-4 backdrop-blur-xl shadow-xl space-y-4">
  <!-- Search Input -->
  <div class="relative">
    <Search class="w-4 h-4 text-cyan-400/80 absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
    <input
      type="text"
      value={$uiState.searchQuery}
      oninput={(e) => settingsStore.setSearchQuery((e.target as HTMLInputElement).value)}
      placeholder="Пошук налаштувань..."
      class="w-full pl-9 pr-3 py-2 rounded-xl bg-[#011c27] border border-cyan-800/40 text-slate-100 placeholder-slate-400 text-xs focus:outline-none focus:border-cyan-400 focus:ring-1 focus:ring-cyan-400 transition-all font-medium"
    />
  </div>

  <!-- Dark Theme Toggle Row (from mockup) -->
  <div class="flex items-center justify-between py-2 px-3 rounded-xl bg-[#011c27]/60 border border-cyan-900/20">
    <div class="flex items-center gap-2.5 text-xs font-bold text-slate-200">
      <Moon class="w-4 h-4 text-cyan-400" />
      <span>Темна тема</span>
    </div>
    <SettingsToggle
      checked={isDarkTheme}
      onchange={(val) => isDarkTheme = val}
      ariaLabel="Перемикач темної теми"
    />
  </div>

  <div class="h-px bg-cyan-900/20 my-1"></div>

  <!-- Navigation items -->
  <nav class="space-y-1">
    {#each filteredNavItems as item}
      {@const Icon = item.icon}
      {@const isActive = $uiState.activeTab === item.id}
      <button
        type="button"
        onclick={() => settingsStore.setTab(item.id)}
        class="w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer text-left group
          {isActive
            ? 'bg-[#005064] text-white shadow-md shadow-cyan-950/40 font-black'
            : 'text-slate-300 hover:text-white hover:bg-cyan-950/30'}"
      >
        <Icon class="w-4 h-4 {isActive ? 'text-cyan-300' : 'text-cyan-500/70 group-hover:text-cyan-300'} shrink-0 transition-colors" />
        <span class="truncate">{item.label}</span>
      </button>
    {/each}

    {#if filteredNavItems.length === 0}
      <div class="py-4 text-center text-slate-400 text-xs font-mono">
        Нічого не знайдено
      </div>
    {/if}
  </nav>
</aside>
