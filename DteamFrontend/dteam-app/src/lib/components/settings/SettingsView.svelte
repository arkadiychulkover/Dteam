<script lang="ts">
  import { onMount } from 'svelte';
  import { settingsStore } from '../../stores/settingsStore';
  import SettingsSidebar from './SettingsSidebar.svelte';
  import GeneralSettingsTab from './tabs/GeneralSettingsTab.svelte';
  import PasswordSettingsTab from './tabs/PasswordSettingsTab.svelte';
  import NotificationsSettingsTab from './tabs/NotificationsSettingsTab.svelte';
  import WalletSettingsTab from './tabs/WalletSettingsTab.svelte';
  import DeleteAccountTab from './tabs/DeleteAccountTab.svelte';
  import { Loader2 } from 'lucide-svelte';

  const { uiState } = settingsStore;

  onMount(() => {
    settingsStore.loadSettings();
  });
</script>

<div class="min-h-[calc(100vh-5rem)] bg-gradient-to-b from-[#021822] via-[#02141c] to-[#010e14] py-8 px-4 sm:px-6 lg:px-8">
  <div class="max-w-7xl mx-auto flex flex-col lg:flex-row gap-6 lg:gap-8 items-start">
    <!-- Left Navigation Sidebar -->
    <SettingsSidebar />

    <!-- Right Content Card: starts from the edge, consistent across all tabs -->
    <main class="flex-1 w-full min-w-0 bg-[#03212c]/90 border border-cyan-900/30 rounded-3xl p-6 sm:p-8 backdrop-blur-xl shadow-2xl relative">
      {#if $uiState.isLoading}
        <div class="min-h-[30rem] flex flex-col items-center justify-center text-cyan-400 gap-3">
          <Loader2 class="w-8 h-8 animate-spin" />
          <span class="text-xs font-mono tracking-wider text-slate-400">Завантаження налаштувань...</span>
        </div>
      {:else}
        {#if $uiState.activeTab === 'general'}
          <GeneralSettingsTab />
        {:else if $uiState.activeTab === 'password'}
          <PasswordSettingsTab />
        {:else if $uiState.activeTab === 'notifications'}
          <NotificationsSettingsTab />
        {:else if $uiState.activeTab === 'wallet'}
          <WalletSettingsTab />
        {:else if $uiState.activeTab === 'delete-account'}
          <DeleteAccountTab />
        {/if}
      {/if}
    </main>
  </div>
</div>
